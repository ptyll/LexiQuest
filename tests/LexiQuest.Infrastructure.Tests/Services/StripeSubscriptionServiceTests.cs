using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LexiQuest.Infrastructure.Tests.Services;

public class StripeSubscriptionServiceTests
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StripeSubscriptionService> _logger;
    private readonly StripeSubscriptionService _service;

    public StripeSubscriptionServiceTests()
    {
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<StripeSubscriptionService>>();
        
        var settings = new StripeSettings
        {
            ApiKey = "sk_test_dummy",
            WebhookSecret = "whsec_dummy",
            MonthlyPriceId = "price_monthly",
            YearlyPriceId = "price_yearly",
            LifetimePriceId = "price_lifetime"
        };
        
        _service = new StripeSubscriptionService(
            settings, 
            _subscriptionRepository, 
            _userRepository,
            _unitOfWork, 
            _logger);
    }

    [Theory]
    [InlineData(SubscriptionPlan.Monthly, "price_monthly")]
    [InlineData(SubscriptionPlan.Yearly, "price_yearly")]
    [InlineData(SubscriptionPlan.Lifetime, "price_lifetime")]
    public async Task CreateCheckoutSession_Monthly_ReturnsStripeUrl(SubscriptionPlan plan, string expectedPriceId)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _service.CreateCheckoutSessionAsync(userId, plan, "test@test.com");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("https://");
    }

    [Fact]
    public async Task CreateCheckoutSession_UserNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateCheckoutSessionAsync(userId, SubscriptionPlan.Monthly, "test@test.com"));
    }

    [Fact]
    public async Task ActivateSubscription_WithValidData_CreatesSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stripeSubscriptionId = "sub_123456";
        var stripeCustomerId = "cus_123456";
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddMonths(1);

        // Act
        await _service.ActivateSubscriptionAsync(
            stripeSubscriptionId, 
            stripeCustomerId, 
            SubscriptionPlan.Monthly, 
            startedAt, 
            expiresAt);

        // Assert
        await _subscriptionRepository.Received(1).AddAsync(Arg.Is<Subscription>(s => 
            s.StripeSubscriptionId == stripeSubscriptionId &&
            s.Plan == SubscriptionPlan.Monthly));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task HandleCheckoutCompleted_ActivatesSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        user.SetStripeCustomerId("cus_123456");
        
        _userRepository.FindByStripeCustomerIdAsync("cus_123456").Returns(user);

        // Act
        await _service.HandleCheckoutCompletedAsync("cus_123456", "sub_123456", SubscriptionPlan.Monthly);

        // Assert
        await _subscriptionRepository.Received(1).AddAsync(Arg.Any<Subscription>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task HandleInvoicePaid_ExtendsSubscription()
    {
        // Arrange
        var stripeSubscriptionId = "sub_123456";
        var existingSubscription = Subscription.Create(
            Guid.NewGuid(), 
            SubscriptionPlan.Monthly, 
            stripeSubscriptionId, 
            DateTime.UtcNow.AddMonths(-1), 
            DateTime.UtcNow);
        
        _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId).Returns(existingSubscription);

        // Act
        await _service.HandleInvoicePaidAsync(stripeSubscriptionId, DateTime.UtcNow.AddMonths(1));

        // Assert
        _subscriptionRepository.Received(1).Update(existingSubscription);
        existingSubscription.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(20));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task HandleSubscriptionCancelled_DeactivatesSubscription()
    {
        // Arrange
        var stripeSubscriptionId = "sub_123456";
        var existingSubscription = Subscription.Create(
            Guid.NewGuid(), 
            SubscriptionPlan.Monthly, 
            stripeSubscriptionId, 
            DateTime.UtcNow.AddMonths(-1), 
            DateTime.UtcNow.AddMonths(1));
        
        _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSubscriptionId).Returns(existingSubscription);

        // Act
        await _service.HandleSubscriptionCancelledAsync(stripeSubscriptionId);

        // Assert
        existingSubscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        _subscriptionRepository.Received(1).Update(existingSubscription);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }
}
