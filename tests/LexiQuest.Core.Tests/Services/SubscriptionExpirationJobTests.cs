using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class SubscriptionExpirationJobTests
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubscriptionExpirationJob> _logger;
    private readonly SubscriptionExpirationJob _job;

    public SubscriptionExpirationJobTests()
    {
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<SubscriptionExpirationJob>>();
        _job = new SubscriptionExpirationJob(
            _subscriptionRepository,
            _userRepository,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task CheckExpiredSubscriptions_NoExpiredSubscriptions_DoesNothing()
    {
        // Arrange
        _subscriptionRepository.GetExpiredActiveSubscriptionsAsync(Arg.Any<DateTime>())
            .Returns(new List<Subscription>());

        // Act
        await _job.CheckExpiredSubscriptionsAsync();

        // Assert
        await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task CheckExpiredSubscriptions_WithExpiredSubscription_MarksAsExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = Subscription.Create(
            userId,
            SubscriptionPlan.Monthly,
            "sub_123",
            DateTime.UtcNow.AddMonths(-2),
            DateTime.UtcNow.AddDays(-1));

        var user = User.Create("test@test.com", "testuser");
        typeof(User).GetProperty("Id")?.SetValue(user, userId);
        user.Premium.Activate(SubscriptionPlan.Monthly.ToString(), DateTime.UtcNow.AddDays(30));

        _subscriptionRepository.GetExpiredActiveSubscriptionsAsync(Arg.Any<DateTime>())
            .Returns(new List<Subscription> { subscription });
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        await _job.CheckExpiredSubscriptionsAsync();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Expired);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CheckExpiredSubscriptions_WithExpiredSubscription_DisablesUserPremium()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = Subscription.Create(
            userId,
            SubscriptionPlan.Monthly,
            "sub_123",
            DateTime.UtcNow.AddMonths(-2),
            DateTime.UtcNow.AddDays(-1));

        var user = User.Create("test@test.com", "testuser");
        typeof(User).GetProperty("Id")?.SetValue(user, userId);
        user.Premium.Activate(SubscriptionPlan.Monthly.ToString(), DateTime.UtcNow.AddDays(30));

        _subscriptionRepository.GetExpiredActiveSubscriptionsAsync(Arg.Any<DateTime>())
            .Returns(new List<Subscription> { subscription });
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        await _job.CheckExpiredSubscriptionsAsync();

        // Assert
        user.Premium.IsPremium.Should().BeFalse();
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CheckExpiredSubscriptions_WithMultipleExpiredSubscriptions_ProcessesAll()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var subscription1 = Subscription.Create(
            userId1,
            SubscriptionPlan.Monthly,
            "sub_123",
            DateTime.UtcNow.AddMonths(-2),
            DateTime.UtcNow.AddDays(-1));

        var subscription2 = Subscription.Create(
            userId2,
            SubscriptionPlan.Yearly,
            "sub_456",
            DateTime.UtcNow.AddYears(-2),
            DateTime.UtcNow.AddDays(-5));

        var user1 = User.Create("test1@test.com", "testuser1");
        typeof(User).GetProperty("Id")?.SetValue(user1, userId1);
        user1.Premium.Activate(SubscriptionPlan.Monthly.ToString(), DateTime.UtcNow.AddDays(30));

        var user2 = User.Create("test2@test.com", "testuser2");
        typeof(User).GetProperty("Id")?.SetValue(user2, userId2);
        user2.Premium.Activate(SubscriptionPlan.Yearly.ToString(), DateTime.UtcNow.AddDays(30));

        _subscriptionRepository.GetExpiredActiveSubscriptionsAsync(Arg.Any<DateTime>())
            .Returns(new List<Subscription> { subscription1, subscription2 });
        _userRepository.GetByIdAsync(userId1).Returns(user1);
        _userRepository.GetByIdAsync(userId2).Returns(user2);

        // Act
        await _job.CheckExpiredSubscriptionsAsync();

        // Assert
        subscription1.Status.Should().Be(SubscriptionStatus.Expired);
        subscription2.Status.Should().Be(SubscriptionStatus.Expired);
        user1.Premium.IsPremium.Should().BeFalse();
        user2.Premium.IsPremium.Should().BeFalse();
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CheckExpiredSubscriptions_UserNotFound_ContinuesProcessing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = Subscription.Create(
            userId,
            SubscriptionPlan.Monthly,
            "sub_123",
            DateTime.UtcNow.AddMonths(-2),
            DateTime.UtcNow.AddDays(-1));

        _subscriptionRepository.GetExpiredActiveSubscriptionsAsync(Arg.Any<DateTime>())
            .Returns(new List<Subscription> { subscription });
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        await _job.CheckExpiredSubscriptionsAsync();

        // Assert - should not throw, subscription still marked as expired
        subscription.Status.Should().Be(SubscriptionStatus.Expired);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CheckExpiredSubscriptions_WithErrorInOneSubscription_ContinuesProcessingOthers()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var subscription1 = Subscription.Create(
            userId1,
            SubscriptionPlan.Monthly,
            "sub_123",
            DateTime.UtcNow.AddMonths(-2),
            DateTime.UtcNow.AddDays(-1));

        var subscription2 = Subscription.Create(
            userId2,
            SubscriptionPlan.Yearly,
            "sub_456",
            DateTime.UtcNow.AddYears(-2),
            DateTime.UtcNow.AddDays(-5));

        var user2 = User.Create("test2@test.com", "testuser2");
        typeof(User).GetProperty("Id")?.SetValue(user2, userId2);
        user2.Premium.Activate(SubscriptionPlan.Yearly.ToString(), DateTime.UtcNow.AddDays(30));

        _subscriptionRepository.GetExpiredActiveSubscriptionsAsync(Arg.Any<DateTime>())
            .Returns(new List<Subscription> { subscription1, subscription2 });

        // First user will throw exception when retrieving
        _userRepository.GetByIdAsync(userId1)
            .Returns(Task.FromException<User>(new Exception("Database error")));
        _userRepository.GetByIdAsync(userId2).Returns(user2);

        // Act
        await _job.CheckExpiredSubscriptionsAsync();

        // Assert - second subscription should still be processed
        subscription1.Status.Should().Be(SubscriptionStatus.Expired); // Will be expired before error
        subscription2.Status.Should().Be(SubscriptionStatus.Expired);
        user2.Premium.IsPremium.Should().BeFalse();
        await _unitOfWork.Received(1).SaveChangesAsync();
    }
}

