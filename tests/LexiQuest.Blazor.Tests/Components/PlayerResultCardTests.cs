using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components;
using LexiQuest.Shared.DTOs.Multiplayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class PlayerResultCardTests : BunitContext
{
    private readonly IStringLocalizer<MatchResult> _localizer;

    public PlayerResultCardTests()
    {
        _localizer = Substitute.For<IStringLocalizer<MatchResult>>();
        _localizer["Score_Correct"].Returns(new LocalizedString("Score_Correct", "Správně"));
        
        Services.AddSingleton(_localizer);
    }

    [Fact]
    public void PlayerResultCard_Renders_PlayerInfo()
    {
        // Arrange
        var player = new PlayerMatchResult(
            Username: "TestPlayer",
            Avatar: null,
            CorrectCount: 10,
            TotalTime: TimeSpan.FromSeconds(120),
            ComboMax: 3,
            XPEarned: 100
        );
        
        // Act
        var cut = Render<PlayerResultCard>(parameters => parameters
            .Add(p => p.Player, player)
            .Add(p => p.IsWinner, false));
        
        // Assert
        cut.Find(".player-name").TextContent.Should().Be("TestPlayer");
        cut.Find(".stat-value").TextContent.Should().Be("10");
    }

    [Fact]
    public void PlayerResultCard_IsWinner_ShowsWinnerBadge()
    {
        // Arrange
        var player = new PlayerMatchResult(
            Username: "Winner",
            Avatar: null,
            CorrectCount: 15,
            TotalTime: TimeSpan.FromSeconds(100),
            ComboMax: 5,
            XPEarned: 150
        );
        
        // Act
        var cut = Render<PlayerResultCard>(parameters => parameters
            .Add(p => p.Player, player)
            .Add(p => p.IsWinner, true));
        
        // Assert
        cut.Find(".winner-badge").Should().NotBeNull();
        cut.Find(".player-card").ClassList.Should().Contain("winner");
    }

    [Fact]
    public void PlayerResultCard_NotWinner_NoWinnerBadge()
    {
        // Arrange
        var player = new PlayerMatchResult(
            Username: "Loser",
            Avatar: null,
            CorrectCount: 5,
            TotalTime: TimeSpan.FromSeconds(180),
            ComboMax: 1,
            XPEarned: 30
        );
        
        // Act
        var cut = Render<PlayerResultCard>(parameters => parameters
            .Add(p => p.Player, player)
            .Add(p => p.IsWinner, false));
        
        // Assert
        cut.FindAll(".winner-badge").Count.Should().Be(0);
        cut.Find(".player-card").ClassList.Should().NotContain("winner");
    }

    [Fact]
    public void PlayerResultCard_FormatsTime_Correctly()
    {
        // Arrange
        var player = new PlayerMatchResult(
            Username: "Player",
            Avatar: null,
            CorrectCount: 8,
            TotalTime: TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(30)),
            ComboMax: 2,
            XPEarned: 80
        );
        
        // Act
        var cut = Render<PlayerResultCard>(parameters => parameters
            .Add(p => p.Player, player)
            .Add(p => p.IsWinner, false));
        
        // Assert
        var timeValue = cut.FindAll(".stat-value")[1].TextContent;
        timeValue.Should().Be("02:30");
    }
}
