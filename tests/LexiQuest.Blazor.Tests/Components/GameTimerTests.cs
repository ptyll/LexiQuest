using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Game;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LexiQuest.Blazor.Tests.Helpers;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Components;

public class GameTimerTests : BunitContext
{
    private readonly IStringLocalizer<GameTimer> _localizer;

    public GameTimerTests()
    {
        _localizer = Substitute.For<IStringLocalizer<GameTimer>>();
        _localizer["TimeRemaining"].Returns(new LocalizedString("TimeRemaining", "Time: {0}"));

        Services.AddSingleton(_localizer);
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void GameTimer_Renders_ProgressBar()
    {
        // Arrange & Act
        var cut = Render<GameTimer>(parameters => parameters
            .Add(p => p.TotalSeconds, 30)
            .Add(p => p.RemainingSeconds, 15));

        // Assert
        cut.Find(".timer-bar-container").Should().NotBeNull();
        cut.Find(".timer-bar").Should().NotBeNull();
    }

    [Fact]
    public void GameTimer_Renders_TimeText()
    {
        // Arrange & Act
        var cut = Render<GameTimer>(parameters => parameters
            .Add(p => p.RemainingSeconds, 45));

        // Assert
        cut.Find(".timer-text").TextContent.Should().Contain("00:45");
    }

    [Fact]
    public void GameTimer_HighTime_ShowsNormalClass()
    {
        // Arrange & Act
        var cut = Render<GameTimer>(parameters => parameters
            .Add(p => p.TotalSeconds, 30)
            .Add(p => p.RemainingSeconds, 20));

        // Assert
        cut.Find(".game-timer").ClassList.Should().Contain("timer-normal");
    }

    [Fact]
    public void GameTimer_MediumTime_ShowsWarningClass()
    {
        // Arrange & Act
        var cut = Render<GameTimer>(parameters => parameters
            .Add(p => p.TotalSeconds, 30)
            .Add(p => p.RemainingSeconds, 10));

        // Assert
        cut.Find(".game-timer").ClassList.Should().Contain("timer-warning");
    }

    [Fact]
    public void GameTimer_LowTime_ShowsCriticalClass()
    {
        // Arrange & Act
        var cut = Render<GameTimer>(parameters => parameters
            .Add(p => p.TotalSeconds, 30)
            .Add(p => p.RemainingSeconds, 4));

        // Assert
        cut.Find(".game-timer").ClassList.Should().Contain("timer-critical");
    }

    [Fact]
    public void GameTimer_ProgressBarWidth_CalculatedCorrectly()
    {
        // Arrange & Act
        var cut = Render<GameTimer>(parameters => parameters
            .Add(p => p.TotalSeconds, 30)
            .Add(p => p.RemainingSeconds, 15));

        // Assert
        var bar = cut.Find(".timer-bar");
        bar.GetAttribute("style").Should().Contain("width: 50%");
    }
}
