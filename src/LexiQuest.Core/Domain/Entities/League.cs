using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.Entities;

public class League
{
    public Guid Id { get; private set; }
    public LeagueTier Tier { get; private set; }
    public DateTime WeekStart { get; private set; }
    public DateTime WeekEnd { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private readonly List<LeagueParticipant> _participants = new();
    public IReadOnlyCollection<LeagueParticipant> Participants => _participants.AsReadOnly();
    
    public bool IsFull => _participants.Count >= 30;

    private League() { } // EF Core constructor

    public static League Create(LeagueTier tier, DateTime weekStart, DateTime weekEnd)
    {
        if (weekStart >= weekEnd)
            throw new ArgumentException("Week start must be before week end");

        return new League
        {
            Id = Guid.NewGuid(),
            Tier = tier,
            WeekStart = weekStart,
            WeekEnd = weekEnd,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddParticipant(Guid userId)
    {
        if (_participants.Any(p => p.UserId == userId))
            throw new InvalidOperationException("User is already in this league");

        if (IsFull)
            throw new InvalidOperationException("League is full");

        var participant = LeagueParticipant.Create(userId, Id);
        _participants.Add(participant);
    }

    public void RemoveParticipant(Guid userId)
    {
        var participant = _participants.FirstOrDefault(p => p.UserId == userId);
        if (participant != null)
        {
            _participants.Remove(participant);
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateRanks()
    {
        var rankedParticipants = _participants
            .OrderByDescending(p => p.WeeklyXP)
            .ThenBy(p => p.CreatedAt)
            .ToList();

        for (int i = 0; i < rankedParticipants.Count; i++)
        {
            rankedParticipants[i].SetRank(i + 1);
        }
    }

    public List<LeagueParticipant> GetTopParticipants(int count)
    {
        return _participants
            .OrderBy(p => p.Rank)
            .Take(count)
            .ToList();
    }

    public List<LeagueParticipant> GetBottomParticipants(int count)
    {
        return _participants
            .OrderByDescending(p => p.Rank)
            .Take(count)
            .ToList();
    }
}

public class LeagueParticipant
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid LeagueId { get; private set; }
    public int WeeklyXP { get; private set; }
    public int Rank { get; private set; }
    public bool IsPromoted { get; private set; }
    public bool IsDemoted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private LeagueParticipant() { } // EF Core constructor

    public static LeagueParticipant Create(Guid userId, Guid leagueId)
    {
        return new LeagueParticipant
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LeagueId = leagueId,
            WeeklyXP = 0,
            Rank = 0,
            IsPromoted = false,
            IsDemoted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddXP(int xp)
    {
        if (xp < 0)
            throw new ArgumentException("XP cannot be negative");
        
        WeeklyXP += xp;
    }

    public void SetRank(int rank)
    {
        if (rank < 1)
            throw new ArgumentException("Rank must be positive");
        
        Rank = rank;
    }

    public void MarkAsPromoted()
    {
        IsPromoted = true;
        IsDemoted = false;
    }

    public void MarkAsDemoted()
    {
        IsDemoted = true;
        IsPromoted = false;
    }

    public void ResetWeeklyXP()
    {
        WeeklyXP = 0;
        Rank = 0;
        IsPromoted = false;
        IsDemoted = false;
    }
}
