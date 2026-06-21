using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Services;

public class BossGameService : IBossGameService
{
    private static readonly TimeSpan TwistRevealInterval = TimeSpan.FromSeconds(3);
    private const int BossCoinReward = 50;

    private readonly LexiQuestDbContext _context;
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly ILevelCalculator _levelCalculator;
    private readonly TimeProvider _timeProvider;
    private readonly Random _rng = new();

    public BossGameService(
        LexiQuestDbContext context,
        IWordRepository wordRepository,
        IXpCalculator xpCalculator,
        ILevelCalculator levelCalculator,
        TimeProvider timeProvider)
    {
        _context = context;
        _wordRepository = wordRepository;
        _xpCalculator = xpCalculator;
        _levelCalculator = levelCalculator;
        _timeProvider = timeProvider;
    }

    public async Task<BossSessionDto> StartBossGameAsync(
        Guid userId,
        BossStartRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.BossType))
        {
            throw new InvalidOperationException("Invalid boss type");
        }

        var now = GetUtcNow();
        var session = GameSession.CreateBossSession(userId, request.BossType, request.Difficulty, now);
        _context.GameSessions.Add(session);

        var words = await _wordRepository.GetRandomBatchAsync(
            session.TotalRounds,
            request.Difficulty,
            null,
            cancellationToken);

        if (words.Count == 0)
        {
            throw new InvalidOperationException("No words available for the selected difficulty");
        }

        session.SetWordIds(words.Select(word => word.Id).ToList());
        var firstRound = CreateRound(session, words[0], 1, now);
        _context.GameRounds.Add(firstRound);

        await _context.SaveChangesAsync(cancellationToken);
        return ToSessionDto(session, firstRound);
    }

    public async Task<BossSessionDto?> GetBossStateAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.Mode == GameMode.Boss, cancellationToken);

        if (session == null)
        {
            return null;
        }

        var round = await GetDisplayRoundAsync(session.Id, cancellationToken);
        return round == null ? null : ToSessionDto(session, round);
    }

    public async Task<BossRoundResultDto> SubmitAnswerAsync(
        Guid userId,
        Guid sessionId,
        BossAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.Mode == GameMode.Boss, cancellationToken);

        if (session == null)
        {
            throw new InvalidOperationException("Boss session not found");
        }

        if (session.Status != GameSessionStatus.InProgress)
        {
            throw new InvalidOperationException("Boss session is not in progress");
        }

        var round = await GetActiveRoundAsync(session.Id, cancellationToken);
        if (round == null)
        {
            throw new InvalidOperationException("No active boss round found");
        }

        if (session.BossType == BossType.Twist)
        {
            UpdateTwistRevealState(round, saveChanges: false);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        var answer = request.Answer.Trim();
        var isCorrect = answer.Equals(round.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
        var now = GetUtcNow();

        user?.Stats.UpdateAccuracy(isCorrect);
        user?.Stats.UpdateAverageResponseTime(TimeSpan.FromMilliseconds(Math.Max(0, request.TimeSpentMs)));

        var xpGained = 0;
        var baseXp = 0;
        var speedBonus = 0;
        var completionBonus = 0;
        var perfectBonus = 0;
        var forbiddenPenalty = 0;
        var earlyGuessBonus = 0;

        if (isCorrect)
        {
            session.RecordCorrectAnswer();

            var xpResult = _xpCalculator.CalculateCorrectAnswer(
                Math.Max(0, request.TimeSpentMs),
                session.ComboCount,
                session.CorrectAnswers);

            baseXp = xpResult.TotalXP;
            speedBonus = xpResult.SpeedBonus;
            xpGained = baseXp;

            if (session.BossType == BossType.Condition && round.ContainsForbiddenLetter(answer))
            {
                forbiddenPenalty = 5;
                xpGained = Math.Max(0, xpGained - forbiddenPenalty);
            }

            if (session.BossType == BossType.Twist)
            {
                earlyGuessBonus = CalculateEarlyGuessBonus(round.RevealedLettersCount);
                xpGained += earlyGuessBonus;
            }

            AddXp(session, user, xpGained);
        }
        else
        {
            session.RecordWrongAnswer(loseLife: session.BossType == BossType.Marathon, endedAt: now);
        }

        round.RecordAttempt(answer, isCorrect, Math.Max(0, request.TimeSpentMs));
        round.SetXPEarned(xpGained);

        var reachedFinalRound = session.CurrentRound >= session.TotalRounds;
        if ((isCorrect || session.BossType != BossType.Marathon || !session.IsGameOver) && reachedFinalRound)
        {
            var bonuses = CalculateCompletionBonuses(session, now);
            completionBonus = bonuses.CompletionBonus;
            speedBonus += bonuses.SpeedBonus;
            perfectBonus = bonuses.PerfectBonus;

            var totalBonus = completionBonus + bonuses.SpeedBonus + perfectBonus;
            AddXp(session, user, totalBonus);
            xpGained += totalBonus;
            session.Complete(now);
            user?.AddCoinTransaction(
                BossCoinReward,
                CoinTransactionType.BossLevel.ToString(),
                "Dokončení boss levelu");

            await _context.SaveChangesAsync(cancellationToken);
            return ToRoundResult(session, round, xpGained, baseXp, completionBonus, speedBonus, perfectBonus, forbiddenPenalty, earlyGuessBonus);
        }

        if (session.IsGameOver)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return ToRoundResult(session, round, xpGained, baseXp, completionBonus, speedBonus, perfectBonus, forbiddenPenalty, earlyGuessBonus);
        }

        var nextRound = await GenerateNextRoundAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToRoundResult(
            session,
            round,
            xpGained,
            baseXp,
            completionBonus,
            speedBonus,
            perfectBonus,
            forbiddenPenalty,
            earlyGuessBonus,
            nextRound);
    }

    public async Task<TwistRevealStateDto?> GetTwistRevealStateAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.Mode == GameMode.Boss, cancellationToken);

        if (session?.BossType != BossType.Twist || session.Status != GameSessionStatus.InProgress)
        {
            return null;
        }

        var round = await GetActiveRoundAsync(session.Id, cancellationToken);
        if (round == null)
        {
            return null;
        }

        var state = UpdateTwistRevealState(round, saveChanges: true);
        await _context.SaveChangesAsync(cancellationToken);
        return state;
    }

    private async Task<GameRound> GenerateNextRoundAsync(GameSession session, CancellationToken cancellationToken)
    {
        var word = await _wordRepository.GetRandomAsync(session.Difficulty, null, cancellationToken)
            ?? throw new InvalidOperationException("No words available for the selected difficulty");

        var roundNumber = session.CurrentRound + 1;
        var round = CreateRound(session, word, roundNumber, GetUtcNow());
        _context.GameRounds.Add(round);
        session.AdvanceToNextRound();

        return round;
    }

    private GameRound CreateRound(GameSession session, Word word, int roundNumber, DateTime startedAt)
    {
        var round = GameRound.Create(
            sessionId: session.Id,
            roundNumber: roundNumber,
            wordId: word.Id,
            scrambledWord: word.Scramble(_rng),
            correctAnswer: word.Original,
            timeLimitSeconds: GetTimeLimitSeconds(session.BossType),
            startedAt: startedAt);

        if (session.BossType == BossType.Condition && roundNumber % 3 == 0 && !string.IsNullOrWhiteSpace(session.ForbiddenLetters))
        {
            round.SetForbiddenLetters(session.ForbiddenLetters);
        }

        if (session.BossType == BossType.Twist)
        {
            round.SetRevealedPositions(GetInitialRevealedPositions(word.Length));
        }

        return round;
    }

    private TwistRevealStateDto UpdateTwistRevealState(GameRound round, bool saveChanges)
    {
        var now = GetUtcNow();
        var startedAt = round.StartedAt ?? now;
        var elapsed = now - startedAt;
        var initial = Math.Min(2, round.CorrectAnswer.Length);
        var intervalsPassed = Math.Max(0, (int)(elapsed.TotalMilliseconds / TwistRevealInterval.TotalMilliseconds));
        var revealedCount = Math.Min(round.CorrectAnswer.Length, initial + intervalsPassed);
        var positions = Enumerable.Range(0, revealedCount).ToArray();

        if (saveChanges || round.RevealedLettersCount != revealedCount)
        {
            round.SetRevealedPositions(positions);
        }

        var timeUntilNextReveal = TimeSpan.Zero;
        if (revealedCount < round.CorrectAnswer.Length)
        {
            var elapsedInCurrentInterval = elapsed.TotalMilliseconds % TwistRevealInterval.TotalMilliseconds;
            timeUntilNextReveal = TimeSpan.FromMilliseconds(TwistRevealInterval.TotalMilliseconds - elapsedInCurrentInterval);
        }

        return new TwistRevealStateDto
        {
            RevealedPositions = positions.ToList(),
            RevealedLetters = GetRevealedLetters(round, positions),
            NextRevealAt = now.Add(timeUntilNextReveal),
            TimeUntilNextReveal = timeUntilNextReveal,
            CurrentBonusXP = CalculateEarlyGuessBonus(revealedCount),
            RevealedLettersCount = revealedCount
        };
    }

    private async Task<GameRound?> GetActiveRoundAsync(Guid sessionId, CancellationToken cancellationToken) =>
        await _context.GameRounds
            .Where(r => r.SessionId == sessionId && !r.IsCompleted)
            .OrderByDescending(r => r.RoundNumber)
            .FirstOrDefaultAsync(cancellationToken);

    private async Task<GameRound?> GetDisplayRoundAsync(Guid sessionId, CancellationToken cancellationToken) =>
        await _context.GameRounds
            .Where(r => r.SessionId == sessionId && !r.IsCompleted)
            .OrderByDescending(r => r.RoundNumber)
            .FirstOrDefaultAsync(cancellationToken)
        ?? await _context.GameRounds
            .Where(r => r.SessionId == sessionId)
            .OrderByDescending(r => r.RoundNumber)
            .FirstOrDefaultAsync(cancellationToken);

    private BossSessionDto ToSessionDto(GameSession session, GameRound round)
    {
        var revealedPositions = ParsePositions(round.RevealedPositions);
        var timeUntilReveal = session.BossType == BossType.Twist && !round.IsCompleted
            ? CalculateTimeUntilNextReveal(round)
            : null;

        return new BossSessionDto
        {
            Id = session.Id,
            BossType = session.BossType ?? BossType.Marathon,
            CurrentRound = session.CurrentRound,
            TotalRounds = session.TotalRounds,
            LivesRemaining = session.LivesRemaining,
            Mode = session.Mode,
            Difficulty = session.Difficulty,
            IsGameOver = session.IsGameOver,
            IsCompleted = session.Status == GameSessionStatus.Completed,
            TotalXP = session.TotalXP,
            CorrectAnswers = session.CorrectAnswers,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            CurrentScrambledWord = round.IsCompleted ? string.Empty : round.ScrambledWord,
            WordLength = round.CorrectAnswer.Length,
            ForbiddenLetters = round.ForbiddenLetters,
            RevealedLettersCount = round.RevealedLettersCount,
            RevealedPositions = revealedPositions,
            RevealedLetters = GetRevealedLetters(round, revealedPositions),
            TimeUntilNextReveal = timeUntilReveal,
            CurrentEarlyGuessBonus = CalculateEarlyGuessBonus(round.RevealedLettersCount)
        };
    }

    private BossRoundResultDto ToRoundResult(
        GameSession session,
        GameRound round,
        int xpGained,
        int baseXp,
        int completionBonus,
        int speedBonus,
        int perfectBonus,
        int forbiddenPenalty,
        int earlyGuessBonus,
        GameRound? nextRound = null)
    {
        return new BossRoundResultDto
        {
            IsCorrect = round.IsCorrect,
            CorrectAnswer = round.CorrectAnswer,
            XPGained = xpGained,
            BaseXP = baseXp,
            BonusXP = completionBonus + perfectBonus + earlyGuessBonus,
            CompletionBonus = completionBonus,
            SpeedBonus = speedBonus,
            PerfectBonus = perfectBonus,
            ForbiddenLetterPenalty = forbiddenPenalty > 0 ? "-5 XP" : null,
            ForbiddenLetterPenaltyXP = forbiddenPenalty,
            EarlyGuessBonus = earlyGuessBonus,
            LivesRemaining = session.LivesRemaining,
            CurrentRound = session.CurrentRound,
            TotalRounds = session.TotalRounds,
            TotalXP = session.TotalXP,
            IsGameOver = session.IsGameOver,
            IsCompleted = session.Status == GameSessionStatus.Completed,
            NextScrambledWord = nextRound?.ScrambledWord,
            NextRoundNumber = nextRound?.RoundNumber,
            WordLength = nextRound?.CorrectAnswer.Length,
            ForbiddenLetters = nextRound?.ForbiddenLetters,
            RevealedLettersCount = nextRound?.RevealedLettersCount ?? 0,
            RevealedPositions = nextRound == null ? null : ParsePositions(nextRound.RevealedPositions),
            RevealedLetters = nextRound == null ? null : GetRevealedLetters(nextRound, ParsePositions(nextRound.RevealedPositions)),
            EndedAt = session.EndedAt
        };
    }

    private (int CompletionBonus, int SpeedBonus, int PerfectBonus) CalculateCompletionBonuses(GameSession session, DateTime now)
    {
        var duration = now - session.StartedAt;
        var perfectRun = session.LivesRemaining == 3 && session.CorrectAnswers == session.TotalRounds;

        return session.BossType switch
        {
            BossType.Marathon => (
                CompletionBonus: perfectRun ? 0 : 100,
                SpeedBonus: duration.TotalMinutes < 5 ? 50 : 0,
                PerfectBonus: perfectRun ? 200 : 0),
            BossType.Condition => (
                CompletionBonus: perfectRun ? 150 : 75,
                SpeedBonus: duration.TotalMinutes < 4 ? 40 : 0,
                PerfectBonus: 0),
            BossType.Twist => (
                CompletionBonus: perfectRun ? 180 : 90,
                SpeedBonus: duration.TotalMinutes < 3 ? 60 : 0,
                PerfectBonus: 0),
            _ => (CompletionBonus: 0, SpeedBonus: 0, PerfectBonus: 0)
        };
    }

    private void AddXp(GameSession session, User? user, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        session.AddXP(amount);

        if (user == null)
        {
            return;
        }

        var previousLevel = _levelCalculator.GetLevelFromXp(user.Stats.TotalXP);
        user.Stats.AddXP(amount);
        var newLevel = _levelCalculator.GetLevelFromXp(user.Stats.TotalXP);

        if (newLevel > previousLevel)
        {
            typeof(UserStats)
                .GetProperty(nameof(UserStats.Level))
                ?.SetValue(user.Stats, newLevel);
        }
    }

    private TimeSpan? CalculateTimeUntilNextReveal(GameRound round)
    {
        if (round.RevealedLettersCount >= round.CorrectAnswer.Length)
        {
            return TimeSpan.Zero;
        }

        var now = GetUtcNow();
        var elapsed = now - (round.StartedAt ?? now);
        var elapsedInCurrentInterval = elapsed.TotalMilliseconds % TwistRevealInterval.TotalMilliseconds;
        return TimeSpan.FromMilliseconds(TwistRevealInterval.TotalMilliseconds - elapsedInCurrentInterval);
    }

    private static int[] GetInitialRevealedPositions(int wordLength) =>
        Enumerable.Range(0, Math.Min(2, wordLength)).ToArray();

    private static int CalculateEarlyGuessBonus(int revealedCount) =>
        revealedCount switch
        {
            2 => 10,
            3 => 7,
            4 => 5,
            >= 5 => 2,
            _ => 0
        };

    private static int GetTimeLimitSeconds(BossType? bossType) =>
        bossType switch
        {
            BossType.Marathon => 30,
            BossType.Condition => 30,
            BossType.Twist => 30,
            _ => 30
        };

    private static List<int> ParsePositions(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(position => int.TryParse(position, out var parsed) ? parsed : -1)
                .Where(position => position >= 0)
                .ToList();

    private static List<RevealedLetterDto> GetRevealedLetters(GameRound round, IEnumerable<int> positions) =>
        positions
            .Where(position => position >= 0 && position < round.CorrectAnswer.Length)
            .Select(position => new RevealedLetterDto(position, round.CorrectAnswer[position].ToString()))
            .ToList();

    private DateTime GetUtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
