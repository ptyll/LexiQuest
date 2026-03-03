namespace LexiQuest.Core.Domain.ValueObjects;

public class Streak
{
    public int CurrentDays { get; private set; }
    public int LongestDays { get; private set; }
    public DateTime? LastActivityDate { get; private set; }

    private Streak() { }

    public static Streak CreateDefault()
    {
        return new Streak
        {
            CurrentDays = 0,
            LongestDays = 0,
            LastActivityDate = null
        };
    }

    public void RecordActivity(DateTime utcNow)
    {
        var today = utcNow.Date;

        if (LastActivityDate == null)
        {
            CurrentDays = 1;
            LastActivityDate = today;
        }
        else if (LastActivityDate.Value.Date == today)
        {
            // Already recorded today
            return;
        }
        else if (LastActivityDate.Value.Date == today.AddDays(-1))
        {
            // Consecutive day
            CurrentDays++;
            LastActivityDate = today;
        }
        else
        {
            // Streak broken
            CurrentDays = 1;
            LastActivityDate = today;
        }

        if (CurrentDays > LongestDays)
        {
            LongestDays = CurrentDays;
        }
    }

    public void Reset()
    {
        CurrentDays = 0;
        LastActivityDate = null;
    }
}
