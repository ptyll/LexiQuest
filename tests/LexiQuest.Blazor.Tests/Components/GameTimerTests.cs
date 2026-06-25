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

    [Fact]
    public async Task GameTimer_Tick_ReportsRemainingSecondsChanged()
    {
        // Arrange
        var reportedSeconds = new List<int>();

        Render<GameTimer>(parameters => parameters
            .Add(p => p.TotalSeconds, 30)
            .Add(p => p.RemainingSeconds, 2)
            .Add(p => p.RemainingSecondsChanged, seconds =>
            {
                reportedSeconds.Add(seconds);
                return Task.CompletedTask;
            }));

        // Act
        await Task.Delay(1200);

        // Assert
        reportedSeconds.Should().Contain(1);
    }

    [Fact]
    public async Task GameTimer_TimeUp_InvokesCallbackOnlyOnce()
    {
        // Arrange
        var timeUpCount = 0;
        var remainingUpdates = new List<int>();

        Render<GameTimer>(parameters => parameters
            .Add(p => p.TotalSeconds, 1)
            .Add(p => p.RemainingSeconds, 1)
            .Add(p => p.RemainingSecondsChanged, seconds =>
            {
                remainingUpdates.Add(seconds);
                return Task.CompletedTask;
            })
            .Add(p => p.OnTimeUp, () =>
            {
                timeUpCount++;
                return Task.CompletedTask;
            }));

        // Act
        await Task.Delay(2300);

        // Assert
        remainingUpdates.Should().Contain(0);
        timeUpCount.Should().Be(1);
    }

    [Fact]
    public void GameTimer_ProgressBar_DoesNotAnimateWidth()
    {
        // Arrange
        var cssPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "LexiQuest.Blazor.Client",
            "Components",
            "Game",
            "GameTimer.razor.css");

        // Act
        var css = File.ReadAllText(cssPath);

        // Assert
        css.Should().NotContain("transition: width");
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LexiQuest.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Repository root with LexiQuest.slnx was not found.");
    }
}
