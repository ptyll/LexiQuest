namespace LexiQuest.Core.Domain.Entities;

/// <summary>
/// Represents a member of a team.
/// </summary>
public class TeamMember
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TeamId { get; private set; }
    public TeamRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public int WeeklyXP { get; private set; }
    public long AllTimeXP { get; private set; }
    public int Wins { get; private set; }

    private TeamMember() { } // EF Core constructor

    public static TeamMember Create(Guid userId, Guid teamId, TeamRole role = TeamRole.Member)
    {
        return new TeamMember
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TeamId = teamId,
            Role = role,
            JoinedAt = DateTime.UtcNow,
            WeeklyXP = 0,
            AllTimeXP = 0,
            Wins = 0
        };
    }

    public void UpdateRole(TeamRole role)
    {
        Role = role;
    }

    public void AddWeeklyXP(int xp)
    {
        if (xp > 0)
        {
            WeeklyXP += xp;
        }
    }

    public void ResetWeeklyXP()
    {
        WeeklyXP = 0;
    }

    public void AddToAllTimeXP(int xp)
    {
        if (xp > 0)
        {
            AllTimeXP += xp;
        }
    }

    public void IncrementWins()
    {
        Wins++;
    }
}

/// <summary>
/// Roles within a team.
/// </summary>
public enum TeamRole
{
    Member = 0,
    Officer = 1,
    Leader = 2
}
