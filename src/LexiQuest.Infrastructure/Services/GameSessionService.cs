using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Services;

/// <summary>
/// Service for managing game sessions.
/// </summary>
public class GameSessionService : IGameSessionService
{
    private readonly LexiQuestDbContext _context;
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly Random _rng = new();

    // Default game settings
    private const int DefaultRoundCount = 10;
    private const int DefaultTimeLimitSeconds = 30;
    private const int DefaultLives = 5;

    public GameSessionService(
        LexiQuestDbContext context,
        IWordRepository wordRepository,
        IXpCalculator xpCalculator)
    {
        _context = context;
        _wordRepository = wordRepository;
        _xpCalculator = xpCalculator;
    }

    /// <inheritdoc />
    public async Task<ScrambledWordDto> StartGameAsync(Guid userId, StartGameRequest request, CancellationToken cancellationToken = default)
    {
        // Determine difficulty
        var difficulty = request.Difficulty ?? DifficultyLevel.Beginner;

        // Create game session
        var session = GameSession.Create(
            userId: userId,
            mode: request.Mode,
            difficulty: difficulty,
            totalRounds: DefaultRoundCount,
            lives: DefaultLives
        );

        _context.GameSessions.Add(session);

        // Get random words for the game
        var words = await _wordRepository.GetRandomBatchAsync(DefaultRoundCount, difficulty, null, cancellationToken);
        if (words.Count == 0)
        {
            throw new InvalidOperationException("No words available for the selected difficulty");
        }

        // Store word IDs in session for later use
        session.SetWordIds(words.Select(w => w.Id).ToList());

        // Create first round
        var firstWord = words[0];
        var scrambled = firstWord.Scramble(_rng);

        var round = GameRound.Create(
            sessionId: session.Id,
            roundNumber: 1,
            wordId: firstWord.Id,
            scrambledWord: scrambled,
            correctAnswer: firstWord.Original,
            timeLimitSeconds: DefaultTimeLimitSeconds
        );

        _context.GameRounds.Add(round);
        await _context.SaveChangesAsync(cancellationToken);

        return new ScrambledWordDto(
            SessionId: session.Id,
            RoundNumber: 1,
            ScrambledWord: scrambled,
            WordLength: firstWord.Length,
            Difficulty: difficulty,
            TimeLimitSeconds: DefaultTimeLimitSeconds,
            TotalRounds: DefaultRoundCount,
            LivesRemaining: session.LivesRemaining
        );
    }

    /// <inheritdoc />
    public async Task<GameRoundResult> SubmitAnswerAsync(Guid userId, SubmitAnswerRequest request, CancellationToken cancellationToken = default)
    {
        // Get session and validate ownership
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == userId, cancellationToken);

        if (session == null)
        {
            throw new InvalidOperationException("Game session not found");
        }

        if (session.Status != GameSessionStatus.InProgress)
        {
            throw new InvalidOperationException("Game session is not in progress");
        }

        // Get current round
        var currentRound = await _context.GameRounds
            .Where(r => r.SessionId == session.Id && !r.IsCompleted)
            .OrderByDescending(r => r.RoundNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentRound == null)
        {
            throw new InvalidOperationException("No active round found");
        }

        // Validate answer
        var userAnswer = request.Answer?.Trim() ?? string.Empty;
        var isCorrect = userAnswer.Equals(currentRound.CorrectAnswer, StringComparison.OrdinalIgnoreCase);

        // Record the attempt
        currentRound.RecordAttempt(userAnswer, isCorrect, request.TimeSpentMs);

        int xpEarned = 0;
        int speedBonus = 0;

