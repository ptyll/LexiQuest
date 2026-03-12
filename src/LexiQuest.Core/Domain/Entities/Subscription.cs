using LexiQuest.Core.Domain.Enums;

namespace LexiQuest.Core.Domain.Entities;

public class Subscription
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public SubscriptionPlan Plan { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string StripeSubscriptionId { get; private set; } = string.Empty;
    public SubscriptionStatus Status { get; private set; }

    public bool IsActive => Status == SubscriptionStatus.Active && ExpiresAt > DateTime.UtcNow;

    private Subscription() { }

    public static Subscription Create(Guid userId, SubscriptionPlan plan, string stripeSubscriptionId, DateTime startedAt, DateTime expiresAt)
    {
        return new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Plan = plan,
            StripeSubscriptionId = stripeSubscriptionId,
            StartedAt = startedAt,
            ExpiresAt = expiresAt,
            Status = SubscriptionStatus.Active,
            CancelledAt = null
        };
    }

    public void Cancel(DateTime cancelledAt)
    {
        CancelledAt = cancelledAt;
        Status = SubscriptionStatus.Cancelled;
    }

    public void Extend(DateTime newExpiresAt)
    {
        ExpiresAt = newExpiresAt;
    }

    public void MarkAsExpired()
    {
        Status = SubscriptionStatus.Expired;
    }

    public void MarkAsPastDue()
    {
        Status = SubscriptionStatus.PastDue;
    }
}
