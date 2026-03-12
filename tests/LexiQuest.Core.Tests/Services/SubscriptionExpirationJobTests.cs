using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
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
