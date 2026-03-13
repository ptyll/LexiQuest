using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Game;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LexiQuest.Blazor.Tests.Helpers;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Components;

public class LevelUpModalTests : BunitContext
{
    private readonly IStringLocalizer<LevelUpModal> _localizer;

    public LevelUpModalTests()
    {
        _localizer = Substitute.For<IStringLocalizer<LevelUpModal>>();
        _localizer["Title"].Returns(new LocalizedString("Title", "Level Up!"));
        _localizer["NewLevel", Arg.Any<object[]>()].Returns(x => new LocalizedString("NewLevel", $"You reached level {x.Arg<object[]>()[0]}!"));
        _localizer["Continue"].Returns(new LocalizedString("Continue", "Continue"));
        _localizer["Unlocks.Title"].Returns(new LocalizedString("Unlocks.Title", "Unlocked:"));
        
        Services.AddSingleton(_localizer);
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void LevelUpModal_Renders_NewLevel()
    {
        // Arrange
        var xpEvent = new XPGainedEvent(
            Amount: 100,
            Source: "Game",
            LeveledUp: true,
            NewLevel: 3,
            TotalXP: 250,
            Unlocks: null
        );

        // Act
        var cut = Render<LevelUpModal>(parameters => parameters
            .Add(p => p.XpEvent, xpEvent)
            .Add(p => p.IsVisible, true)
        );

        // Assert
        cut.Find(".levelup-title").TextContent.Should().Contain("Level Up!");
        cut.Find(".levelup-level").TextContent.Should().Contain("3");
    }

    [Fact]
    public void LevelUpModal_ShowsUnlocks_WhenAvailable()
    {
        // Arrange
        var unlocks = new List<UnlockableReward>
        {
            new("Path", "Path2", "Intermediate path unlocked")
        };
        var xpEvent = new XPGainedEvent(
            Amount: 100,
            Source: "Game",
            LeveledUp: true,
            NewLevel: 3,
            TotalXP: 250,
            Unlocks: unlocks
        );

        // Act
        var cut = Render<LevelUpModal>(parameters => parameters
            .Add(p => p.XpEvent, xpEvent)
            .Add(p => p.IsVisible, true)
        );

        // Assert
        cut.Find(".levelup-unlocks").Should().NotBeNull();
        cut.Find(".unlock-item").TextContent.Should().Contain("Intermediate path unlocked");
    }

    [Fact]
    public void LevelUpModal_Hidden_WhenNotVisible()
    {
        // Arrange
        var xpEvent = new XPGainedEvent(
            Amount: 100,
            Source: "Game",
            LeveledUp: true,
            NewLevel: 3,
            TotalXP: 250,
            Unlocks: null
        );

        // Act
        var cut = Render<LevelUpModal>(parameters => parameters
            .Add(p => p.XpEvent, xpEvent)
            .Add(p => p.IsVisible, false)
        );

        // Assert
        cut.FindAll(".levelup-modal").Should().BeEmpty();
    }
}
