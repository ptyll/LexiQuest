using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Landing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class FeaturesSectionTests : TestContext
{
    private readonly IStringLocalizer<FeaturesSection> _localizer;

    public FeaturesSectionTests()
    {
        _localizer = Substitute.For<IStringLocalizer<FeaturesSection>>();
        _localizer["Features.Title"].Returns(new LocalizedString("Features.Title", "Proč hrát LexiQuest?"));
        _localizer["Features.Tab1.Title"].Returns(new LocalizedString("Features.Tab1.Title", "RPG Progress"));
        _localizer["Features.Tab1.Item1"].Returns(new LocalizedString("Features.Tab1.Item1", "✨ Systém XP a levelů"));
        _localizer["Features.Tab1.Item2"].Returns(new LocalizedString("Features.Tab1.Item2", "🎯 Odemykej nová témata"));
        _localizer["Features.Tab1.Item3"].Returns(new LocalizedString("Features.Tab1.Item3", "🏆 Sbírej achievementy"));
        _localizer["Features.Tab2.Title"].Returns(new LocalizedString("Features.Tab2.Title", "Souboje"));
        _localizer["Features.Tab2.Item1"].Returns(new LocalizedString("Features.Tab2.Item1", "⚔️ Boss levely"));
        _localizer["Features.Tab2.Item2"].Returns(new LocalizedString("Features.Tab2.Item2", "⏱️ Maraton"));
        _localizer["Features.Tab2.Item3"].Returns(new LocalizedString("Features.Tab2.Item3", "🎲 Twist"));
        _localizer["Features.Tab3.Title"].Returns(new LocalizedString("Features.Tab3.Title", "Soutěže"));
        _localizer["Features.Tab3.Item1"].Returns(new LocalizedString("Features.Tab3.Item1", "🏅 Týdenní ligy"));
        _localizer["Features.Tab3.Item2"].Returns(new LocalizedString("Features.Tab3.Item2", "📊 Žebříčky"));
        _localizer["Features.Tab3.Item3"].Returns(new LocalizedString("Features.Tab3.Item3", "🎁 Odměny"));
        
        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
    }

    [Fact]
    public void FeaturesSection_Renders_Title()
    {
        // Act
        var cut = Render<FeaturesSection>();

        // Assert
        cut.Find("[data-testid='features-title']").TextContent.Should().Be("Proč hrát LexiQuest?");
    }

    [Fact]
    public void FeaturesSection_Renders_Section()
    {
        // Act
        var cut = Render<FeaturesSection>();

        // Assert
        cut.Find("[data-testid='features-section']").Should().NotBeNull();
    }

    [Fact]
    public void FeaturesSection_DefaultTab_ShowsRPGContent()
    {
        // Act
        var cut = Render<FeaturesSection>();

        // Assert - RPG is default active tab
        var panel = cut.Find("[data-testid='tab-panel-rpg']");
        panel.TextContent.Should().Contain("Systém XP a levelů");
        panel.TextContent.Should().Contain("Odemykej nová témata");
        panel.TextContent.Should().Contain("Sbírej achievementy");
    }

    [Fact]
    public void FeaturesSection_HasTabsContainer()
    {
        // Act
        var cut = Render<FeaturesSection>();

        // Assert - check for TmTabs component rendered
        cut.Find(".tm-tabs").Should().NotBeNull();
    }

    [Fact]
    public void FeaturesSection_RPGTab_Has3FeatureItems()
    {
        // Act
        var cut = Render<FeaturesSection>();

        // Assert
        var panel = cut.Find("[data-testid='tab-panel-rpg']");
        var items = panel.QuerySelectorAll(".feature-list li");
        items.Length.Should().Be(3);
    }

    [Fact]
    public void FeaturesSection_RPGTab_HasScreenshotPlaceholder()
    {
        // Act
        var cut = Render<FeaturesSection>();

        // Assert
        var panel = cut.Find("[data-testid='tab-panel-rpg']");
        var placeholder = panel.QuerySelector(".screenshot-placeholder");
        placeholder.Should().NotBeNull();
    }
}
