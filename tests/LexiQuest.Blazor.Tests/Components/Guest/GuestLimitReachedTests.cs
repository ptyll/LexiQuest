using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Guest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components.Guest;

/// <summary>
/// Tests for GuestLimitReached component (T-302.3).
/// </summary>
public class GuestLimitReachedTests : TestContext
{
    private readonly IStringLocalizer<GuestLimitReached> _localizer;

    public GuestLimitReachedTests()
    {
        _localizer = Substitute.For<IStringLocalizer<GuestLimitReached>>();
        _localizer["Title"].Returns(new LocalizedString("Title", "Denní limit dosažen"));
        _localizer["Description"].Returns(new LocalizedString("Description", "Pro dnešek jste vyčerpali svůj limit her."));
        _localizer["Register"].Returns(new LocalizedString("Register", "Zaregistrovat se"));
        _localizer["Back"].Returns(new LocalizedString("Back", "Zpět na úvodní stránku"));
        _localizer["HaveAccount"].Returns(new LocalizedString("HaveAccount", "Už máte účet?"));
        _localizer["Login"].Returns(new LocalizedString("Login", "Přihlásit se"));

        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
    }

    [Fact]
    public void GuestLimitReached_Renders_Card()
    {
        // Act
        var cut = Render<GuestLimitReached>();

        // Assert
        cut.Find("[data-testid='guest-limit-reached']").Should().NotBeNull();
    }

    [Fact]
    public void GuestLimitReached_Renders_Title()
    {
        // Act
        var cut = Render<GuestLimitReached>();

        // Assert
        cut.Find(".limit-title").TextContent.Should().Be("Denní limit dosažen");
    }

    [Fact]
    public void GuestLimitReached_Renders_Description()
    {
        // Act
        var cut = Render<GuestLimitReached>();

        // Assert
        cut.Find(".limit-description").TextContent.Should().Be("Pro dnešek jste vyčerpali svůj limit her.");
    }

    [Fact]
    public void GuestLimitReached_Renders_RegisterButton()
    {
        // Act
        var cut = Render<GuestLimitReached>();

        // Assert
        var btn = cut.Find("[data-testid='btn-register']");
        btn.TextContent.Should().Contain("Zaregistrovat se");
    }

    [Fact]
    public void GuestLimitReached_RegisterButton_TriggersOnRegister()
    {
        // Arrange
        var registerClicked = false;
        var cut = Render<GuestLimitReached>(parameters => parameters
            .Add(p => p.OnRegister, () => { registerClicked = true; }));

        // Act
        cut.Find("[data-testid='btn-register'] button").Click();

        // Assert
        registerClicked.Should().BeTrue();
    }

    [Fact]
    public void GuestLimitReached_BackButton_TriggersOnBack()
    {
        // Arrange
        var backClicked = false;
        var cut = Render<GuestLimitReached>(parameters => parameters
            .Add(p => p.OnBack, () => { backClicked = true; }));

        // Act
        cut.Find("[data-testid='btn-back'] button").Click();

        // Assert
        backClicked.Should().BeTrue();
    }

    [Fact]
    public void GuestLimitReached_LoginButton_TriggersOnLogin()
    {
        // Arrange
        var loginClicked = false;
        var cut = Render<GuestLimitReached>(parameters => parameters
            .Add(p => p.OnLogin, () => { loginClicked = true; }));

        // Act
        cut.Find("[data-testid='btn-login'] button").Click();

        // Assert
        loginClicked.Should().BeTrue();
    }

    [Fact]
    public void GuestLimitReached_Renders_HaveAccountText()
    {
        // Act
        var cut = Render<GuestLimitReached>();

        // Assert
        cut.Find(".limit-login").TextContent.Should().Contain("Už máte účet?");
    }
}
