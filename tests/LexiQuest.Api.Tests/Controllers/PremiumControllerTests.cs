using System.Security.Claims;
using FluentAssertions;
using LexiQuest.Api.Controllers;
using LexiQuest.Core.Domain.Entities;
// SubscriptionPlan and SubscriptionStatus are from LexiQuest.Shared.DTOs.Premium
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Premium;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace LexiQuest.Api.Tests.Controllers;

public class PremiumControllerTests
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IPremiumFeatureService _premiumFeatureService;
    private readonly PremiumController _controller;
    private readonly Guid _testUserId;

    public PremiumControllerTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _premiumFeatureService = Substitute.For<IPremiumFeatureService>();
        _controller = new PremiumController(_subscriptionService, _premiumFeatureService);
        _testUserId = Guid.NewGuid();

        // Setup authenticated user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task CreateCheckout_ValidRequest_ReturnsCheckoutUrl()
    {
        // Arrange
        var request = new CreateCheckoutRequest(Shared.DTOs.Premium.SubscriptionPlan.Monthly);
        var expectedUrl = "https://checkout.stripe.com/test_session";
        _subscriptionService.CreateCheckoutSessionAsync(
            _testUserId, 
            Arg.Any<Core.Domain.Enums.SubscriptionPlan>(), 
            Arg.Any<string>(), 
            Arg.Any<CancellationToken>())
            .Returns(expectedUrl);

        // Act
        var result = await _controller.CreateCheckout(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as CheckoutResponse;
        response!.StripeCheckoutUrl.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task GetStatus_WithActiveSubscription_ReturnsActiveStatus()
    {
        // Arrange
        var subscription = Subscription.Create(
            _testUserId,
            Core.Domain.Enums.SubscriptionPlan.Monthly,
            "sub_123",
            DateTime.UtcNow.AddMonths(-1),
            DateTime.UtcNow.AddMonths(1));

        _subscriptionService.GetActiveSubscriptionAsync(_testUserId, Arg.Any<CancellationToken>())
            .Returns(subscription);

        // Act
        var result = await _controller.GetStatus(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var status = okResult!.Value as SubscriptionStatusDto;
        status!.IsActive.Should().BeTrue();
        status.Plan.Should().Be(Shared.DTOs.Premium.SubscriptionPlan.Monthly);
    }

    [Fact]
    public async Task GetStatus_NoSubscription_ReturnsExpiredStatus()
    {
        // Arrange
        _subscriptionService.GetActiveSubscriptionAsync(_testUserId, Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        // Act
        var result = await _controller.GetStatus(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var status = okResult!.Value as SubscriptionStatusDto;
        status!.IsActive.Should().BeFalse();
        status.Status.Should().Be(Shared.DTOs.Premium.SubscriptionStatus.Expired);
    }

    [Fact]
    public async Task CancelSubscription_CallsService()
    {
        // Arrange
        _subscriptionService.CancelSubscriptionAsync(_testUserId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CancelSubscription(CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        await _subscriptionService.Received(1).CancelSubscriptionAsync(_testUserId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetFeatures_ReturnsAllFeaturesWithCorrectAvailability(bool isPremium)
    {
        // Arrange
        _subscriptionService.IsPremiumAsync(_testUserId, Arg.Any<CancellationToken>())
            .Returns(isPremium);

        // Act
        var result = await _controller.GetFeatures(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var features = okResult!.Value as List<PremiumFeatureDto>;
        features!.Should().HaveCount(10);
        features.All(f => f.IsAvailable == isPremium).Should().BeTrue();
    }
}
