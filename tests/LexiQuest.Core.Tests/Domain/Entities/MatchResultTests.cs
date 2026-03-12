using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class MatchResultTests
{
    private static readonly Guid Player1Id = Guid.NewGuid();
    private static readonly Guid Player2Id = Guid.NewGuid();

    private static MatchResult CreateResult(
        Guid? winnerId = null,
        bool isDraw = false,
        int p1Score = 5,
        int p2Score = 3,
        int? seriesP1Wins = null,
        int? seriesP2Wins = null)
    {
        return MatchResult.Create(
            matchId: Guid.NewGuid(),
            player1Id: Player1Id,
            player2Id: Player2Id,
            player1Username: "player1",
            player2Username: "player2",
            player1Score: p1Score,
            player2Score: p2Score,
            player1Time: TimeSpan.FromSeconds(120),
            player2Time: TimeSpan.FromSeconds(150),
            player1MaxCombo: 3,
            player2MaxCombo: 2,
            winnerId: winnerId,
            isDraw: isDraw,
            player1XpEarned: 100,
            player2XpEarned: 50,
            player1LeagueXpEarned: 20,
            player2LeagueXpEarned: 10,
            isPrivateRoom: false,
            roomCode: null,
            seriesPlayer1Wins: seriesP1Wins,
            seriesPlayer2Wins: seriesP2Wins,
            wordCount: 10,
            timeLimitMinutes: 5,
            difficulty: DifficultyLevel.Intermediate,
            startedAt: DateTime.UtcNow.AddMinutes(-5));
    }

    // --- Create ---

    [Fact]
    public void Create_SetsAllProperties()
    {
        var matchId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow.AddMinutes(-5);

        var result = MatchResult.Create(
            matchId, Player1Id, Player2Id,
            "p1", "p2",
            7, 4,
            TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(130),
            5, 3,
            Player1Id, false,
            150, 80,
            30, 15,
            true, "ABCD",
            2, 1,
            10, 5,
            DifficultyLevel.Advanced,
            startedAt);

        result.Id.Should().NotBeEmpty();
        result.MatchId.Should().Be(matchId);
        result.Player1Id.Should().Be(Player1Id);
        result.Player2Id.Should().Be(Player2Id);
        result.Player1Username.Should().Be("p1");
        result.Player2Username.Should().Be("p2");
        result.Player1Score.Should().Be(7);
        result.Player2Score.Should().Be(4);
        result.WinnerId.Should().Be(Player1Id);
        result.IsDraw.Should().BeFalse();
        result.IsPrivateRoom.Should().BeTrue();
        result.RoomCode.Should().Be("ABCD");
        result.SeriesPlayer1Wins.Should().Be(2);
        result.SeriesPlayer2Wins.Should().Be(1);
        result.WordCount.Should().Be(10);
        result.TimeLimitMinutes.Should().Be(5);
        result.Difficulty.Should().Be(DifficultyLevel.Advanced);
        result.StartedAt.Should().Be(startedAt);
        result.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    // --- GetResultForPlayer ---

    [Fact]
    public void GetResultForPlayer_WinnerPerspective_ReturnsWin()
    {
        var result = CreateResult(winnerId: Player1Id);

        result.GetResultForPlayer(Player1Id).Should().Be(MatchResultType.Win);
    }

    [Fact]
    public void GetResultForPlayer_LoserPerspective_ReturnsLoss()
    {
        var result = CreateResult(winnerId: Player1Id);

        result.GetResultForPlayer(Player2Id).Should().Be(MatchResultType.Loss);
    }

    [Fact]
    public void GetResultForPlayer_Draw_ReturnsDraw()
    {
        var result = CreateResult(isDraw: true);

        result.GetResultForPlayer(Player1Id).Should().Be(MatchResultType.Draw);
        result.GetResultForPlayer(Player2Id).Should().Be(MatchResultType.Draw);
    }

    [Fact]
    public void GetResultForPlayer_DrawTakesPrecedence_OverWinnerId()
    {
        // If isDraw is true, it doesn't matter what winnerId is
        var result = CreateResult(winnerId: Player1Id, isDraw: true);

        result.GetResultForPlayer(Player1Id).Should().Be(MatchResultType.Draw);
    }

    // --- GetOpponentId ---

    [Fact]
    public void GetOpponentId_FromPlayer1_ReturnsPlayer2()
    {
        var result = CreateResult();

        result.GetOpponentId(Player1Id).Should().Be(Player2Id);
    }

    [Fact]
    public void GetOpponentId_FromPlayer2_ReturnsPlayer1()
    {
        var result = CreateResult();

        result.GetOpponentId(Player2Id).Should().Be(Player1Id);
    }

    // --- GetOpponentUsername ---

    [Fact]
    public void GetOpponentUsername_FromPlayer1_ReturnsPlayer2Username()
    {
        var result = CreateResult();

        result.GetOpponentUsername(Player1Id).Should().Be("player2");
    }

    [Fact]
    public void GetOpponentUsername_FromPlayer2_ReturnsPlayer1Username()
    {
        var result = CreateResult();

        result.GetOpponentUsername(Player2Id).Should().Be("player1");
    }

    // --- GetPlayerScore ---

    [Fact]
    public void GetPlayerScore_Player1_ReturnsPlayer1Score()
    {
        var result = CreateResult(p1Score: 8, p2Score: 3);

        result.GetPlayerScore(Player1Id).Should().Be(8);
    }

    [Fact]
    public void GetPlayerScore_Player2_ReturnsPlayer2Score()
    {
        var result = CreateResult(p1Score: 8, p2Score: 3);

        result.GetPlayerScore(Player2Id).Should().Be(3);
    }

    // --- GetOpponentScore ---

    [Fact]
    public void GetOpponentScore_Player1_ReturnsPlayer2Score()
    {
        var result = CreateResult(p1Score: 8, p2Score: 3);

        result.GetOpponentScore(Player1Id).Should().Be(3);
    }

    [Fact]
    public void GetOpponentScore_Player2_ReturnsPlayer1Score()
    {
        var result = CreateResult(p1Score: 8, p2Score: 3);

        result.GetOpponentScore(Player2Id).Should().Be(8);
    }

    // --- GetPlayerXPEarned ---

    [Fact]
    public void GetPlayerXPEarned_Player1_ReturnsPlayer1XP()
    {
        var result = CreateResult();

        result.GetPlayerXPEarned(Player1Id).Should().Be(100);
    }

    [Fact]
    public void GetPlayerXPEarned_Player2_ReturnsPlayer2XP()
    {
        var result = CreateResult();

        result.GetPlayerXPEarned(Player2Id).Should().Be(50);
    }

    // --- GetSeriesScore ---

    [Fact]
    public void GetSeriesScore_WithSeriesData_Player1Perspective()
    {
        var result = CreateResult(seriesP1Wins: 3, seriesP2Wins: 1);

        var score = result.GetSeriesScore(Player1Id);

        score.Should().NotBeNull();
        score!.Value.YourWins.Should().Be(3);
        score!.Value.OpponentWins.Should().Be(1);
    }

    [Fact]
    public void GetSeriesScore_WithSeriesData_Player2Perspective()
    {
        var result = CreateResult(seriesP1Wins: 3, seriesP2Wins: 1);

        var score = result.GetSeriesScore(Player2Id);

        score.Should().NotBeNull();
        score!.Value.YourWins.Should().Be(1);
        score!.Value.OpponentWins.Should().Be(3);
    }

    [Fact]
    public void GetSeriesScore_NoSeriesData_ReturnsNull()
    {
        var result = CreateResult();

        var score = result.GetSeriesScore(Player1Id);

        score.Should().BeNull();
    }

    [Fact]
    public void GetSeriesScore_OnlyOneSeriesValue_ReturnsNull()
    {
        var result = CreateResult(seriesP1Wins: 2, seriesP2Wins: null);

        var score = result.GetSeriesScore(Player1Id);

        score.Should().BeNull();
    }

    // --- SetPlayerAvatars ---

    [Fact]
    public void SetPlayerAvatars_SetsBothAvatars()
    {
        var result = CreateResult();

        result.SetPlayerAvatars("avatar1.png", "avatar2.png");

        result.Player1Avatar.Should().Be("avatar1.png");
        result.Player2Avatar.Should().Be("avatar2.png");
    }

    [Fact]
    public void SetPlayerAvatars_NullAvatars_Allowed()
    {
        var result = CreateResult();

        result.SetPlayerAvatars(null, null);

        result.Player1Avatar.Should().BeNull();
        result.Player2Avatar.Should().BeNull();
    }

    // --- GetOpponentAvatar ---

    [Fact]
    public void GetOpponentAvatar_FromPlayer1_ReturnsPlayer2Avatar()
    {
        var result = CreateResult();
        result.SetPlayerAvatars("a1.png", "a2.png");

        result.GetOpponentAvatar(Player1Id).Should().Be("a2.png");
    }

    [Fact]
    public void GetOpponentAvatar_FromPlayer2_ReturnsPlayer1Avatar()
    {
        var result = CreateResult();
        result.SetPlayerAvatars("a1.png", "a2.png");

        result.GetOpponentAvatar(Player2Id).Should().Be("a1.png");
    }

    // --- Duration ---

    [Fact]
    public void Duration_CalculatedFromStartAndCompletion()
    {
        var result = CreateResult();

        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        result.Duration.Should().BeCloseTo(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(5));
    }
}
