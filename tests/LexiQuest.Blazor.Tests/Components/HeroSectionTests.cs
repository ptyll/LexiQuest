using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Landing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class HeroSectionTests : TestContext
{
    private readonly IStringLocalizer<HeroSection> _localizer;

    public HeroSectionTests()
    {
        _localizer = Substitute.For<IStringLocalizer<HeroSection>>();
        _localizer["Hero.Tagline"].Returns(new LocalizedString("Hero.Tagline", "Rozlušti slova, získej moc"));
        _localizer["Hero.Subtitle"].Returns(new LocalizedString("Hero.Subtitle", "Procvič si češtinu zábavnou formou!"));
        _localizer["Hero.CTA.Register"].Returns(new LocalizedString("Hero.CTA.Register", "Zaregistrovat se"));
        _localizer["Hero.CTA.TryFree"].Returns(new LocalizedString("Hero.CTA.TryFree", "Zahrát si bez registrace"));
        _localizer["Hero.SocialProof"].Returns(new LocalizedString("Hero.SocialProof", "10 000+ hráčů"));
        
        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
    }

    [Fact]
    public void HeroSection_Renders_Tagline()
    {
        // Act
        var cut = Render<HeroSection>();

        // Assert
        cut.Find("[data-testid='hero-tagline']").TextContent.Should().Be("Rozlušti slova, získej moc");
    }

    [Fact]
    public void HeroSection_Renders_Subtitle()
    {
        // Act
        var cut = Render<HeroSection>();

        // Assert
        cut.Find("[data-testid='hero-subtitle']").TextContent.Should().Be("Procvič si češtinu zábavnou formou!");
    }

    [Fact]
    public void HeroSection_Renders_CTAButtons()
    {
        // Act
        var cut = Render<HeroSection>();

        // Assert
        var registerBtn = cut.Find("[data-testid='hero-cta-register']");
        registerBtn.TextContent.Should().Contain("Zaregistrovat se");
        
        var guestBtn = cut.Find("[data-testid='hero-cta-guest']");
        guestBtn.TextContent.Should().Contain("Zahrát si bez registrace");
    }

    [Fact]
    public void HeroSection_Renders_SocialProof()
    {
        // Act
        var cut = Render<HeroSection>();

        // Assert
        cut.Find("[data-testid='hero-social-proof']").TextContent.Should().Contain("10 000+ hráčů");
    }

    [Fact]
    public void HeroSection_Renders_Logo()
    {
        // Act
        var cut = Render<HeroSection>();

        // Assert
        cut.Find("[data-testid='hero-logo']").Should().NotBeNull();
    }

    [Fact]
    public void HeroSection_RegisterButton_Click_NavigatesToRegister()
    {
        // Arrange
        var navigationCalled = false;
        var cut = Render<HeroSection>(parameters =>
            parameters.Add(p => p.OnRegisterClick, () => { navigationCalled = true; }));

        // Act
        cut.Find("[data-testid='hero-cta-register'] button").Click();

        // Assert
        navigationCalled.Should().BeTrue();
    }

    [Fact]
    public void HeroSection_GuestButton_Click_NavigatesToGuest()
    {
        // Arrange
        var navigationCalled = false;
        var cut = Render<HeroSection>(parameters =>
            parameters.Add(p => p.OnGuestClick, () => { navigationCalled = true; }));

        // Act
        cut.Find("[data-testid='hero-cta-guest'] button").Click();

        // Assert
        navigationCalled.Should().BeTrue();
    }
}