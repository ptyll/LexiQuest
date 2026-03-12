using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Landing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class HowItWorksSectionTests : TestContext
{
    private readonly IStringLocalizer<HowItWorksSection> _localizer;

    public HowItWorksSectionTests()
    {
        _localizer = Substitute.For<IStringLocalizer<HowItWorksSection>>();
        _localizer["HowItWorks.Title"].Returns(new LocalizedString("HowItWorks.Title", "Jak to funguje?"));
        _localizer["HowItWorks.Step1.Title"].Returns(new LocalizedString("HowItWorks.Step1.Title", "Dostaneš zamíchaná písmena"));
        _localizer["HowItWorks.Step1.Description"].Returns(new LocalizedString("HowItWorks.Step1.Description", "Na obrazovce se objeví písmena..."));
        _localizer["HowItWorks.Step2.Title"].Returns(new LocalizedString("HowItWorks.Step2.Title", "Rozlušti slovo"));
        _localizer["HowItWorks.Step2.Description"].Returns(new LocalizedString("HowItWorks.Step2.Description", "Použij svou představivost..."));
        _localizer["HowItWorks.Step3.Title"].Returns(new LocalizedString("HowItWorks.Step3.Title", "Postupuj a soutěž"));
        _localizer["HowItWorks.Step3.Description"].Returns(new LocalizedString("HowItWorks.Step3.Description", "Za každé slovo získáváš body..."));
        
        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
    }

    [Fact]
    public void HowItWorksSection_Renders_Title()
    {
        // Act
        var cut = Render<HowItWorksSection>();

        // Assert
        cut.Find("[data-testid='how-it-works-title']").TextContent.Should().Be("Jak to funguje?");
    }

    [Fact]
    public void HowItWorksSection_Renders_3Steps()
    {
        // Act
        var cut = Render<HowItWorksSection>();

        // Assert
        cut.Find("[data-testid='step-1']").Should().NotBeNull();
        cut.Find("[data-testid='step-2']").Should().NotBeNull();
        cut.Find("[data-testid='step-3']").Should().NotBeNull();
    }

    [Fact]
    public void HowItWorksSection_Step1_Renders_TitleAndDescription()
    {
        // Act
        var cut = Render<HowItWorksSection>();

        // Assert
        var step1 = cut.Find("[data-testid='step-1']");
        step1.TextContent.Should().Contain("Dostaneš zamíchaná písmena");
        step1.TextContent.Should().Contain("Na obrazovce se objeví písmena...");
    }

    [Fact]
    public void HowItWorksSection_Step2_Renders_TitleAndDescription()
    {
        // Act
        var cut = Render<HowItWorksSection>();

        // Assert
        var step2 = cut.Find("[data-testid='step-2']");
        step2.TextContent.Should().Contain("Rozlušti slovo");
        step2.TextContent.Should().Contain("Použij svou představivost...");
    }

    [Fact]
    public void HowItWorksSection_Step3_Renders_TitleAndDescription()
    {
        // Act
        var cut = Render<HowItWorksSection>();

        // Assert
        var step3 = cut.Find("[data-testid='step-3']");
        step3.TextContent.Should().Contain("Postupuj a soutěž");
        step3.TextContent.Should().Contain("Za každé slovo získáváš body...");
    }

    [Fact]
    public void HowItWorksSection_Steps_HaveNumberBadges()
    {
        // Act
        var cut = Render<HowItWorksSection>();

        // Assert
        var badges = cut.FindAll("[data-testid='step-number']");
        badges.Count.Should().Be(3);
        badges[0].TextContent.Trim().Should().Be("1");
        badges[1].TextContent.Trim().Should().Be("2");
        badges[2].TextContent.Trim().Should().Be("3");
    }
}