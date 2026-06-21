namespace LexiQuest.Core.Domain.Entities;

public class UserPathLevelProgress
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PathId { get; private set; }
    public Guid PathLevelId { get; private set; }
    public int LevelNumber { get; private set; }
    public LevelStatus Status { get; private set; }
    public bool IsPerfect { get; private set; }
    public DateTime CompletedAt { get; private set; }

    private UserPathLevelProgress() { }

    public static UserPathLevelProgress Complete(
        Guid userId,
        Guid pathId,
        Guid pathLevelId,
        int levelNumber,
        bool isPerfect)
    {
        return new UserPathLevelProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PathId = pathId,
            PathLevelId = pathLevelId,
            LevelNumber = levelNumber,
            Status = isPerfect ? LevelStatus.Perfect : LevelStatus.Completed,
            IsPerfect = isPerfect,
            CompletedAt = DateTime.UtcNow
        };
    }

    public void MarkCompleted(bool isPerfect)
    {
        if (IsPerfect && !isPerfect)
        {
            return;
        }

        Status = isPerfect ? LevelStatus.Perfect : LevelStatus.Completed;
        IsPerfect = isPerfect;
        CompletedAt = DateTime.UtcNow;
    }
}
