using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services.BossRules;

/// <summary>
/// Twist boss: 12 words, letters reveal over time, bonus for early guessing.
/// </summary>
public class TwistBossRules : IBossRules
{
    public static readonly TimeSpan RevealInterval = TimeSpan.FromSeconds(3);
    
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<TwistBossRules> _localizer;

    public TwistBossRules(
        IWordRepository wordRepository,
        IXpCalculator xpCalculator,
        IUnitOfWork unitOfWork,
        IStringLocalizer<TwistBossRules> localizer)
    {
        _wordRepository = wordRepository;
        _xpCalculator = xpCalculator;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public static (int TotalRounds, int Lives) GetSettings() => (12, 3);

    public int CalculateEarlyGuessBonus(int revealedCount, int remainingCount)
    {
        // Table: 2 revealed=10XP, 3=7XP, 4=5XP, 5+=2XP
        return revealedCount switch
        {
            2 => 10,
            3 => 7,
            4 => 5,
            >= 5 => 2,
            _ => 0
        };
    }

    public int CalculateRevealedLetters(int wordLength, List<int> revealedPositions, TimeSpan elapsed, TimeSpan interval)
    {
        var intervalsPassed = (int)(elapsed.TotalSeconds / interval.TotalSeconds);
        var additionalReveals = Math.Min(intervalsPassed, wordLength - revealedPositions.Count);
        return revealedPositions.Count + additionalReveals;
    }

    public int CalculateWrongAnswerPenalty() => -3;

    public async Task ProcessWrongAnswerAsync(GameSession session)
    {
        // Twist boss: no life loss, just XP penalty handled separately
        await Task.CompletedTask;
    }

    public int CalculateCompletionBonus(GameSession session, bool perfectRun)
    {
        return perfectRun ? 180 : 90;
    }

    public int CalculateSpeedBonus(TimeSpan duration)
    {
        return duration.TotalMinutes < 3 ? 60 : 0;
    }

    public Task<GameSession> InitializeSessionAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var session = GameSession.CreateBossSession(userId, BossType.Twist, difficulty);
        return Task.FromResult(session);
    }
}
