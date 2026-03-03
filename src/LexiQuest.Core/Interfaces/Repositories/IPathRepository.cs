using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository for learning paths.
/// </summary>
public interface IPathRepository
{
    /// <summary>
    /// Gets all learning paths.
    /// </summary>
    Task<List<LearningPath>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a path by ID.
    /// </summary>
    Task<LearningPath?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the count of completed levels for a user in a path.
    /// </summary>
    Task<int> GetCompletedLevelsCountAsync(Guid userId, Guid pathId, CancellationToken cancellationToken = default);
}