        if (isCorrect)
        {
            // Update session stats
            session.RecordCorrectAnswer();

            // Calculate XP
            var xpResult = _xpCalculator.CalculateCorrectAnswer(
                request.TimeSpentMs,
                session.ComboCount,
                session.CorrectAnswers
            );

            xpEarned = xpResult.TotalXP;
            speedBonus = xpResult.SpeedBonus;
            session.AddXP(xpEarned);

            // Check if game is complete
            bool isLevelComplete = session.CurrentRound >= session.TotalRounds;

            if (isLevelComplete)
            {
                session.Complete();
                await _context.SaveChangesAsync(cancellationToken);

                return new GameRoundResult(
                    IsCorrect: true,
                    CorrectAnswer: currentRound.CorrectAnswer,
                    XPEarned: xpEarned,
                    SpeedBonus: speedBonus,
                    ComboCount: session.ComboCount,
                    IsLevelComplete: true,
                    LivesRemaining: session.LivesRemaining,
                    NextScrambledWord: null,
                    NextRoundNumber: null,
                    IsGameOver: false
                );
            }

            // Generate next round
            try
            {
                var nextRoundResult = await GenerateNextRoundAsync(session, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return new GameRoundResult(
                    IsCorrect: true,
                    CorrectAnswer: currentRound.CorrectAnswer,
                    XPEarned: xpEarned,
                    SpeedBonus: speedBonus,
                    ComboCount: session.ComboCount,
                    IsLevelComplete: false,
                    LivesRemaining: session.LivesRemaining,
                    NextScrambledWord: nextRoundResult.ScrambledWord,
                    NextRoundNumber: nextRoundResult.RoundNumber,
                    IsGameOver: false
                );
            }
            catch (ArgumentOutOfRangeException)
            {
                // Ran out of words - complete the game
                session.Complete();
                await _context.SaveChangesAsync(cancellationToken);

                return new GameRoundResult(
                    IsCorrect: true,
                    CorrectAnswer: currentRound.CorrectAnswer,
                    XPEarned: xpEarned,
                    SpeedBonus: speedBonus,
                    ComboCount: session.ComboCount,
                    IsLevelComplete: true,
                    LivesRemaining: session.LivesRemaining,
                    NextScrambledWord: null,
                    NextRoundNumber: null,
                    IsGameOver: false
                );
            }
        }
        else
        {
            // Wrong answer
            session.RecordWrongAnswer();
            
            bool isGameOver = session.LivesRemaining <= 0;
            await _context.SaveChangesAsync(cancellationToken);

            return new GameRoundResult(
                IsCorrect: false,
                CorrectAnswer: currentRound.CorrectAnswer,
                XPEarned: 0,
                SpeedBonus: 0,
                ComboCount: 0, // Reset combo
                IsLevelComplete: false,
                LivesRemaining: session.LivesRemaining,
                NextScrambledWord: null,
                NextRoundNumber: null,
                IsGameOver: isGameOver
            );
        }
    }

    /// <inheritdoc />
    public async Task<ScrambledWordDto?> GetSessionStateAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null || session.Status != GameSessionStatus.InProgress)
        {
            return null;
        }

        var currentRound = await _context.GameRounds
            .Where(r => r.SessionId == session.Id && !r.IsCompleted)
            .OrderByDescending(r => r.RoundNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentRound == null)
        {
            return null;
        }

        return new ScrambledWordDto(
            SessionId: session.Id,
            RoundNumber: currentRound.RoundNumber,
            ScrambledWord: currentRound.ScrambledWord,
            WordLength: currentRound.CorrectAnswer.Length,
            Difficulty: session.Difficulty,
            TimeLimitSeconds: currentRound.TimeLimitSeconds,
            TotalRounds: session.TotalRounds,
            LivesRemaining: session.LivesRemaining
        );
    }

    /// <inheritdoc />
    public async Task<bool> ForfeitGameAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session == null || session.Status != GameSessionStatus.InProgress)
        {
            return false;
        }

        session.Forfeit();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<GameRound> GenerateNextRoundAsync(GameSession session, CancellationToken cancellationToken)
    {
        var nextRoundNumber = session.CurrentRound + 1;

        // Get the word for this round
        var wordId = session.GetWordIdForRound(nextRoundNumber);
        var word = await _wordRepository.GetByIdAsync(wordId, cancellationToken);

        if (word == null)
        {
            throw new InvalidOperationException($"Word not found for round {nextRoundNumber}");
        }

        var scrambled = word.Scramble(_rng);

        var round = GameRound.Create(
            sessionId: session.Id,
            roundNumber: nextRoundNumber,
            wordId: word.Id,
            scrambledWord: scrambled,
            correctAnswer: word.Original,
            timeLimitSeconds: DefaultTimeLimitSeconds
        );

        _context.GameRounds.Add(round);
        session.AdvanceToNextRound();

        return round;
    }
}