public class PremiumExpiryReminderJobTests
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizer<PremiumExpiryReminderJob> _localizer;
    private readonly ILogger<PremiumExpiryReminderJob> _logger;
    private readonly PremiumExpiryReminderJob _job;

    public PremiumExpiryReminderJobTests()
    {
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _emailService = Substitute.For<IEmailService>();
        _localizer = Substitute.For<IStringLocalizer<PremiumExpiryReminderJob>>();
        _logger = Substitute.For<ILogger<PremiumExpiryReminderJob>>();

        _localizer["PremiumExpiry.Title"].Returns(new LocalizedString("PremiumExpiry.Title", "Premium brzy vyprší"));
        _localizer["PremiumExpiry.Message"].Returns(new LocalizedString(
            "PremiumExpiry.Message",
            "Vaše Premium členství vyprší {0}. Obnovte ho včas, ať nepřijdete o prémiové funkce."));

        _job = new PremiumExpiryReminderJob(
            _subscriptionRepository,
            _userRepository,
            _emailService,
            _localizer,
            _logger);
    }

    [Fact]
    public async Task PremiumExpiryReminderJob_ExpiringWithinThreeDays_SendsEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = new DateTime(2026, 6, 23, 12, 0, 0, DateTimeKind.Utc);
        var subscription = Subscription.Create(
            userId,
            SubscriptionPlan.Monthly,
            "sub_123",
            expiresAt.AddMonths(-1),
            expiresAt);

        var user = User.Create("premium@test.com", "premiumuser");
        typeof(User).GetProperty("Id")?.SetValue(user, userId);

        _subscriptionRepository
            .GetActiveSubscriptionsExpiringBetweenAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { subscription });
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _job.ExecuteAsync();

        // Assert
        await _emailService.Received(1).SendNotificationEmailAsync(
            "premium@test.com",
            "Premium brzy vyprší",
            Arg.Is<string>(message =>
                message.Contains("23.06.2026", StringComparison.Ordinal)
                && message.Contains("prémiové funkce", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PremiumExpiryReminderJob_LifetimeSubscription_DoesNotSendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = Subscription.Create(
            userId,
            SubscriptionPlan.Lifetime,
            "sub_lifetime",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(2));

        _subscriptionRepository
            .GetActiveSubscriptionsExpiringBetweenAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { subscription });

        // Act
        await _job.ExecuteAsync();

        // Assert
        await _emailService.DidNotReceive().SendNotificationEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }
}
