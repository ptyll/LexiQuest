using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services.BossRules;

/// <summary>
/// Condition boss: 15 words, forbidden letters pattern, penalty for using them.
/// </summary>
public class ConditionBossRules : IBossRules
{
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<ConditionBossRules> _localizer;

    public ConditionBossRules(
        IWordRepository wordRepository,
        IXpCalculator xpCalculator,
        IUnitOfWork unitOfWork,
        IStringLocalizer<ConditionBossRules> localizer)
    {
        _wordRepository = wordRepository;
        _xpCalculator = xpCalculator;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public static (int TotalRounds, int Lives) GetSettings() => (15, 3);

    public bool UsesForbiddenLetters(string answer, string forbiddenLetters)
    {
        var upperAnswer = answer.ToUpperInvariant();
        return forbiddenLetters.Any(forbidden => upperAnswer.Contains(forbidden));
    }

    public int CalculateForbiddenLetterPenalty(string answer, string forbiddenLetters, int baseXp)
    {
        return UsesForbiddenLetters(answer, forbiddenLetters) ? 5 : 0;
    }

    public int CalculateWrongAnswerPenalty() => -5;

    public async Task ProcessWrongAnswerAsync(GameSession session)
    {
        // Condition boss: no life loss, just XP penalty handled separately
        await Task.CompletedTask;
    }

    public int CalculateCompletionBonus(GameSession session, bool perfectRun)
    {
        return perfectRun ? 150 : 75;
    }

    public int CalculateSpeedBonus(TimeSpan duration)
    {
        return duration.TotalMinutes < 4 ? 40 : 0;
    }

    public Task<GameSession> InitializeSessionAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var session = GameSession.CreateBossSession(userId, BossType.Condition, difficulty);
        return Task.FromResult(session);
    }
}
