namespace LexiQuest.Core.Domain.Entities;

public class StreakProtection
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int ShieldsRemaining { get; private set; }
    public bool FreezeUsedThisWeek { get; private set; }
    public DateTime? LastShieldActivatedAt { get; set; }
    public bool IsShieldActive { get; private set; }

    private StreakProtection() { }

    public static StreakProtection Create(Guid userId)
    {
        return new StreakProtection
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ShieldsRemaining = 0,
            FreezeUsedThisWeek = false,
            LastShieldActivatedAt = null,
            IsShieldActive = false
        };
    }

    public bool ActivateShield()
    {
        if (ShieldsRemaining <= 0)
            return false;

        if (IsShieldActive)
            return false;

        IsShieldActive = true;
        ShieldsRemaining--;
        LastShieldActivatedAt = DateTime.UtcNow;
        return true;
    }

    public void AddShields(int count)
    {
        if (count > 0)
            ShieldsRemaining += count;
    }

    public void RemoveShields(int count)
    {
        if (count > 0)
            ShieldsRemaining = Math.Max(0, ShieldsRemaining - count);
    }

    public void UseFreeze()
    {
        FreezeUsedThisWeek = true;
    }

    public bool CanUseFreeze()
    {
        return !FreezeUsedThisWeek;
    }

    public void ResetWeeklyFreeze()
    {
        FreezeUsedThisWeek = false;
    }

    public void DeactivateShield()
    {
        IsShieldActive = false;
    }
}
