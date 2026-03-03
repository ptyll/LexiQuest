using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Game;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Components;

public class XpBarTests : BunitContext
{
    private readonly IStringLocalizer<XpBar> _localizer;

    public XpBarTests()
    {
        _localizer = Substitute.For<IStringLocalizer<XpBar>>();
        _localizer["XP.Current", Arg.Any<object[]>()].Returns(x => new LocalizedString("XP.Current", $"{x.Arg<object[]>()[0]}/{x.Arg<object[]>()[1]} XP"));
        _localizer["Level", Arg.Any<object[]>()].Returns(x => new LocalizedString("Level", $"Level {x.Arg<object[]>()[0]}"));
        
        Services.AddSingleton(_localizer);
    }

    [Fact]
    public void XpBar_Renders_CurrentXPAndLevel()
    {
        // Arrange
        var progress = new XPProgress(
            TotalXP: 150,
            CurrentLevel: 2,
            XPInCurrentLevel: 50,
            XPRequiredForNextLevel: 150,
            ProgressPercentage: 33
        );

        // Act
        var cut = Render<XpBar>(parameters => parameters
            .Add(p => p.Progress, progress)
        );

        // Assert
        cut.Find(".xp-level").TextContent.Should().Contain("2");
        cut.Find(".xp-text").TextContent.Should().Contain("50/150");
    }

    [Fact]
    public void XpBar_ShowsCorrectProgress_Percentage()
    {
        // Arrange
        var progress = new XPProgress(
            TotalXP: 125,
            CurrentLevel: 2,
            XPInCurrentLevel: 25,
            XPRequiredForNextLevel: 150,
            ProgressPercentage: 16
        );

        // Act
        var cut = Render<XpBar>(parameters => parameters
            .Add(p => p.Progress, progress)
        );

        // Assert
        var progressBar = cut.Find(".xp-progress-fill");
        progressBar.GetAttribute("style").Should().Contain("16%");
    }

    [Fact]
    public void XpBar_ZeroProgress_ShowsEmptyBar()
    {
        // Arrange
        var progress = new XPProgress(
            TotalXP: 0,
            CurrentLevel: 1,
            XPInCurrentLevel: 0,
            XPRequiredForNextLevel: 100,
            ProgressPercentage: 0
        );

        // Act
        var cut = Render<XpBar>(parameters => parameters
            .Add(p => p.Progress, progress)
        );

        // Assert
        var progressBar = cut.Find(".xp-progress-fill");
        progressBar.GetAttribute("style").Should().Contain("0%");
    }
}
