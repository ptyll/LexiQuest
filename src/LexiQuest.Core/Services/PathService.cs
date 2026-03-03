using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Services;

public class PathService : IPathService
{
    private readonly IPathRepository _pathRepository;
    private readonly IUserRepository _userRepository;

    public PathService(IPathRepository pathRepository, IUserRepository userRepository)
    {
        _pathRepository = pathRepository;
        _userRepository = userRepository;
    }

    public async Task<List<LearningPathDto>> GetPathsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        var paths = await _pathRepository.GetAllAsync(cancellationToken);
        var result = new List<LearningPathDto>();

        foreach (var path in paths)
        {
            var completedLevels = await _pathRepository.GetCompletedLevelsCountAsync(userId, path.Id, cancellationToken);
            var isUnlocked = await IsPathUnlockedAsync(userId, path.Difficulty, cancellationToken);
            var progressPercentage = path.TotalLevels > 0 
                ? (double)completedLevels / path.TotalLevels * 100 
                : 0;

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

    public async Task<bool> IsPathUnlockedAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        return difficulty switch
        {
            DifficultyLevel.Beginner => true, // Always unlocked
            DifficultyLevel.Intermediate => user.Stats.Level >= 5, // Requires Level 5
            DifficultyLevel.Advanced => await IsAdvancedUnlockedAsync(userId, cancellationToken),
            DifficultyLevel.Expert => await IsExpertUnlockedAsync(userId, cancellationToken),
            _ => false
        };
    }

    public async Task<PathProgressDto> GetPathProgressAsync(Guid userId, Guid pathId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        var path = await _pathRepository.GetByIdAsync(pathId, cancellationToken);
        if (path == null)
        {
            throw new InvalidOperationException($"Path {pathId} not found");
        }

        var completedLevels = await _pathRepository.GetCompletedLevelsCountAsync(userId, pathId, cancellationToken);
        var currentLevel = completedLevels + 1;
        
        var levelDtos = new List<PathLevelDto>();
        foreach (var level in path.Levels.OrderBy(l => l.LevelNumber))
        {
            var status = GetLevelStatus(level.LevelNumber, completedLevels);
            levelDtos.Add(new PathLevelDto(
                Id: level.Id,
                LevelNumber: level.LevelNumber,
                Status: status,
                IsBoss: level.IsBoss,
                IsPerfect: level.IsPerfect
            ));
        }

        return new PathProgressDto(
            PathId: pathId,
            TotalLevels: path.TotalLevels,
            CompletedLevels: completedLevels,
            CurrentLevel: Math.Min(currentLevel, path.TotalLevels),
            Levels: levelDtos
        );
    }

    private async Task<bool> IsAdvancedUnlockedAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Advanced requires Intermediate path to be completed
        var paths = await _pathRepository.GetAllAsync(cancellationToken);
        var intermediatePath = paths.FirstOrDefault(p => p.Difficulty == DifficultyLevel.Intermediate);
        
        if (intermediatePath == null)
            return false;

        var completedLevels = await _pathRepository.GetCompletedLevelsCountAsync(userId, intermediatePath.Id, cancellationToken);
        return completedLevels >= intermediatePath.TotalLevels;
    }

    private async Task<bool> IsExpertUnlockedAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Expert requires Advanced path to be completed
        var paths = await _pathRepository.GetAllAsync(cancellationToken);
        var advancedPath = paths.FirstOrDefault(p => p.Difficulty == DifficultyLevel.Advanced);
        
        if (advancedPath == null)
            return false;

        var completedLevels = await _pathRepository.GetCompletedLevelsCountAsync(userId, advancedPath.Id, cancellationToken);
        return completedLevels >= advancedPath.TotalLevels;
    }

    private string GetLevelStatus(int levelNumber, int completedLevels)
    {
        if (levelNumber <= completedLevels)
            return "Completed";
        if (levelNumber == completedLevels + 1)
            return "Current";
        return "Locked";
    }
}
