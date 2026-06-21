using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Premium;

public class PremiumStatusDto
{
    public bool IsActive { get; set; }
    public SubscriptionPlan? Plan { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Expired;
}
