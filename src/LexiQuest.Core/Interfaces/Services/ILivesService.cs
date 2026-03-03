using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing player lives system.
/// </summary>
public interface ILivesService
{
    /// <summary>
    /// Gets the maximum lives for a given difficulty level.
    /// </summary>
    int GetMaxLives(DifficultyLevel difficulty);

    /// <summary>
    /// Gets the current lives status for a user.
    /// </summary>
    Task<LivesStatus> GetLivesStatusAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decrements lives by one. Returns false if no lives remaining.
    /// </summary>
    Task<bool> LoseLifeAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Regenerates one life if not at max and enough time has passed.
    /// </summary>
    Task<bool> RegenerateLifeAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refills lives to maximum.
    /// </summary>
    Task RefillLivesAsync(Guid userId, CancellationToken cancellationToken = default);
}
