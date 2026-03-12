using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Landing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class PathsPreviewSectionTests : TestContext
{
    private readonly IStringLocalizer<PathsPreviewSection> _localizer;

    public PathsPreviewSectionTests()
    {
        _localizer = Substitute.For<IStringLocalizer<PathsPreviewSection>>();
        _localizer["Paths.Title"].Returns(new LocalizedString("Paths.Title", "Vyber si svou cestu"));
        _localizer["Path1.Name"].Returns(new LocalizedString("Path1.Name", "Začátečník"));
        _localizer["Path1.Description"].Returns(new LocalizedString("Path1.Description", "Jednoduchá slova pro rychlý start"));
        _localizer["Path1.Letters"].Returns(new LocalizedString("Path1.Letters", "3-5 písmen"));
        _localizer["Path2.Name"].Returns(new LocalizedString("Path2.Name", "Pokročilý"));
        _localizer["Path2.Description"].Returns(new LocalizedString("Path2.Description", "Středně těžká slova pro procvičení"));
        _localizer["Path2.Letters"].Returns(new LocalizedString("Path2.Letters", "5-7 písmen"));
        _localizer["Path3.Name"].Returns(new LocalizedString("Path3.Name", "Expert"));
        _localizer["Path3.Description"].Returns(new LocalizedString("Path3.Description", "Těžká slova pro zkušené hráče"));
        _localizer["Path3.Letters"].Returns(new LocalizedString("Path3.Letters", "7-10 písmen"));
        _localizer["Path4.Name"].Returns(new LocalizedString("Path4.Name", "Mistr"));
        _localizer["Path4.Description"].Returns(new LocalizedString("Path4.Description", "Nejtěžší výzvy pro pravé mistry"));
        _localizer["Path4.Letters"].Returns(new LocalizedString("Path4.Letters", "10+ písmen"));
        
        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
    }

    [Fact]
    public void PathsPreviewSection_Renders_Title()
    {
        // Act
        var cut = Render<PathsPreviewSection>();

        // Assert
        cut.Find("[data-testid='paths-title']").TextContent.Should().Be("Vyber si svou cestu");
    }

    [Fact]
    public void PathsPreviewSection_Renders_4PathCards()
    {
        // Act
        var cut = Render<PathsPreviewSection>();

        // Assert
        cut.Find("[data-testid='path-card-1']").Should().NotBeNull();
        cut.Find("[data-testid='path-card-2']").Should().NotBeNull();
        cut.Find("[data-testid='path-card-3']").Should().NotBeNull();
        cut.Find("[data-testid='path-card-4']").Should().NotBeNull();
    }

    [Fact]
    public void PathsPreviewSection_Path1_Renders_NameAndDescription()
    {
        // Act
        var cut = Render<PathsPreviewSection>();

        // Assert
        var card = cut.Find("[data-testid='path-card-1']");
        card.TextContent.Should().Contain("Začátečník");
        card.TextContent.Should().Contain("Jednoduchá slova pro rychlý start");
        card.TextContent.Should().Contain("3-5 písmen");
    }

    [Fact]
    public void PathsPreviewSection_Path2_Renders_NameAndDescription()
    {
        // Act
        var cut = Render<PathsPreviewSection>();

        // Assert
        var card = cut.Find("[data-testid='path-card-2']");
        card.TextContent.Should().Contain("Pokročilý");
        card.TextContent.Should().Contain("Středně těžká slova pro procvičení");
        card.TextContent.Should().Contain("5-7 písmen");
    }

    [Fact]
    public void PathsPreviewSection_Path3_Renders_NameAndDescription()
    {
        // Act
        var cut = Render<PathsPreviewSection>();

        // Assert
        var card = cut.Find("[data-testid='path-card-3']");
        card.TextContent.Should().Contain("Expert");
        card.TextContent.Should().Contain("Těžká slova pro zkušené hráče");
        card.TextContent.Should().Contain("7-10 písmen");
    }

    [Fact]
    public void PathsPreviewSection_Path4_Renders_NameAndDescription()
    {
        // Act
        var cut = Render<PathsPreviewSection>();

        // Assert
        var card = cut.Find("[data-testid='path-card-4']");
        card.TextContent.Should().Contain("Mistr");
        card.TextContent.Should().Contain("Nejtěžší výzvy pro pravé mistry");
        card.TextContent.Should().Contain("10+ písmen");
    }

    [Fact]
    public void PathsPreviewSection_AllCards_HaveGradientClass()
    {
        // Act
        var cut = Render<PathsPreviewSection>();

        // Assert
        var cards = cut.FindAll("[data-testid^='path-card-']");
        cards.Count.Should().Be(4);
        foreach (var card in cards)
        {
            card.ClassList.Should().Contain("path-card");
        }
    }
}
