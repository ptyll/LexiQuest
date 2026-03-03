using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// DTO for learning path information.
/// </summary>
public record LearningPathDto(
    Guid Id,
    string Name,
    string Description,
    DifficultyLevel Difficulty,
    int TotalLevels,
    int CompletedLevels,
    bool IsUnlocked,
    double ProgressPercentage
);

/// <summary>
/// DTO for path progress.
/// </summary>
public record PathProgressDto(
    Guid PathId,
    int TotalLevels,
    int CompletedLevels,
    int CurrentLevel,
    List<PathLevelDto> Levels
);

/// <summary>
/// DTO for path level information.
/// </summary>
public record PathLevelDto(
    Guid Id,
    int LevelNumber,
    string Status,
    bool IsBoss,
    bool IsPerfect
);
