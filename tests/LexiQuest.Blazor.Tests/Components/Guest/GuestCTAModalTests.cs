using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Guest;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components.Guest;

/// <summary>
/// Tests for GuestCTAModal component (T-302.2).
/// </summary>
public class GuestCTAModalTests : TestContext
{
    private readonly IStringLocalizer<GuestCTAModal> _localizer;

    public GuestCTAModalTests()
    {
        _localizer = Substitute.For<IStringLocalizer<GuestCTAModal>>();
        _localizer["Title"].Returns(new LocalizedString("Title", "Skvělé!"));
        _localizer["Description"].Returns(new LocalizedString("Description", "Zaregistrujte se a získejte plný přístup!"));
        _localizer["Benefit_SaveProgress"].Returns(new LocalizedString("Benefit_SaveProgress", "Ukládání pokroku"));
        _localizer["Benefit_Achievements"].Returns(new LocalizedString("Benefit_Achievements", "Achievementy"));
        _localizer["Benefit_Leagues"].Returns(new LocalizedString("Benefit_Leagues", "Ligy"));
        _localizer["Benefit_Stats"].Returns(new LocalizedString("Benefit_Stats", "Statistiky"));
        _localizer["Later"].Returns(new LocalizedString("Later", "Možná později"));
        _localizer["Register"].Returns(new LocalizedString("Register", "Zaregistrovat se"));

        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
        Services.AddSingleton(Substitute.For<NavigationManager>());
    }

    [Fact]
    public void GuestCTAModal_Renders_WhenIsOpenTrue()
    {
        // Act
        var cut = Render<GuestCTAModal>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        cut.Find("[data-testid='guest-cta-modal']").Should().NotBeNull();
    }

    [Fact]
    public void GuestCTAModal_DoesNotRender_WhenIsOpenFalse()
    {
        // Act
        var cut = Render<GuestCTAModal>(parameters => parameters
            .Add(p => p.IsOpen, false));

        // Assert
        cut.FindAll("[data-testid='guest-cta-modal']").Count.Should().Be(0);
    }

    [Fact]
    public void GuestCTAModal_Renders_Title()
    {
        // Act
        var cut = Render<GuestCTAModal>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        cut.Find("h3").TextContent.Should().Be("Skvělé!");
    }

    [Fact]
    public void GuestCTAModal_Renders_Description()
    {
        // Act
        var cut = Render<GuestCTAModal>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        var modalContent = cut.Find("[data-testid='guest-cta-modal']");
        modalContent.TextContent.Should().Contain("Zaregistrujte se a získejte plný přístup!");
    }

    [Fact]
    public void GuestCTAModal_Renders_Benefits()
    {
        // Act
        var cut = Render<GuestCTAModal>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        var modalContent = cut.Find("[data-testid='guest-cta-modal']");
        modalContent.TextContent.Should().Contain("Ukládání pokroku");
        modalContent.TextContent.Should().Contain("Achievementy");
        modalContent.TextContent.Should().Contain("Ligy");
        modalContent.TextContent.Should().Contain("Statistiky");
    }

    [Fact]
    public void GuestCTAModal_RegisterButton_TriggersOnRegister()
    {
        // Arrange
        var registerClicked = false;
        var cut = Render<GuestCTAModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnRegister, () => { registerClicked = true; }));

        // Act - click the button inside the register div
        cut.Find("[data-testid='btn-register'] button").Click();

        // Assert
        registerClicked.Should().BeTrue();
    }

    [Fact]
    public void GuestCTAModal_LaterButton_TriggersOnLater()
    {
        // Arrange
        var laterClicked = false;
        var cut = Render<GuestCTAModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnLater, () => { laterClicked = true; }));

        // Act - click the button inside the later div
        cut.Find("[data-testid='btn-later'] button").Click();

        // Assert
        laterClicked.Should().BeTrue();
    }

    [Fact]
    public void GuestCTAModal_CloseButton_TriggersOnClose()
    {
        // Arrange
        var closeClicked = false;
        var cut = Render<GuestCTAModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, () => { closeClicked = true; }));

        // Act
        cut.Find(".btn-close").Click();

        // Assert
        closeClicked.Should().BeTrue();
    }
}
