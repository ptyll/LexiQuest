using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Stats;
using LexiQuest.Shared.DTOs.Streak;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components.Stats;

public class StreakIndicatorTests : TestContext
{
    private readonly IStringLocalizer<StreakIndicator> _localizer;

    public StreakIndicatorTests()
    {
        _localizer = Substitute.For<IStringLocalizer<StreakIndicator>>();
        _localizer["AtRisk"].Returns(new LocalizedString("AtRisk", "⚠️ Ve hrozbě"));
        _localizer["TimeRemaining"].Returns(new LocalizedString("TimeRemaining", "Zbývá"));
        _localizer["ShieldActive"].Returns(new LocalizedString("ShieldActive", "🛡️ Štít aktivní"));
        _localizer["ShieldExpired"].Returns(new LocalizedString("ShieldExpired", "Štít expiroval"));
        _localizer["FreezeAvailable"].Returns(new LocalizedString("FreezeAvailable", "❄️ Freeze dostupný"));
        _localizer["ActivateShield"].Returns(new LocalizedString("ActivateShield", "Aktivovat štít"));
        _localizer["BuyShields"].Returns(new LocalizedString("BuyShields", "Koupit štíty"));
        _localizer["FreeShieldAvailable"].Returns(new LocalizedString("FreeShieldAvailable", "Dostupný zdarma"));
        _localizer["PremiumShieldAvailable"].Returns(new LocalizedString("PremiumShieldAvailable", "Dostupný (Premium)"));
        _localizer["NextShieldAvailable"].Returns(new LocalizedString("NextShieldAvailable", "Další za"));
        _localizer["Days"].Returns(new LocalizedString("Days", "dní"));
        Services.AddSingleton(_localizer);
    }

