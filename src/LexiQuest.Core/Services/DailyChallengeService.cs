using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services;

public class DailyChallengeService : IDailyChallengeService
{
    private readonly IWordRepository _wordRepository;
    private readonly IDailyChallengeRepository _challengeRepository;
    private readonly IGameSessionService _gameSessionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<DailyChallengeService> _localizer;

    public DailyChallengeService(
        IWordRepository wordRepository,
        IDailyChallengeRepository challengeRepository,
        IGameSessionService gameSessionService,
        IUnitOfWork unitOfWork,
        IStringLocalizer<DailyChallengeService> localizer)
    {
        _wordRepository = wordRepository;
        _challengeRepository = challengeRepository;
        _gameSessionService = gameSessionService;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<DailyChallenge?> GetTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _challengeRepository.GetByDateAsync(today, cancellationToken);
    }

    public async Task<DailyChallenge> GetOrCreateTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var existing = await _challengeRepository.GetByDateAsync(today, cancellationToken);
        
        if (existing != null)
            return existing;

        // Create new challenge for today
        var modifier = GetModifierForDay(today.DayOfWeek);
        var difficulty = GetDifficultyForModifier(modifier);
        
        var word = await _wordRepository.GetRandomAsync(difficulty: difficulty, cancellationToken: cancellationToken);
        if (word == null)
            throw new InvalidOperationException(_localizer["Error.NoWordsAvailable"]);

        var challenge = DailyChallenge.Create(today, word.Id, modifier);
        await _challengeRepository.AddAsync(challenge, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return challenge;
    }

    public async Task<ChallengeResultDto> SubmitAnswerAsync(
        Guid userId, 
        DateTime date, 
        string answer, 
        TimeSpan timeTaken,
        CancellationToken cancellationToken = default)
    {
        // Check if already completed
        if (await _challengeRepository.HasUserCompletedAsync(userId, date, cancellationToken))
            throw new InvalidOperationException(_localizer["Error.AlreadyCompleted"]);

        var challenge = await _challengeRepository.GetByDateAsync(date, cancellationToken);
        if (challenge == null)
            throw new InvalidOperationException(_localizer["Error.ChallengeNotFound"]);

        var word = await _wordRepository.GetByIdAsync(challenge.WordId, cancellationToken);
        if (word == null)
            throw new InvalidOperationException(_localizer["Error.WordNotFound"]);

        var isCorrect = word.Original.Equals(answer, StringComparison.OrdinalIgnoreCase);
        var baseXP = isCorrect ? CalculateBaseXP(word.Difficulty) : 0;
        var multiplier = GetXPMultiplier(challenge.Modifier, timeTaken);
        var totalXP = (int)(baseXP * multiplier);

        if (isCorrect)
        {
            var completion = DailyChallengeCompletion.Create(userId, date, timeTaken, totalXP);
            await _challengeRepository.RecordCompletionAsync(completion, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var rank = isCorrect ? await GetRankAsync(userId, date, cancellationToken) : 0;

        return new ChallengeResultDto(
            IsCorrect: isCorrect,
            CorrectAnswer: word.Original,
            XPEarned: totalXP,
            TimeTaken: timeTaken,
            Rank: rank
        );
    }

    public async Task<List<DailyLeaderboardEntry>> GetLeaderboardAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var entries = await _challengeRepository.GetLeaderboardAsync(date, cancellationToken);
        return entries?.OrderBy(e => e.TimeTaken).ToList() ?? new List<DailyLeaderboardEntry>();
    }

    public static DailyModifier GetModifierForDay(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => DailyModifier.Category,
        DayOfWeek.Tuesday => DailyModifier.Speed,
        DayOfWeek.Wednesday => DailyModifier.NoHints,
        DayOfWeek.Thursday => DailyModifier.DoubleLetters,
        DayOfWeek.Friday => DailyModifier.Team,
        DayOfWeek.Saturday => DailyModifier.Hard,
        DayOfWeek.Sunday => DailyModifier.Easy,
        _ => DailyModifier.Easy
    };

    private static DifficultyLevel GetDifficultyForModifier(DailyModifier modifier) => modifier switch
    {
        DailyModifier.Hard => DifficultyLevel.Expert,
        DailyModifier.Easy => DifficultyLevel.Beginner,
        _ => DifficultyLevel.Intermediate
    };

    private static int CalculateBaseXP(DifficultyLevel difficulty) => difficulty switch
    {
        DifficultyLevel.Beginner => 10,
        DifficultyLevel.Intermediate => 20,
        DifficultyLevel.Advanced => 25,
        DifficultyLevel.Expert => 30,
        _ => 20
    };

    private static double GetXPMultiplier(DailyModifier modifier, TimeSpan timeTaken)
    {
        var baseMultiplier = modifier switch
        {
            DailyModifier.Speed => 1.5,
            DailyModifier.Hard => 2.0,
            DailyModifier.NoHints => 1.3,
            DailyModifier.DoubleLetters => 1.4,
            _ => 1.0
        };

        // Speed bonus for fast completion
        if (timeTaken.TotalSeconds < 5)
            baseMultiplier *= 1.5;
        else if (timeTaken.TotalSeconds < 10)
            baseMultiplier *= 1.2;

        return baseMultiplier;
    }

    private async Task<int> GetRankAsync(Guid userId, DateTime date, CancellationToken cancellationToken)
    {
        var leaderboard = await GetLeaderboardAsync(date, cancellationToken);
        var rank = leaderboard.FindIndex(e => e.UserId == userId);
        return rank >= 0 ? rank + 1 : leaderboard.Count + 1;
    }
}
