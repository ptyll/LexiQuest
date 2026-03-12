using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository for push subscription data access.
/// </summary>
public interface IPushSubscriptionRepository
{
    /// <summary>
    /// Gets push subscriptions for a user.
    /// </summary>
    Task<List<PushSubscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a push subscription by endpoint.
    /// </summary>
    Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new push subscription.
    /// </summary>
    Task AddAsync(PushSubscription subscription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a push subscription.
    /// </summary>
    void Remove(PushSubscription subscription);

    /// <summary>
    /// Updates a push subscription.
    /// </summary>
    void Update(PushSubscription subscription);
}