    [Fact]
    public void StreakIndicator_Renders_FlamesBasedOnStreak()
    {
        // Arrange
        var component = Render<StreakIndicator>(parameters => parameters
            .Add(p => p.CurrentDays, 5)
            .Add(p => p.FireLevel, "Medium")
            .Add(p => p.ShieldProtection, new StreakProtectionDto(0, false, false, null, false)));

        // Act & Assert
        component.FindAll(".flame").Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void StreakIndicator_WithShieldActive_ShowsShieldIcon()
    {
        // Arrange
        var shieldProtection = new StreakProtectionDto(
            ShieldsRemaining: 1,
            HasActiveShield: true,
            FreezeUsedThisWeek: false,
            NextShieldAvailableAt: null,
            CanActivateFreeShield: false);

        var component = Render<StreakIndicator>(parameters => parameters
            .Add(p => p.CurrentDays, 10)
            .Add(p => p.FireLevel, "Large")
            .Add(p => p.ShieldProtection, shieldProtection));

        // Act & Assert
        component.Find(".shield-active").Should().NotBeNull();
        component.Markup.Should().Contain("🛡️");
    }

    [Fact]
    public void StreakIndicator_WithoutShield_HidesShieldIcon()
    {
        // Arrange
        var shieldProtection = new StreakProtectionDto(
            ShieldsRemaining: 0,
            HasActiveShield: false,
            FreezeUsedThisWeek: false,
            NextShieldAvailableAt: null,
            CanActivateFreeShield: false);

        var component = Render<StreakIndicator>(parameters => parameters
            .Add(p => p.CurrentDays, 10)
            .Add(p => p.FireLevel, "Large")
            .Add(p => p.ShieldProtection, shieldProtection));

        // Act & Assert
        component.FindAll(".shield-active").Should().BeEmpty();
    }

    [Fact]
    public void StreakIndicator_WithFreeShieldAvailable_ShowsActivateButton()
    {
        // Arrange
        var shieldProtection = new StreakProtectionDto(
            ShieldsRemaining: 0,
            HasActiveShield: false,
            FreezeUsedThisWeek: false,
            NextShieldAvailableAt: null,
            CanActivateFreeShield: true);

        var component = Render<StreakIndicator>(parameters => parameters
            .Add(p => p.CurrentDays, 10)
            .Add(p => p.FireLevel, "Large")
            .Add(p => p.ShieldProtection, shieldProtection));

        // Act & Assert
        var button = component.Find(".shield-activate-btn");
        button.Should().NotBeNull();
        button.TextContent.Should().Contain("Aktivovat štít");
    }

    [Fact]
    public void StreakIndicator_ClickActivateShield_InvokesCallback()
    {
        // Arrange
        var shieldProtection = new StreakProtectionDto(
            ShieldsRemaining: 0,
            HasActiveShield: false,
            FreezeUsedThisWeek: false,
            NextShieldAvailableAt: null,
            CanActivateFreeShield: true);

        var callbackInvoked = false;
        var component = Render<StreakIndicator>(parameters => parameters
            .Add(p => p.CurrentDays, 10)
            .Add(p => p.FireLevel, "Large")
            .Add(p => p.ShieldProtection, shieldProtection)
            .Add(p => p.OnActivateShield, () => { callbackInvoked = true; }));

        // Act
        component.Find(".shield-activate-btn").Click();

        // Assert
        callbackInvoked.Should().BeTrue();
    }

    [Fact]
    public void StreakIndicator_WithShieldsRemaining_ShowsCount()
    {
        // Arrange
        var shieldProtection = new StreakProtectionDto(
            ShieldsRemaining: 3,
            HasActiveShield: false,
            FreezeUsedThisWeek: false,
            NextShieldAvailableAt: null,
            CanActivateFreeShield: false);

        var component = Render<StreakIndicator>(parameters => parameters
            .Add(p => p.CurrentDays, 10)
            .Add(p => p.FireLevel, "Large")
            .Add(p => p.ShieldProtection, shieldProtection));

        // Act & Assert
        component.Markup.Should().Contain("3");
        component.Markup.Should().Contain("🛡️");
    }

    [Fact]
    public void StreakIndicator_WithFreezeAvailable_ShowsFreezeBadge()
    {
        // Arrange
        var shieldProtection = new StreakProtectionDto(
            ShieldsRemaining: 0,
            HasActiveShield: false,
            FreezeUsedThisWeek: false,
            NextShieldAvailableAt: null,
            CanActivateFreeShield: false);

        var component = Render<StreakIndicator>(parameters => parameters
            .Add(p => p.CurrentDays, 10)
            .Add(p => p.FireLevel, "Large")
            .Add(p => p.ShieldProtection, shieldProtection)
            .Add(p => p.IsPremium, true));

        // Act & Assert
        component.Find(".freeze-badge").Should().NotBeNull();
        component.Markup.Should().Contain("❄️");
    }

    [Fact]
    public void StreakIndicator_FreezeUsed_HidesFreezeBadge()
    {
        // Arrange
        var shieldProtection = new StreakProtectionDto(
            ShieldsRemaining: 0,
            HasActiveShield: false,
            FreezeUsedThisWeek: true,
            NextShieldAvailableAt: null,
            CanActivateFreeShield: false);

        var component = Render<StreakIndicator>(parameters => parameters
            .Add(p => p.CurrentDays, 10)
            .Add(p => p.FireLevel, "Large")
            .Add(p => p.ShieldProtection, shieldProtection)
            .Add(p => p.IsPremium, true));

        // Act & Assert
        component.FindAll(".freeze-badge").Should().BeEmpty();
    }

    [Fact]
    public void StreakIndicator_NextShieldAvailable_ShowsCountdown()
    {
        // Arrange
        var nextAvailable = DateTime.UtcNow.AddDays(5);
        var shieldProtection = new StreakProtectionDto(
            ShieldsRemaining: 0,
            HasActiveShield: false,
            FreezeUsedThisWeek: false,
            NextShieldAvailableAt: nextAvailable,
            CanActivateFreeShield: false);

        var component = Render<StreakIndicator>(parameters => parameters
            .Add(p => p.CurrentDays, 10)
            .Add(p => p.FireLevel, "Large")
            .Add(p => p.ShieldProtection, shieldProtection));

        // Act & Assert
        component.Markup.Should().Contain("Další za");
        component.Markup.Should().Contain("5");
    }

    [Fact]
    public void StreakIndicator_ClickBuyShields_InvokesCallback()
    {
        // Arrange
        var shieldProtection = new StreakProtectionDto(
            ShieldsRemaining: 0,
            HasActiveShield: false,
            FreezeUsedThisWeek: false,
            NextShieldAvailableAt: null,
            CanActivateFreeShield: false);

        var callbackInvoked = false;
        var component = Render<StreakIndicator>(parameters => parameters
            .Add(p => p.CurrentDays, 10)
            .Add(p => p.FireLevel, "Large")
            .Add(p => p.ShieldProtection, shieldProtection)
            .Add(p => p.OnBuyShields, () => { callbackInvoked = true; }));

        // Act
        component.Find(".buy-shields-btn").Click();

        // Assert
        callbackInvoked.Should().BeTrue();
    }
}
