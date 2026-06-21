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
            .OrderBy(p => p.Difficulty)
            .ToListAsync(cancellationToken);

        var result = new List<LearningPathDto>();

        foreach (var path in paths)
        {
            var isUnlocked = await IsPathUnlockedAsync(userId, path.Difficulty, cancellationToken);
            var completedLevels = await GetCompletedLevelsCountAsync(userId, path.Id, cancellationToken);
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
        return IsPathUnlockedInternalAsync(userId, difficulty, cancellationToken);
    }

    public async Task<PathProgressDto> GetPathProgressAsync(Guid userId, Guid pathId, CancellationToken cancellationToken = default)
    {
        var path = await _context.LearningPaths
            .Include(p => p.Levels)
            .FirstOrDefaultAsync(p => p.Id == pathId, cancellationToken);

        if (path == null)
            throw new InvalidOperationException("Path not found");

        var progress = await _context.UserPathLevelProgresses
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.PathId == pathId)
            .ToListAsync(cancellationToken);

        var progressByLevel = progress.ToDictionary(p => p.LevelNumber);
        var completedLevels = progress.Count(IsCompletedProgress);
        var currentLevel = Math.Min(completedLevels + 1, path.TotalLevels);

        var levelDtos = path.Levels
            .OrderBy(l => l.LevelNumber)
            .Select(l => new PathLevelDto(
                Id: l.Id,
                LevelNumber: l.LevelNumber,
                Status: GetLevelStatus(l, currentLevel, progressByLevel),
                IsBoss: l.IsBoss,
                IsPerfect: progressByLevel.TryGetValue(l.LevelNumber, out var levelProgress)
                    ? levelProgress.IsPerfect
                    : false,
                WordCount: GetWordCount(l),
                WordLengthMin: path.WordLengthMin,
                WordLengthMax: path.WordLengthMax,
                TimePerWordSeconds: GetTimePerWordSeconds(path, l),
                HintCount: GetHintCount(l),
                Lives: GetLives(path.Difficulty, l),
                XpReward: GetXpReward(l)
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

    private async Task<bool> IsPathUnlockedInternalAsync(
        Guid userId,
        DifficultyLevel difficulty,
        CancellationToken cancellationToken)
    {
        if (difficulty == DifficultyLevel.Beginner)
        {
            return true;
        }

        var userLevel = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Stats.Level)
            .FirstOrDefaultAsync(cancellationToken);

        if (userLevel == 0)
        {
            throw new InvalidOperationException("User not found");
        }

        return difficulty switch
        {
            DifficultyLevel.Intermediate => userLevel >= 5 || await IsPathCompleteAsync(userId, DifficultyLevel.Beginner, cancellationToken),
            DifficultyLevel.Advanced => await IsPathCompleteAsync(userId, DifficultyLevel.Intermediate, cancellationToken),
            DifficultyLevel.Expert => await IsPathCompleteAsync(userId, DifficultyLevel.Advanced, cancellationToken),
            _ => false
        };
    }

    private async Task<bool> IsPathCompleteAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken)
    {
        var path = await _context.LearningPaths
            .Include(p => p.Levels)
            .FirstOrDefaultAsync(p => p.Difficulty == difficulty, cancellationToken);

        return path != null
            && path.TotalLevels > 0
            && await GetCompletedLevelsCountAsync(userId, path.Id, cancellationToken) >= path.TotalLevels;
    }

    private async Task<int> GetCompletedLevelsCountAsync(Guid userId, Guid pathId, CancellationToken cancellationToken)
    {
        return await _context.UserPathLevelProgresses
            .AsNoTracking()
            .CountAsync(
                progress => progress.UserId == userId
                    && progress.PathId == pathId
                    && (progress.Status == LevelStatus.Completed || progress.Status == LevelStatus.Perfect),
                cancellationToken);
    }

    private static bool IsCompletedProgress(UserPathLevelProgress progress) =>
        progress.Status is LevelStatus.Completed or LevelStatus.Perfect;

    private static string GetLevelStatus(
        PathLevel level,
        int currentLevel,
        IReadOnlyDictionary<int, UserPathLevelProgress> progressByLevel)
    {
        if (progressByLevel.TryGetValue(level.LevelNumber, out var progress))
        {
            return progress.Status switch
            {
                LevelStatus.Perfect => "Perfect",
                LevelStatus.Completed => "Completed",
                _ => "Locked"
            };
        }

        return level.LevelNumber == currentLevel ? "Current" : "Locked";
    }

    private static int GetWordCount(PathLevel level) => level.IsBoss ? 20 : 10;

    private static int GetTimePerWordSeconds(LearningPath path, PathLevel level) =>
        level.IsBoss ? Math.Min(path.TimePerWord, 15) : path.TimePerWord;

    private static int GetHintCount(PathLevel level) => level.IsBoss ? 0 : 3;

    private static int GetLives(DifficultyLevel difficulty, PathLevel level)
    {
        if (level.IsBoss)
        {
            return 3;
        }

        return difficulty switch
        {
            DifficultyLevel.Beginner => 5,
            DifficultyLevel.Intermediate => 4,
            DifficultyLevel.Advanced => 3,
            DifficultyLevel.Expert => 3,
            _ => 5
        };
    }

    private static int GetXpReward(PathLevel level) => level.IsBoss ? 250 : 100;
}
