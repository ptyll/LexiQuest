using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Premium;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Blazor.Tests.Pages;

public class PremiumPageTests : BunitContext
{
    private readonly IPremiumService _premiumService;
    private readonly IStringLocalizer<Premium> _localizer;

    public PremiumPageTests()
    {
        _premiumService = Substitute.For<IPremiumService>();
        _localizer = Substitute.For<IStringLocalizer<Premium>>();
        
        // Setup localizer returns
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        
        Services.AddSingleton(_premiumService);
        Services.AddSingleton(_localizer);
    }

    [Fact]
    public void PremiumPage_Renders_3PricingCards()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));

        // Act
        var cut = Render<Premium>();

        // Assert
        cut.FindAll(".pricing-card").Count.Should().Be(3);
    }

    [Fact]
    public void PremiumPage_Renders_HeroSection()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));

        // Act
        var cut = Render<Premium>();

        // Assert
        cut.Find(".hero-title").Should().NotBeNull();
        cut.Find(".hero-subtitle").Should().NotBeNull();
    }

    [Fact]
    public void PremiumPage_Renders_FeatureGroups()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));

        // Act
        var cut = Render<Premium>();

        // Assert
        cut.FindAll(".feature-group").Count.Should().Be(4);
    }

    [Fact]
    public void PremiumPage_YearlyCard_HasBestValueBadge()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));

        // Act
        var cut = Render<Premium>();

        // Assert
        var yearlyCard = cut.FindAll(".pricing-card")[1]; // Yearly is second
        yearlyCard.QuerySelector(".best-value-badge").Should().NotBeNull();
    }

    [Fact]
    public void PremiumPage_YearlyCard_ShowsDiscountedPrice()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));

        // Act
        var cut = Render<Premium>();

        // Assert
        var yearlyCard = cut.FindAll(".pricing-card")[1];
        yearlyCard.QuerySelector(".original-price").Should().NotBeNull();
    }

    [Fact]
    public void PremiumPage_MonthlyCard_HasSubscribeButton()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));

        // Act
        var cut = Render<Premium>();

        // Assert
        var monthlyCard = cut.FindAll(".pricing-card")[0];
        var button = monthlyCard.QuerySelector("button");
        button.Should().NotBeNull();
    }

    [Fact]
    public void PremiumPage_ClickMonthly_Subscribe_CallsService()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));
        _premiumService.CreateCheckoutAsync(SubscriptionPlan.Monthly).Returns(Task.FromResult(new CheckoutResponse("https://stripe.com/checkout")));

        // Act
        var cut = Render<Premium>();
        var monthlyButton = cut.FindAll(".pricing-card")[0].QuerySelector("button");
        monthlyButton?.Click();

        // Assert
        _premiumService.Received(1).CreateCheckoutAsync(SubscriptionPlan.Monthly);
    }

    [Fact]
    public void PremiumPage_ClickYearly_Subscribe_CallsService()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));
        _premiumService.CreateCheckoutAsync(SubscriptionPlan.Yearly).Returns(Task.FromResult(new CheckoutResponse("https://stripe.com/checkout")));

        // Act
        var cut = Render<Premium>();
        var yearlyButton = cut.FindAll(".pricing-card")[1].QuerySelector("button");
        yearlyButton?.Click();

        // Assert
        _premiumService.Received(1).CreateCheckoutAsync(SubscriptionPlan.Yearly);
    }

    [Fact]
    public void PremiumPage_ClickLifetime_Buy_CallsService()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));
        _premiumService.CreateCheckoutAsync(SubscriptionPlan.Lifetime).Returns(Task.FromResult(new CheckoutResponse("https://stripe.com/checkout")));

        // Act
        var cut = Render<Premium>();
        var lifetimeButton = cut.FindAll(".pricing-card")[2].QuerySelector("button");
        lifetimeButton?.Click();

        // Assert
        _premiumService.Received(1).CreateCheckoutAsync(SubscriptionPlan.Lifetime);
    }

    [Fact]
    public void PremiumPage_Renders_PaymentMethods()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));

        // Act
        var cut = Render<Premium>();

        // Assert
        cut.Find(".payment-methods").Should().NotBeNull();
    }

    [Fact]
    public void PremiumPage_Renders_CancelAnytimeNotice()
    {
        // Arrange
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(null));

        // Act
        var cut = Render<Premium>();

        // Assert
        cut.Find(".cancel-notice").Should().NotBeNull();
    }

    [Fact]
    public void PremiumPage_AlreadyPremium_ShowsActiveStatus()
    {
        // Arrange
        var status = new PremiumStatusDto
        {
            IsActive = true,
            Plan = SubscriptionPlan.Yearly,
            ExpiresAt = DateTime.UtcNow.AddMonths(6)
        };
        _premiumService.GetStatusAsync().Returns(Task.FromResult<PremiumStatusDto?>(status));

        // Act
        var cut = Render<Premium>();

        // Assert
        cut.Find(".premium-active-badge").Should().NotBeNull();
    }
}
