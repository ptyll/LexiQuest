using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Landing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class FooterTests : TestContext
{
    private readonly IStringLocalizer<Footer> _localizer;

    public FooterTests()
    {
        _localizer = Substitute.For<IStringLocalizer<Footer>>();
        _localizer["Footer.About"].Returns(new LocalizedString("Footer.About", "O nás"));
        _localizer["Footer.Terms"].Returns(new LocalizedString("Footer.Terms", "Podmínky použití"));
        _localizer["Footer.Privacy"].Returns(new LocalizedString("Footer.Privacy", "Ochrana soukromí"));
        _localizer["Footer.Contact"].Returns(new LocalizedString("Footer.Contact", "Kontakt"));
        _localizer["Footer.Copyright"].Returns(new LocalizedString("Footer.Copyright", "© 2026 LexiQuest. Všechna práva vyhrazena."));
        
        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
    }

    [Fact]
    public void Footer_Renders_Logo()
    {
        // Act
        var cut = Render<Footer>();

        // Assert
        cut.Find("[data-testid='footer-logo']").Should().NotBeNull();
    }

    [Fact]
    public void Footer_Renders_NavLinks()
    {
        // Act
        var cut = Render<Footer>();

        // Assert
        cut.Find("[data-testid='footer-about']").TextContent.Should().Be("O nás");
        cut.Find("[data-testid='footer-terms']").TextContent.Should().Be("Podmínky použití");
        cut.Find("[data-testid='footer-privacy']").TextContent.Should().Be("Ochrana soukromí");
        cut.Find("[data-testid='footer-contact']").TextContent.Should().Be("Kontakt");
    }

    [Fact]
    public void Footer_Renders_SocialIcons()
    {
        // Act
        var cut = Render<Footer>();

        // Assert
        cut.Find("[data-testid='footer-social-github']").Should().NotBeNull();
        cut.Find("[data-testid='footer-social-twitter']").Should().NotBeNull();
        cut.Find("[data-testid='footer-social-discord']").Should().NotBeNull();
    }

    [Fact]
    public void Footer_Renders_Copyright()
    {
        // Act
        var cut = Render<Footer>();

        // Assert
        cut.Find("[data-testid='footer-copyright']").TextContent.Should().Be("© 2026 LexiQuest. Všechna práva vyhrazena.");
    }

    [Fact]
    public void Footer_HasCorrectStructure()
    {
        // Act
        var cut = Render<Footer>();

        // Assert
        cut.Find("footer").Should().NotBeNull();
        cut.Find("[data-testid='footer-container']").Should().NotBeNull();
    }
}
