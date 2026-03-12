using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services;

/// <summary>
/// Service for managing boss level games.
/// </summary>
public class BossService : IBossService
{
    private readonly IBossRules _marathonRules;
    private readonly IBossRules _conditionRules;
    private readonly IBossRules _twistRules;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<BossService> _localizer;

    public BossService(
        IBossRules marathonRules,
        IBossRules conditionRules,
        IBossRules twistRules,
        IUnitOfWork unitOfWork,
        IStringLocalizer<BossService> localizer)
    {
        _marathonRules = marathonRules;
        _conditionRules = conditionRules;
        _twistRules = twistRules;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    /// <summary>
    /// Starts a new boss game session.
    /// </summary>
    public async Task<GameSession> StartBossGameAsync(Guid userId, BossType bossType, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var rules = GetBossRules(bossType);
        var session = await rules.InitializeSessionAsync(userId, difficulty, cancellationToken);
        return session;
    }

    /// <summary>
    /// Gets the appropriate boss rules implementation for the given boss type.
    /// </summary>
    public IBossRules GetBossRules(BossType bossType)
    {
        return bossType switch
        {
            BossType.Marathon => _marathonRules,
            BossType.Condition => _conditionRules,
            BossType.Twist => _twistRules,
            _ => throw new ArgumentOutOfRangeException(nameof(bossType), bossType, _localizer["Boss.Error.InvalidType"])
        };
    }
}

/// <summary>
/// Interface for boss service operations.
/// </summary>
public interface IBossService
{
    Task<GameSession> StartBossGameAsync(Guid userId, BossType bossType, DifficultyLevel difficulty, CancellationToken cancellationToken = default);
    IBossRules GetBossRules(BossType bossType);
}
