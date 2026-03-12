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

/// <summary>
/// Result of a boss round submission.
/// </summary>
public class BossRoundResultDto
{
    public bool IsCorrect { get; set; }
    public int XPGained { get; set; }
    public int LivesRemaining { get; set; }
    public bool IsGameOver { get; set; }
    public bool IsCompleted { get; set; }
    public string? NextScrambledWord { get; set; }
    public int? BonusXP { get; set; }
    public string? ForbiddenLetterPenalty { get; set; }
    public int? EarlyGuessBonus { get; set; }
}

/// <summary>
/// Twist boss reveal state DTO.
/// </summary>
public class TwistRevealStateDto
{
    public List<int> RevealedPositions { get; set; } = [];
    public DateTime NextRevealAt { get; set; }
    public TimeSpan TimeUntilNextReveal { get; set; }
    public int CurrentBonusXP { get; set; }
}
