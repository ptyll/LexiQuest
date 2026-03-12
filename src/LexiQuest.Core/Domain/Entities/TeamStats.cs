namespace LexiQuest.Core.Domain.Entities;

/// <summary>
/// Statistics for a team.
/// </summary>
public class TeamStats
{
    public int WeeklyXP { get; private set; }
    public long AllTimeXP { get; private set; }
    public int Rank { get; private set; }
    public int TotalWins { get; private set; }
    public int MatchesPlayed { get; private set; }
    public int WinRatePercentage { get; private set; }

    public TeamStats(int weeklyXP, long allTimeXP, int rank, int totalWins, int matchesPlayed = 0)
    {
        WeeklyXP = weeklyXP;
        AllTimeXP = allTimeXP;
        Rank = rank;
        TotalWins = totalWins;
        MatchesPlayed = matchesPlayed;
        WinRatePercentage = matchesPlayed > 0 ? (int)((double)totalWins / matchesPlayed * 100) : 0;
    }

    public void UpdateWeeklyXP(int xp)
    {
        WeeklyXP = xp;
    }

    public void AddToAllTimeXP(int xp)
    {
        if (xp > 0)
        {
            AllTimeXP += xp;
        }
    }

    public void UpdateRank(int rank)
    {
        Rank = rank;
    }

    public void AddWin()
    {
        TotalWins++;
        MatchesPlayed++;
        RecalculateWinRate();
    }

    public void AddLoss()
    {
        MatchesPlayed++;
        RecalculateWinRate();
    }

    private void RecalculateWinRate()
    {
        WinRatePercentage = MatchesPlayed > 0 ? (int)((double)TotalWins / MatchesPlayed * 100) : 0;
    }
}
