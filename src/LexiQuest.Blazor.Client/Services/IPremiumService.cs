using LexiQuest.Shared.DTOs.Premium;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Blazor.Services;

public interface IPremiumService
{
    Task<PremiumStatusDto?> GetStatusAsync();
    Task<IReadOnlyList<PremiumFeatureDto>> GetFeaturesAsync();
    Task<CheckoutResponse> CreateCheckoutAsync(SubscriptionPlan plan);
    Task<SubscriptionStatusDto?> CompleteFakeCheckoutAsync(string sessionId, SubscriptionPlan plan);
    Task<bool> CancelSubscriptionAsync();
    Task<bool> IsPremiumAsync();
}
