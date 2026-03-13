using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Multiplayer;
using LexiQuest.Blazor.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using LexiQuest.Blazor.Tests.Helpers;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class SeriesScoreTests : BunitContext
{
    private readonly IStringLocalizer<Multiplayer> _localizer;

    public SeriesScoreTests()
    {
        _localizer = Substitute.For<IStringLocalizer<Multiplayer>>();
        SetupLocalizer();
        
        Services.AddSingleton(_localizer);
        TempoTestHelper.RegisterTempoServices(Services);
    }

    private void SetupLocalizer()
    {
        _localizer["Room_Series_Score"].Returns(new LocalizedString("Room_Series_Score", "Série: {0}:{1}"));
        _localizer["Room_Series_GameOf"].Returns(new LocalizedString("Room_Series_GameOf", "Hra {0} z {1}"));
        _localizer["MatchHistory_Result_Win"].Returns(new LocalizedString("MatchHistory_Result_Win", "Výhra"));
        _localizer["MatchHistory_Result_Loss"].Returns(new LocalizedString("MatchHistory_Result_Loss", "Prohra"));
        _localizer["Room_Rematch_Request"].Returns(new LocalizedString("Room_Rematch_Request", "Chci odvetu!"));
        _localizer["Room_Rematch_Accept"].Returns(new LocalizedString("Room_Rematch_Accept", "Přijmout"));
        _localizer["Room_Rematch_Decline"].Returns(new LocalizedString("Room_Rematch_Decline", "Odmítnout"));
    }

    [Fact]
    public void SeriesScore_BestOf3_ShowsScoreAndGameNumber()
    {
        // Arrange
        var cut = Render<SeriesScore>(parameters => parameters
            .Add(p => p.CurrentGame, 2)
            .Add(p => p.BestOf, 3)
            .Add(p => p.Player1Wins, 1)
            .Add(p => p.Player2Wins, 0)
            .Add(p => p.Player1Username, "Player1")
            .Add(p => p.Player2Username, "Player2"));

        // Assert
        cut.Find(".game-indicator").TextContent.Should().Contain("Hra 2 z 3");
        cut.Find(".series-score").TextContent.Should().Contain("Série: 1:0");
    }

    [Fact]
    public void SeriesScore_ShowsPlayerAvatars()
    {
        // Arrange
        var cut = Render<SeriesScore>(parameters => parameters
            .Add(p => p.CurrentGame, 1)
            .Add(p => p.BestOf, 3)
            .Add(p => p.Player1Wins, 0)
            .Add(p => p.Player2Wins, 0)
            .Add(p => p.Player1Username, "Player1")
            .Add(p => p.Player2Username, "Player2"));

        // Assert
        cut.FindAll(".player-avatar").Count.Should().Be(2);
    }

    [Fact]
    public void SeriesScore_Player1Leading_HighlightsPlayer1()
    {
        // Arrange
        var cut = Render<SeriesScore>(parameters => parameters
            .Add(p => p.CurrentGame, 2)
            .Add(p => p.BestOf, 3)
            .Add(p => p.Player1Wins, 1)
            .Add(p => p.Player2Wins, 0)
            .Add(p => p.Player1Username, "Player1")
            .Add(p => p.Player2Username, "Player2"));

        // Assert
        cut.Find(".player1-score").ClassList.Should().Contain("leading");
        cut.Find(".player2-score").ClassList.Should().NotContain("leading");
    }

    [Fact]
    public void SeriesScore_SeriesComplete_ShowsFinalResult()
    {
        // Arrange
        var cut = Render<SeriesScore>(parameters => parameters
            .Add(p => p.CurrentGame, 3)
            .Add(p => p.BestOf, 3)
            .Add(p => p.Player1Wins, 2)
            .Add(p => p.Player2Wins, 1)
            .Add(p => p.Player1Username, "Player1")
            .Add(p => p.Player2Username, "Player2")
            .Add(p => p.IsSeriesComplete, true));

        // Assert
        cut.Find(".series-complete").Should().NotBeNull();
        cut.Find(".winner-announcement").TextContent.Should().Contain("Výhra");
    }

    [Fact]
    public void SeriesScore_NotComplete_ShowsNextGameCountdown()
    {
        // Arrange
        var cut = Render<SeriesScore>(parameters => parameters
            .Add(p => p.CurrentGame, 1)
            .Add(p => p.BestOf, 3)
            .Add(p => p.Player1Wins, 1)
            .Add(p => p.Player2Wins, 0)
            .Add(p => p.Player1Username, "Player1")
            .Add(p => p.Player2Username, "Player2")
            .Add(p => p.IsSeriesComplete, false)
            .Add(p => p.NextGameCountdown, 5));

        // Assert
        cut.Find(".next-game-countdown").TextContent.Should().Contain("5");
    }

    [Fact]
    public void SeriesScore_BestOf1_HidesSeriesScore()
    {
        // Arrange
        var cut = Render<SeriesScore>(parameters => parameters
            .Add(p => p.CurrentGame, 1)
            .Add(p => p.BestOf, 1)
            .Add(p => p.Player1Wins, 0)
            .Add(p => p.Player2Wins, 0)
            .Add(p => p.Player1Username, "Player1")
            .Add(p => p.Player2Username, "Player2"));

        // Assert
        cut.FindAll(".series-score-display").Count.Should().Be(0);
    }
}
