using FluentAssertions;
using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class TeamStatsTests
{
    private static TeamStats CreateStats(
        int weeklyXP = 0,
        long allTimeXP = 0,
        int rank = 1,
        int totalWins = 0,
        int matchesPlayed = 0) =>
        new TeamStats(weeklyXP, allTimeXP, rank, totalWins, matchesPlayed);

    // --- Constructor ---

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var stats = new TeamStats(100, 5000, 3, 10, 20);

        stats.WeeklyXP.Should().Be(100);
        stats.AllTimeXP.Should().Be(5000);
        stats.Rank.Should().Be(3);
        stats.TotalWins.Should().Be(10);
        stats.MatchesPlayed.Should().Be(20);
        stats.WinRatePercentage.Should().Be(50);
    }

    [Fact]
    public void Constructor_NoMatches_WinRateIsZero()
    {
        var stats = new TeamStats(0, 0, 1, 0, 0);

        stats.WinRatePercentage.Should().Be(0);
    }

    [Fact]
    public void Constructor_AllWins_WinRateIs100()
    {
        var stats = new TeamStats(0, 0, 1, 5, 5);

        stats.WinRatePercentage.Should().Be(100);
    }

    // --- UpdateWeeklyXP ---

    [Fact]
    public void UpdateWeeklyXP_SetsValue()
    {
        var stats = CreateStats();

        stats.UpdateWeeklyXP(500);

        stats.WeeklyXP.Should().Be(500);
    }

    [Fact]
    public void UpdateWeeklyXP_CanSetToZero()
    {
        var stats = CreateStats(weeklyXP: 100);

        stats.UpdateWeeklyXP(0);

        stats.WeeklyXP.Should().Be(0);
    }

    // --- AddToAllTimeXP ---

    [Fact]
    public void AddToAllTimeXP_PositiveAmount_Accumulates()
    {
        var stats = CreateStats(allTimeXP: 1000);

        stats.AddToAllTimeXP(250);

        stats.AllTimeXP.Should().Be(1250);
    }

    [Fact]
    public void AddToAllTimeXP_ZeroAmount_NoChange()
    {
        var stats = CreateStats(allTimeXP: 1000);

        stats.AddToAllTimeXP(0);

        stats.AllTimeXP.Should().Be(1000);
    }

    [Fact]
    public void AddToAllTimeXP_NegativeAmount_IgnoredNoChange()
    {
        var stats = CreateStats(allTimeXP: 1000);

        stats.AddToAllTimeXP(-500);

        stats.AllTimeXP.Should().Be(1000);
    }

    [Fact]
    public void AddToAllTimeXP_MultipleAdds_Accumulates()
    {
        var stats = CreateStats();

        stats.AddToAllTimeXP(100);
        stats.AddToAllTimeXP(200);
        stats.AddToAllTimeXP(300);

        stats.AllTimeXP.Should().Be(600);
    }

    // --- UpdateRank ---

    [Fact]
    public void UpdateRank_SetsRank()
    {
        var stats = CreateStats(rank: 5);

        stats.UpdateRank(1);

        stats.Rank.Should().Be(1);
    }

    // --- AddWin ---

    [Fact]
    public void AddWin_IncrementsTotalWinsAndMatchesPlayed()
    {
        var stats = CreateStats();

        stats.AddWin();

        stats.TotalWins.Should().Be(1);
        stats.MatchesPlayed.Should().Be(1);
    }

    [Fact]
    public void AddWin_RecalculatesWinRate()
    {
        var stats = CreateStats();

        stats.AddWin();

        stats.WinRatePercentage.Should().Be(100);
    }

    [Fact]
    public void AddWin_AfterLoss_RecalculatesCorrectly()
    {
        var stats = CreateStats();
        stats.AddLoss();

        stats.AddWin();

        stats.TotalWins.Should().Be(1);
        stats.MatchesPlayed.Should().Be(2);
        stats.WinRatePercentage.Should().Be(50);
    }

    // --- AddLoss ---

    [Fact]
    public void AddLoss_IncrementsMatchesPlayedOnly()
    {
        var stats = CreateStats();

        stats.AddLoss();

        stats.TotalWins.Should().Be(0);
        stats.MatchesPlayed.Should().Be(1);
    }

    [Fact]
    public void AddLoss_RecalculatesWinRate()
    {
        var stats = CreateStats();

        stats.AddLoss();

        stats.WinRatePercentage.Should().Be(0);
    }

    [Fact]
    public void AddLoss_AfterWins_ReducesWinRate()
    {
        var stats = CreateStats();
        stats.AddWin();
        stats.AddWin();

        stats.AddLoss();

        stats.WinRatePercentage.Should().Be(66); // 2/3 = 66% (int truncation)
    }

    // --- WinRate edge cases ---

    [Fact]
    public void WinRate_1Win3Losses_Is25Percent()
    {
        var stats = CreateStats();
        stats.AddWin();
        stats.AddLoss();
        stats.AddLoss();
        stats.AddLoss();

        stats.WinRatePercentage.Should().Be(25);
    }

    [Fact]
    public void WinRate_ManyGames_CalculatesCorrectly()
    {
        var stats = CreateStats();
        for (int i = 0; i < 7; i++) stats.AddWin();
        for (int i = 0; i < 3; i++) stats.AddLoss();

        stats.WinRatePercentage.Should().Be(70);
        stats.TotalWins.Should().Be(7);
        stats.MatchesPlayed.Should().Be(10);
    }
}
