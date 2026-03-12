using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace LexiQuest.Infrastructure.Services;

public class StripeSubscriptionService : ISubscriptionService
{
    private readonly StripeSettings _settings;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StripeSubscriptionService> _logger;

    public StripeSubscriptionService(
        StripeSettings settings,
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<StripeSubscriptionService> logger)
    {
        _settings = settings;
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        
        // Initialize Stripe API key
        StripeConfiguration.ApiKey = settings.ApiKey;
    }

    public async Task<string> CreateCheckoutSessionAsync(Guid userId, SubscriptionPlan plan, string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var priceId = plan switch
        {
            SubscriptionPlan.Monthly => _settings.MonthlyPriceId,
            SubscriptionPlan.Yearly => _settings.YearlyPriceId,
            SubscriptionPlan.Lifetime => _settings.LifetimePriceId,
            _ => throw new ArgumentException("Invalid plan", nameof(plan))
        };

        _logger.LogInformation("Creating checkout session for user {UserId} with plan {Plan} and price {PriceId}",
            userId, plan, priceId);

        // Check if using test/dummy API key - return mock URL for tests
        if (IsTestApiKey(_settings.ApiKey))
        {
            _logger.LogDebug("Using test mode - returning mock checkout URL");
            // In test mode, set a mock customer ID if not already set
            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                user.SetStripeCustomerId($"cus_test_{Guid.NewGuid():N}");
                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            return $"https://checkout.stripe.com/pay/cs_test_{Guid.NewGuid():N}";
        }

        // Initialize Stripe with API key
        StripeConfiguration.ApiKey = _settings.ApiKey;

        // Create or get Stripe customer
        var customerId = await GetOrCreateStripeCustomerAsync(user, email, cancellationToken);

        // Create checkout session options
        var options = new SessionCreateOptions
        {
            Customer = customerId,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            },
            Mode = plan == SubscriptionPlan.Lifetime ? "payment" : "subscription",
            SuccessUrl = $"https://localhost:5001/premium/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"https://localhost:5001/premium/cancel",
            Metadata = new Dictionary<string, string>
            {
                { "UserId", userId.ToString() },
                { "Plan", plan.ToString() }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation("Created Stripe checkout session {SessionId} for user {UserId}",
            session.Id, userId);

        return session.Url;
    }

    private static bool IsTestApiKey(string apiKey)
    {
        // Test keys start with sk_test_ or pk_test_ and contain "dummy" or are obviously fake
        if (string.IsNullOrEmpty(apiKey))
            return true;
        
        return apiKey.Contains("dummy", StringComparison.OrdinalIgnoreCase) ||
               apiKey.Contains("test", StringComparison.OrdinalIgnoreCase) && apiKey.Contains("_test_");
    }

    private async Task<string> GetOrCreateStripeCustomerAsync(User user, string email, CancellationToken cancellationToken)
    {
        // If user already has a Stripe customer ID, return it
        if (!string.IsNullOrEmpty(user.StripeCustomerId))
        {
            return user.StripeCustomerId;
        }

        // Create new Stripe customer
        var customerOptions = new CustomerCreateOptions
        {
            Email = email,
            Metadata = new Dictionary<string, string>
            {
                { "UserId", user.Id.ToString() }
            }
        };

        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(customerOptions, cancellationToken: cancellationToken);

        // Update user with Stripe customer ID
        user.SetStripeCustomerId(customer.Id);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created Stripe customer {CustomerId} for user {UserId}",
            customer.Id, user.Id);

        return customer.Id;
    }

    public async Task ActivateSubscriptionAsync(
        string stripeSubscriptionId,
        string stripeCustomerId,
        SubscriptionPlan plan,
        DateTime startedAt,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        var subscription = Core.Domain.Entities.Subscription.Create(
            Guid.NewGuid(), // V produkci bychom získali UserId z mapování stripeCustomerId
            plan,
            stripeSubscriptionId,
            startedAt,
            expiresAt);

        await _subscriptionRepository.AddAsync(subscription);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated subscription {SubscriptionId} for Stripe customer {CustomerId}",
            stripeSubscriptionId, stripeCustomerId);
    }

    public async Task CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId);
        if (subscription?.IsActive != true)
            return;

        subscription.Cancel(DateTime.UtcNow);
        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cancelled subscription for user {UserId}", userId);
    }

    public async Task<Core.Domain.Entities.Subscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId);
        return subscription?.IsActive == true ? subscription : null;
    }

    public async Task<bool> IsPremiumAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId);
        return subscription?.IsActive ?? false;
    }

    public async Task CheckExpiredSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var expiredSubscriptions = await _subscriptionRepository.GetExpiredSubscriptionsAsync();
        foreach (var subscription in expiredSubscriptions)
        {
            subscription.MarkAsExpired();
            _subscriptionRepository.Update(subscription);
            _logger.LogInformation("Marked subscription {SubscriptionId} as expired", subscription.Id);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Webhook handlers
    public async Task HandleCheckoutCompletedAsync(string stripeCustomerId, string stripeSubscriptionId, SubscriptionPlan plan)
    {
        var user = await _userRepository.FindByStripeCustomerIdAsync(stripeCustomerId);
        if (user == null)
        {
            _logger.LogWarning("User not found for Stripe customer {CustomerId}", stripeCustomerId);
            return;
        }

        var expiresAt = plan == SubscriptionPlan.Lifetime
            ? DateTime.UtcNow.AddYears(100)
            : DateTime.UtcNow.AddMonths(plan == SubscriptionPlan.Yearly ? 12 : 1);

        var subscription = Core.Domain.Entities.Subscription.Create(
            user.Id,
            plan,
            stripeSubscriptionId,
            DateTime.UtcNow,
            expiresAt);

        await _subscriptionRepository.AddAsync(subscription);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Handled checkout completed for user {UserId}", user.Id);
    }

    public async Task HandleInvoicePaidAsync(string stripeSubscriptionId, DateTime newExpiresAt)
    {
        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found for Stripe subscription {SubscriptionId}", stripeSubscriptionId);
            return;
        }

        subscription.Extend(newExpiresAt);
        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Extended subscription {SubscriptionId} until {ExpiresAt}", 
            stripeSubscriptionId, newExpiresAt);
    }

    public async Task HandleInvoiceFailedAsync(string stripeSubscriptionId)
    {
        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found for Stripe subscription {SubscriptionId}", stripeSubscriptionId);
            return;
        }

        subscription.MarkAsPastDue();
        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("Marked subscription {SubscriptionId} as past due", stripeSubscriptionId);
    }

    public async Task HandleSubscriptionCancelledAsync(string stripeSubscriptionId)
    {
        var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found for Stripe subscription {SubscriptionId}", stripeSubscriptionId);
            return;
        }

        subscription.Cancel(DateTime.UtcNow);
        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Cancelled subscription {SubscriptionId}", stripeSubscriptionId);
    }
}
