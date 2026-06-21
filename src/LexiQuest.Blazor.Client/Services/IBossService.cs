using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// Service for managing boss level games on the frontend.
/// </summary>
public interface IBossService
{
    /// <summary>
    /// Starts a new boss game session.
    /// </summary>
    Task<BossSessionDto> StartBossGameAsync(BossType bossType, DifficultyLevel difficulty, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current boss game state.
    /// </summary>
    Task<BossSessionDto?> GetBossStateAsync(Guid sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Submits an answer for the current boss round.
    /// </summary>
    Task<BossRoundResultDto> SubmitAnswerAsync(Guid sessionId, string answer, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the twist boss reveal state (for progressive reveal mechanic).
    /// </summary>
    Task<TwistRevealStateDto?> GetTwistRevealStateAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
