using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.Entities;

public class Achievement
{
    public Guid Id { get; private set; }
    public string Key { get; private set; } = null!;
    public AchievementCategory Category { get; private set; }
    public int XPReward { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public int RequiredValue { get; private set; }
    public string? IconName { get; private set; }

    private Achievement() { }

    public static Achievement Create(string key, AchievementCategory category, int xpReward, string name, string description, int requiredValue, string? iconName = null)
    {
        return new Achievement
        {
            Id = Guid.NewGuid(),
            Key = key,
            Category = category,
            XPReward = xpReward,
            Name = name,
            Description = description,
            RequiredValue = requiredValue,
            IconName = iconName
        };
    }
}

public class UserAchievement
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid AchievementId { get; private set; }
    public int Progress { get; private set; }
    public bool IsUnlocked { get; private set; }
    public DateTime? UnlockedAt { get; private set; }

    private UserAchievement() { }

    public static UserAchievement Create(Guid userId, Guid achievementId)
    {
        return new UserAchievement
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AchievementId = achievementId,
            Progress = 0,
            IsUnlocked = false
        };
    }

    public void UpdateProgress(int progress)
    {
        if (progress < 0) throw new ArgumentException("Progress cannot be negative");
        Progress = progress;
    }

    public void Unlock()
    {
        if (IsUnlocked) return;
        
        IsUnlocked = true;
        UnlockedAt = DateTime.UtcNow;
    }

    public int GetProgressPercentage(int requiredValue)
    {
        if (requiredValue <= 0) return 0;
        return Math.Min(100, (Progress * 100) / requiredValue);
    }
}
