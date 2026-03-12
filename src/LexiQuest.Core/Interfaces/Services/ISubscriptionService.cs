using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;

namespace LexiQuest.Core.Interfaces.Services;

public interface ISubscriptionService
{
    Task<string> CreateCheckoutSessionAsync(Guid userId, SubscriptionPlan plan, string email, CancellationToken cancellationToken = default);
    Task<Subscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsPremiumAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ActivateSubscriptionAsync(string stripeSubscriptionId, string stripeCustomerId, SubscriptionPlan plan, DateTime startedAt, DateTime expiresAt, CancellationToken cancellationToken = default);
    Task CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CheckExpiredSubscriptionsAsync(CancellationToken cancellationToken = default);
}
