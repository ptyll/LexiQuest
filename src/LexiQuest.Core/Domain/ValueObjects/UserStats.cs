namespace LexiQuest.Core.Domain.ValueObjects;

public class UserStats
{
    public int TotalXP { get; private set; }
    public int Level { get; private set; }
    public double Accuracy { get; private set; }
    public int TotalWordsSolved { get; private set; }
    public TimeSpan AverageResponseTime { get; private set; }
    public string League { get; private set; } = "Bronze";

    private UserStats() { }

    public static UserStats CreateDefault()
    {
        return new UserStats
        {
            TotalXP = 0,
            Level = 1,
            Accuracy = 0,
            TotalWordsSolved = 0,
            AverageResponseTime = TimeSpan.Zero,
            League = "Bronze"
        };
    }

    public void AddXP(int xp)
    {
        TotalXP += xp;
        RecalculateLevel();
    }

    public void UpdateAccuracy(bool isCorrect)
    {
        var total = TotalWordsSolved + 1;
        var correctCount = (int)(Accuracy * TotalWordsSolved / 100.0) + (isCorrect ? 1 : 0);
        Accuracy = Math.Round((double)correctCount / total * 100, 2);
        TotalWordsSolved = total;
    }

    public void UpdateAverageResponseTime(TimeSpan responseTime)
    {
        if (TotalWordsSolved <= 1)
        {
            AverageResponseTime = responseTime;
            return;
        }

        var totalMs = AverageResponseTime.TotalMilliseconds * (TotalWordsSolved - 1) + responseTime.TotalMilliseconds;
        AverageResponseTime = TimeSpan.FromMilliseconds(totalMs / TotalWordsSolved);
    }

    private void RecalculateLevel()
    {
        // Level formula: each level requires level * 100 XP
        var xpRemaining = TotalXP;
        var level = 1;
        while (xpRemaining >= level * 100)
        {
            xpRemaining -= level * 100;
            level++;
        }
        Level = level;
    }

    // Test helper
    internal void SetLevel(int level) => Level = level;
}
