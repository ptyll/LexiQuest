using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Landing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class FinalCTASectionTests : TestContext
{
    private readonly IStringLocalizer<FinalCTASection> _localizer;

    public FinalCTASectionTests()
    {
        _localizer = Substitute.For<IStringLocalizer<FinalCTASection>>();
        _localizer["CTA.Title"].Returns(new LocalizedString("CTA.Title", "Připraven začít?"));
        _localizer["CTA.Subtitle"].Returns(new LocalizedString("CTA.Subtitle", "Zaregistruj se zdarma a získej:"));
        _localizer["CTA.Benefit1"].Returns(new LocalizedString("CTA.Benefit1", "Neomezený počet her"));
        _localizer["CTA.Benefit2"].Returns(new LocalizedString("CTA.Benefit2", "Ukládání XP a progressu"));
        _localizer["CTA.Benefit3"].Returns(new LocalizedString("CTA.Benefit3", "Účast v ligách a žebříčcích"));
        _localizer["CTA.Benefit4"].Returns(new LocalizedString("CTA.Benefit4", "Denní streak a achievementy"));
        _localizer["CTA.Benefit5"].Returns(new LocalizedString("CTA.Benefit5", "Přístup ke všem učebním cestám"));
        _localizer["CTA.Button"].Returns(new LocalizedString("CTA.Button", "Vytvořit účet zdarma"));
        
        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
    }

    [Fact]
    public void FinalCTASection_Renders_Title()
    {
        // Act
        var cut = Render<FinalCTASection>();

        // Assert
        cut.Find("[data-testid='cta-title']").TextContent.Should().Be("Připraven začít?");
    }

    [Fact]
    public void FinalCTASection_Renders_Subtitle()
    {
        // Act
        var cut = Render<FinalCTASection>();

        // Assert
        cut.Find("[data-testid='cta-subtitle']").TextContent.Should().Be("Zaregistruj se zdarma a získej:");
    }

    [Fact]
    public void FinalCTASection_Renders_5Benefits()
    {
        // Act
        var cut = Render<FinalCTASection>();

        // Assert
        var benefits = cut.FindAll("[data-testid='cta-benefit']");
        benefits.Count.Should().Be(5);
    }

    [Fact]
    public void FinalCTASection_Benefits_HaveCheckmarks()
    {
        // Act
        var cut = Render<FinalCTASection>();

        // Assert
        var checkmarks = cut.FindAll(".benefit-check");
        checkmarks.Count.Should().Be(5);
    }

    [Fact]
    public void FinalCTASection_Renders_RegisterButton()
    {
        // Act
        var cut = Render<FinalCTASection>();

        // Assert
        var button = cut.Find("[data-testid='cta-button']");
        button.TextContent.Should().Contain("Vytvořit účet zdarma");
    }

    [Fact]
    public void FinalCTASection_HasGradientBackground()
    {
        // Act
        var cut = Render<FinalCTASection>();

        // Assert
        var section = cut.Find("[data-testid='cta-section']");
        section.ClassList.Should().Contain("cta-section");
    }
}
