using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.Entities;

/// <summary>
/// Represents a learning path with multiple levels.
/// </summary>
public class LearningPath
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DifficultyLevel Difficulty { get; private set; }
    public int TotalLevels { get; private set; }
    public int WordLengthMin { get; private set; }
    public int WordLengthMax { get; private set; }
    public int TimePerWord { get; private set; }
    public List<PathLevel> Levels { get; private set; } = [];

    private LearningPath() { }

    public static LearningPath Create(
        string name,
        string description,
        DifficultyLevel difficulty,
        int totalLevels,
        int wordLengthMin,
        int wordLengthMax,
        int timePerWord)
    {
        return new LearningPath
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Difficulty = difficulty,
            TotalLevels = totalLevels,
            WordLengthMin = wordLengthMin,
            WordLengthMax = wordLengthMax,
            TimePerWord = timePerWord,
            Levels = []
        };
    }

    public void AddLevel(int levelNumber, bool isBoss = false)
    {
        Levels.Add(PathLevel.Create(Id, levelNumber, isBoss));
    }
}

/// <summary>
/// Represents a level within a learning path.
/// </summary>
public class PathLevel
{
    public Guid Id { get; private set; }
    public Guid PathId { get; private set; }
    public int LevelNumber { get; private set; }
    public LevelStatus Status { get; private set; }
    public bool IsBoss { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public bool IsPerfect { get; private set; }

    private PathLevel() { }

    public static PathLevel Create(Guid pathId, int levelNumber, bool isBoss = false)
    {
        return new PathLevel
        {
            Id = Guid.NewGuid(),
            PathId = pathId,
            LevelNumber = levelNumber,
            Status = LevelStatus.Locked,
            IsBoss = isBoss,
            IsPerfect = false
        };
    }

    public void Unlock()
    {
        if (Status == LevelStatus.Locked)
            Status = LevelStatus.Available;
    }

    public void Start()
    {
        if (Status == LevelStatus.Available)
            Status = LevelStatus.Current;
    }

    public void Complete(bool isPerfect = false)
    {
        Status = isPerfect ? LevelStatus.Perfect : LevelStatus.Completed;
        IsPerfect = isPerfect;
        CompletedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Status of a path level.
/// </summary>
public enum LevelStatus
{
    Locked,
    Available,
    Current,
    Completed,
    Perfect
}
