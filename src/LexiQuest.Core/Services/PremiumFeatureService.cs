using LexiQuest.Core.Configuration;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace LexiQuest.Core.Services;

public class PremiumFeatureService : IPremiumFeatureService
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly PremiumAccessOptions _premiumAccessOptions;

    public PremiumFeatureService(
        ISubscriptionService subscriptionService,
        IOptions<PremiumAccessOptions>? premiumAccessOptions = null)
    {
        _subscriptionService = subscriptionService;
        _premiumAccessOptions = premiumAccessOptions?.Value ?? new PremiumAccessOptions();
    }

    public async Task<bool> HasFeatureAsync(Guid userId, PremiumFeature feature)
    {
        if (_premiumAccessOptions.GrantAllFeatures)
        {
            return true;
        }

        var isPremium = await _subscriptionService.IsPremiumAsync(userId);
        
        if (!isPremium)
            return false;

        // Všechny premium funkce jsou dostupné pro premium uživatele
        // V budoucnu můžeme přidat specifické plány pro různé funkce
        return true;
    }

    public async Task<bool> IsPremiumAsync(Guid userId)
    {
        if (_premiumAccessOptions.GrantAllFeatures)
        {
            return true;
        }

        return await _subscriptionService.IsPremiumAsync(userId);
    }
}
