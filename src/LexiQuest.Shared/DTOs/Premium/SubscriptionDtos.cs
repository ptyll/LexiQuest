namespace LexiQuest.Shared.DTOs.Premium;

public record CreateCheckoutRequest(SubscriptionPlan Plan);

public record CheckoutResponse(string StripeCheckoutUrl);

public record SubscriptionStatusDto(
    bool IsActive,
    SubscriptionPlan? Plan,
    DateTime? ExpiresAt,
    SubscriptionStatus Status);

public record CancelSubscriptionRequest(string Reason);

public record PremiumFeatureDto(
    string Feature,
    string Description,
    bool IsAvailable);

public enum SubscriptionPlan
{
    Monthly,
    Yearly,
    Lifetime
}

public enum SubscriptionStatus
{
    Active,
    Cancelled,
    Expired,
    PastDue
}

public enum PremiumFeature
{
    NoAds,
    StreakFreeze,
    StreakShield,
    DoubleXPWeekends,
    ExclusivePaths,
    CustomDictionaries,
    DetailedStats,
    CustomAvatar,
    DiamondLeague,
    TeamCreation
}
