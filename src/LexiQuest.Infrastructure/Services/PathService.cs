using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Services;

/// <summary>
/// Service for managing learning paths.
/// </summary>
public class PathService : IPathService
{
    private readonly LexiQuestDbContext _context;

    public PathService(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<List<LearningPathDto>> GetPathsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var paths = await _context.LearningPaths
            .Include(p => p.Levels)
            .ToListAsync(cancellationToken);

        var result = new List<LearningPathDto>();

        foreach (var path in paths)
        {
            var isUnlocked = await IsPathUnlockedAsync(userId, path.Difficulty, cancellationToken);
            var completedLevels = path.Levels.Count(l => l.Status == LevelStatus.Completed || l.Status == LevelStatus.Perfect);
            var progressPercentage = path.TotalLevels > 0 ? (double)completedLevels / path.TotalLevels * 100 : 0;

            result.Add(new LearningPathDto(
                Id: path.Id,
                Name: path.Name,
                Description: path.Description,
                Difficulty: path.Difficulty,
                TotalLevels: path.TotalLevels,
                CompletedLevels: completedLevels,
                IsUnlocked: isUnlocked,
                ProgressPercentage: Math.Round(progressPercentage, 1)
            ));
        }

        return result;
    }

    public Task<bool> IsPathUnlockedAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        // Beginner path is always unlocked
        if (difficulty == DifficultyLevel.Beginner)
            return Task.FromResult(true);

        // TODO: Check user progress for other paths
        // Intermediate: requires Level 5 or Path 1 complete
        // Advanced: requires Path 2 complete
        // Expert: requires Path 3 complete

        return Task.FromResult(difficulty == DifficultyLevel.Beginner);
    }

    public async Task<PathProgressDto> GetPathProgressAsync(Guid userId, Guid pathId, CancellationToken cancellationToken = default)
    {
        var path = await _context.LearningPaths
            .Include(p => p.Levels)
            .FirstOrDefaultAsync(p => p.Id == pathId, cancellationToken);

        if (path == null)
            throw new InvalidOperationException("Path not found");

        var completedLevels = path.Levels.Count(l => l.Status == LevelStatus.Completed || l.Status == LevelStatus.Perfect);
        var currentLevel = path.Levels
            .Where(l => l.Status == LevelStatus.Current || l.Status == LevelStatus.Available)
            .OrderBy(l => l.LevelNumber)
            .FirstOrDefault()?.LevelNumber ?? 1;

        var levelDtos = path.Levels
            .OrderBy(l => l.LevelNumber)
            .Select(l => new PathLevelDto(
                Id: l.Id,
                LevelNumber: l.LevelNumber,
                Status: l.Status.ToString(),
                IsBoss: l.IsBoss,
                IsPerfect: l.IsPerfect
            ))
            .ToList();

        return new PathProgressDto(
            PathId: path.Id,
            TotalLevels: path.TotalLevels,
            CompletedLevels: completedLevels,
            CurrentLevel: currentLevel,
            Levels: levelDtos
        );
    }
}
