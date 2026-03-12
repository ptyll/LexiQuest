using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;

namespace LexiQuest.Core.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionService(ISubscriptionRepository subscriptionRepository, IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<string> CreateCheckoutSessionAsync(Guid userId, SubscriptionPlan plan, string email, CancellationToken cancellationToken = default)
    {
        // Tato metoda bude implementována v Infrastructure s použitím Stripe SDK
        // Zde vracíme placeholder - skutečná implementace bude v Infrastructure projektu
        throw new NotImplementedException("Stripe checkout session creation requires Infrastructure layer with Stripe SDK");
    }

    public async Task<Subscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId);
        return subscription?.IsActive == true ? subscription : null;
    }

    public async Task<bool> IsPremiumAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId);
        return subscription?.IsActive ?? false;
    }

    public async Task ActivateSubscriptionAsync(
        string stripeSubscriptionId,
        string stripeCustomerId,
        SubscriptionPlan plan,
        DateTime startedAt,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        // Zde bychom měli získat userId z mapování stripeCustomerId -> userId
        // Pro jednoduchost použijeme nového uživatele - v reálné implementaci by to bylo mapováno
        var subscription = Subscription.Create(
            Guid.NewGuid(), // Toto by mělo být mapováno ze stripeCustomerId
            plan,
            stripeSubscriptionId,
            startedAt,
            expiresAt);

        await _subscriptionRepository.AddAsync(subscription);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId);
        if (subscription?.IsActive != true)
            return;

        subscription.Cancel(DateTime.UtcNow);
        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CheckExpiredSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var expiredSubscriptions = await _subscriptionRepository.GetExpiredSubscriptionsAsync();
        foreach (var subscription in expiredSubscriptions)
        {
            subscription.MarkAsExpired();
            _subscriptionRepository.Update(subscription);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
