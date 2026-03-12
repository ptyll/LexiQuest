using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace LexiQuest.Core.Services;

public interface ISubscriptionExpirationJob
{
    Task CheckExpiredSubscriptionsAsync();
}

public class SubscriptionExpirationJob : ISubscriptionExpirationJob
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubscriptionExpirationJob> _logger;

    public SubscriptionExpirationJob(
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<SubscriptionExpirationJob> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task CheckExpiredSubscriptionsAsync()
    {
        _logger.LogInformation("Starting subscription expiration check at {Time}", DateTime.UtcNow);

        var expiredSubscriptions = await _subscriptionRepository.GetExpiredActiveSubscriptionsAsync(DateTime.UtcNow);

        if (!expiredSubscriptions.Any())
        {
            _logger.LogInformation("No expired subscriptions found");
            return;
        }

        _logger.LogInformation("Found {Count} expired subscriptions", expiredSubscriptions.Count());

        foreach (var subscription in expiredSubscriptions)
        {
            try
            {
                await ProcessExpiredSubscriptionAsync(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired subscription {SubscriptionId} for user {UserId}",
                    subscription.Id, subscription.UserId);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Subscription expiration check completed");
    }

    private async Task ProcessExpiredSubscriptionAsync(Subscription subscription)
    {
        _logger.LogInformation("Processing expired subscription {SubscriptionId} for user {UserId}",
            subscription.Id, subscription.UserId);

        // Update subscription status to expired
        subscription.MarkAsExpired();

        // Update user's premium status
        var user = await _userRepository.GetByIdAsync(subscription.UserId);
        if (user != null)
        {
            user.Premium.Deactivate();
            _logger.LogInformation("Premium status disabled for user {UserId}", subscription.UserId);
        }

        _logger.LogInformation("Subscription {SubscriptionId} marked as expired", subscription.Id);
    }
}
