namespace LexiQuest.Core.Domain.ValueObjects;

public class PremiumStatus
{
    public bool IsPremium { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string? Plan { get; private set; }

    private PremiumStatus() { }

    public static PremiumStatus CreateDefault()
    {
        return new PremiumStatus
        {
            IsPremium = false,
            ExpiresAt = null,
            Plan = null
        };
    }

    public void Activate(string plan, DateTime expiresAt)
    {
        IsPremium = true;
        Plan = plan;
        ExpiresAt = expiresAt;
    }

    public void Deactivate()
    {
        IsPremium = false;
        Plan = null;
        ExpiresAt = null;
    }

    public bool IsActive(DateTime utcNow)
    {
        return IsPremium && ExpiresAt.HasValue && ExpiresAt.Value > utcNow;
    }
}
