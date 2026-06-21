using System.Globalization;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LexiQuest.Core.Services;

/// <summary>
/// Sends a reminder email shortly before a paid premium subscription expires.
/// </summary>
public class PremiumExpiryReminderJob
{
    private const int ReminderWindowDays = 3;

    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizer<PremiumExpiryReminderJob> _localizer;
    private readonly ILogger<PremiumExpiryReminderJob> _logger;

    public PremiumExpiryReminderJob(
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IStringLocalizer<PremiumExpiryReminderJob> localizer,
        ILogger<PremiumExpiryReminderJob> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var windowEnd = now.AddDays(ReminderWindowDays);

        _logger.LogInformation(
            "Starting premium expiry reminder job for subscriptions expiring before {WindowEnd}",
            windowEnd);

        var subscriptions = await _subscriptionRepository.GetActiveSubscriptionsExpiringBetweenAsync(
            now,
            windowEnd,
            cancellationToken);

        var sentCount = 0;
        foreach (var subscription in subscriptions)
        {
            if (subscription.Plan == SubscriptionPlan.Lifetime)
            {
                continue;
            }

            var user = await _userRepository.GetByIdAsync(subscription.UserId, cancellationToken);
            if (string.IsNullOrWhiteSpace(user?.Email))
            {
                continue;
            }

            var expiresAt = subscription.ExpiresAt.ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("cs-CZ"));
            await _emailService.SendNotificationEmailAsync(
                user.Email,
                _localizer["PremiumExpiry.Title"],
                string.Format(
                    CultureInfo.GetCultureInfo("cs-CZ"),
                    _localizer["PremiumExpiry.Message"],
                    expiresAt),
                cancellationToken);

            sentCount++;
        }

        _logger.LogInformation("Premium expiry reminder job completed. Sent {Count} emails", sentCount);
    }
}
