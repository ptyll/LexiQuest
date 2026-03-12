namespace LexiQuest.Core.Domain.Entities;

/// <summary>
/// Represents a team/clan in the game.
/// </summary>
public class Team
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Tag { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public Guid LeaderId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<TeamMember> _members = new();
    public IReadOnlyCollection<TeamMember> Members => _members.AsReadOnly();

    private const int MaxMembers = 20;
    private const int MinNameLength = 3;
    private const int MaxNameLength = 30;
    private const int MinTagLength = 2;
    private const int MaxTagLength = 4;

    private Team() { } // EF Core constructor

    public static Team? Create(string name, string tag, Guid leaderId)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name) || 
            name.Length < MinNameLength || 
            name.Length > MaxNameLength)
        {
            return null;
        }

        // Validate tag
        if (string.IsNullOrWhiteSpace(tag) || 
            tag.Length < MinTagLength || 
            tag.Length > MaxTagLength ||
            !tag.All(c => char.IsUpper(c) || char.IsDigit(c)) ||
            !tag.All(char.IsLetterOrDigit))
        {
            return null;
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Tag = tag.ToUpper().Trim(),
            LeaderId = leaderId,
            CreatedAt = DateTime.UtcNow
        };

        // Add leader as first member
        team._members.Add(TeamMember.Create(leaderId, team.Id, TeamRole.Leader));

        return team;
    }

    public void UpdateDetails(string name, string? description, string? logoUrl)
    {
        if (!string.IsNullOrWhiteSpace(name) && 
            name.Length >= MinNameLength && 
            name.Length <= MaxNameLength)
        {
            Name = name.Trim();
        }

        Description = description?.Trim();
        LogoUrl = logoUrl?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMember(Guid userId, TeamRole role)
    {
        if (_members.Count >= MaxMembers)
        {
            throw new InvalidOperationException($"Team has reached the maximum member limit of {MaxMembers}.");
        }

        if (_members.Any(m => m.UserId == userId))
        {
            throw new InvalidOperationException("User is already a member of this team.");
        }

        _members.Add(TeamMember.Create(userId, Id, role));
    }

    public void RemoveMember(Guid userId)
    {
        if (userId == LeaderId)
        {
            throw new InvalidOperationException("Team leader cannot be removed. Transfer leadership first.");
        }

        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            _members.Remove(member);
        }
    }

    public void TransferLeadership(Guid newLeaderId)
    {
        var newLeader = _members.FirstOrDefault(m => m.UserId == newLeaderId);
        if (newLeader == null)
        {
            throw new InvalidOperationException("New leader must be a member of the team.");
        }

        // Demote current leader to officer
        var currentLeader = _members.First(m => m.UserId == LeaderId);
        currentLeader.UpdateRole(TeamRole.Officer);

        // Promote new leader
        newLeader.UpdateRole(TeamRole.Leader);
        LeaderId = newLeaderId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMemberRole(Guid userId, TeamRole newRole)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
        {
            throw new InvalidOperationException("User is not a member of this team.");
        }

        // Cannot change leader role directly - use TransferLeadership
        if (member.Role == TeamRole.Leader && newRole != TeamRole.Leader)
        {
            throw new InvalidOperationException("Use TransferLeadership to change the team leader.");
        }

        member.UpdateRole(newRole);
        UpdatedAt = DateTime.UtcNow;
    }

    public TeamRole? GetMemberRole(Guid userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId)?.Role;
    }

    public bool CanManageMembers(Guid userId)
    {
        var role = GetMemberRole(userId);
        return role == TeamRole.Leader || role == TeamRole.Officer;
    }

    public bool IsLeader(Guid userId)
    {
        return userId == LeaderId;
    }

    public bool HasMember(Guid userId)
    {
        return _members.Any(m => m.UserId == userId);
    }

    public int GetWeeklyXP()
    {
        return _members.Sum(m => m.WeeklyXP);
    }

    public long GetAllTimeXP()
    {
        return _members.Sum(m => m.AllTimeXP);
    }
}
