using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces.Services;

namespace LexiQuest.Core.Services;

public class PremiumFeatureService : IPremiumFeatureService
{
    private readonly ISubscriptionService _subscriptionService;

    public PremiumFeatureService(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public async Task<bool> HasFeatureAsync(Guid userId, PremiumFeature feature)
    {
        var isPremium = await _subscriptionService.IsPremiumAsync(userId);
        
        if (!isPremium)
            return false;

        // Všechny premium funkce jsou dostupné pro premium uživatele
        // V budoucnu můžeme přidat specifické plány pro různé funkce
        return true;
    }

    public async Task<bool> IsPremiumAsync(Guid userId)
    {
        return await _subscriptionService.IsPremiumAsync(userId);
    }
}
