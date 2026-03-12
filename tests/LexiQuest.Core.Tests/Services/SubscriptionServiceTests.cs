using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class SubscriptionServiceTests
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SubscriptionService _service;

    public SubscriptionServiceTests()
    {
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _service = new SubscriptionService(_subscriptionRepository, _unitOfWork);
    }

    [Fact]
    public async Task IsPremium_UserHasActiveSubscription_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = Subscription.Create(
            userId,
            SubscriptionPlan.Monthly,
            "sub_123",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20));

        _subscriptionRepository.GetByUserIdAsync(userId).Returns(subscription);

        // Act
        var result = await _service.IsPremiumAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPremium_UserHasNoSubscription_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _subscriptionRepository.GetByUserIdAsync(userId).Returns((Subscription?)null);

        // Act
        var result = await _service.IsPremiumAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsPremium_UserHasExpiredSubscription_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = Subscription.Create(
            userId,
            SubscriptionPlan.Monthly,
            "sub_123",
            DateTime.UtcNow.AddDays(-40),
            DateTime.UtcNow.AddDays(-10));

        _subscriptionRepository.GetByUserIdAsync(userId).Returns(subscription);

        // Act
        var result = await _service.IsPremiumAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateSubscription_CreatesNewSubscription()
    {
        // Arrange
        var stripeSubId = "sub_123";
        var stripeCustomerId = "cus_456";
        var plan = SubscriptionPlan.Monthly;
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddMonths(1);

        // Act
        await _service.ActivateSubscriptionAsync(stripeSubId, stripeCustomerId, plan, startedAt, expiresAt);

        // Assert
        await _subscriptionRepository.Received(1).AddAsync(Arg.Is<Subscription>(s =>
            s.StripeSubscriptionId == stripeSubId &&
            s.Plan == plan &&
            s.Status == SubscriptionStatus.Active));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CancelSubscription_SetsCancelledAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = Subscription.Create(
            userId,
            SubscriptionPlan.Monthly,
            "sub_123",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20));

        _subscriptionRepository.GetByUserIdAsync(userId).Returns(subscription);

        // Act
        await _service.CancelSubscriptionAsync(userId);

        // Assert
        subscription.CancelledAt.Should().NotBeNull();
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CancelSubscription_NoActiveSubscription_DoesNothing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _subscriptionRepository.GetByUserIdAsync(userId).Returns((Subscription?)null);

        // Act
        await _service.CancelSubscriptionAsync(userId);

        // Assert
        await _unitOfWork.Received(0).SaveChangesAsync();
    }

    [Fact]
    public async Task CheckExpiredSubscriptions_MarksExpiredAsExpired()
    {
        // Arrange
        var expiredSubscription = Subscription.Create(
            Guid.NewGuid(),
            SubscriptionPlan.Monthly,
            "sub_expired",
            DateTime.UtcNow.AddDays(-40),
            DateTime.UtcNow.AddDays(-10));

        _subscriptionRepository.GetExpiredSubscriptionsAsync()
            .Returns(new[] { expiredSubscription });

        // Act
        await _service.CheckExpiredSubscriptionsAsync();

        // Assert
        expiredSubscription.Status.Should().Be(SubscriptionStatus.Expired);
        _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task GetActiveSubscription_UserHasActive_ReturnsSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscription = Subscription.Create(
            userId,
            SubscriptionPlan.Yearly,
            "sub_123",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(355));

        _subscriptionRepository.GetByUserIdAsync(userId).Returns(subscription);

        // Act
        var result = await _service.GetActiveSubscriptionAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Plan.Should().Be(SubscriptionPlan.Yearly);
    }
}
