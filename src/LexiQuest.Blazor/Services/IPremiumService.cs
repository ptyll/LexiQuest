using LexiQuest.Shared.DTOs.Premium;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Blazor.Services;

public interface IPremiumService
{
    Task<PremiumStatusDto?> GetStatusAsync();
    Task<CheckoutResponse> CreateCheckoutAsync(SubscriptionPlan plan);
    Task<bool> CancelSubscriptionAsync();
    Task<bool> IsPremiumAsync();
}
