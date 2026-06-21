using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id);
    Task<Subscription?> GetByUserIdAsync(Guid userId);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
    Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync();
    Task<IEnumerable<Subscription>> GetExpiredActiveSubscriptionsAsync(DateTime now);
    Task<IEnumerable<Subscription>> GetActiveSubscriptionsExpiringBetweenAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
    Task AddAsync(Subscription subscription);
    void Update(Subscription subscription);
}
