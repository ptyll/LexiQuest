using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services.BossRules;

/// <summary>
/// Marathon boss: 20 words, 3 lives, no regeneration, speed bonus for under 5 minutes.
/// </summary>
public class MarathonBossRules : IBossRules
{
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<MarathonBossRules> _localizer;

    public MarathonBossRules(
        IWordRepository wordRepository,
        IXpCalculator xpCalculator,
        IUnitOfWork unitOfWork,
        IStringLocalizer<MarathonBossRules> localizer)
    {
        _wordRepository = wordRepository;
        _xpCalculator = xpCalculator;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public static (int TotalRounds, int Lives) GetSettings() => (20, 3);

    public async Task ProcessWrongAnswerAsync(GameSession session)
    {
        session.LoseLife();
        await Task.CompletedTask;
    }

    public int CalculateCompletionBonus(GameSession session, bool perfectRun)
    {
        return perfectRun ? 200 : 100;
    }

    public int CalculateSpeedBonus(TimeSpan duration)
    {
        return duration.TotalMinutes < 5 ? 50 : 0;
    }

    public Task<GameSession> InitializeSessionAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var session = GameSession.CreateBossSession(userId, BossType.Marathon, difficulty);
        return Task.FromResult(session);
    }
}
