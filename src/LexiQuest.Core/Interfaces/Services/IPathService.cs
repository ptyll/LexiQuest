using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing learning paths.
/// </summary>
public interface IPathService
{
    /// <summary>
    /// Gets all learning paths with progress for a user.
    /// </summary>
    Task<List<LearningPathDto>> GetPathsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a path is unlocked for a user.
    /// </summary>
    Task<bool> IsPathUnlockedAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets progress for a specific path.
    /// </summary>
    Task<PathProgressDto> GetPathProgressAsync(Guid userId, Guid pathId, CancellationToken cancellationToken = default);
}
