using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.Entities;

public class DailyChallenge
{
    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }
    public Guid WordId { get; private set; }
    public DailyModifier Modifier { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private DailyChallenge() { }

    public static DailyChallenge Create(DateTime date, Guid wordId, DailyModifier modifier)
    {
        return new DailyChallenge
        {
            Id = Guid.NewGuid(),
            Date = date,
            WordId = wordId,
            Modifier = modifier,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class DailyChallengeCompletion
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ChallengeDate { get; private set; }
    public TimeSpan TimeTaken { get; private set; }
    public int XPEarned { get; private set; }
    public DateTime CompletedAt { get; private set; }

    private DailyChallengeCompletion() { }

    public static DailyChallengeCompletion Create(Guid userId, DateTime challengeDate, TimeSpan timeTaken, int xpEarned)
    {
        return new DailyChallengeCompletion
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ChallengeDate = challengeDate,
            TimeTaken = timeTaken,
            XPEarned = xpEarned,
            CompletedAt = DateTime.UtcNow
        };
    }
}

public record DailyLeaderboardEntry(Guid UserId, string Username, TimeSpan TimeTaken, int XPEarned);
