namespace LexiQuest.Core.Domain.Entities;

/// <summary>
/// Represents a request to join a team.
/// </summary>
public class TeamJoinRequest
{
    public Guid Id { get; private set; }
    public Guid TeamId { get; private set; }
    public Guid UserId { get; private set; }
    public string? Message { get; private set; }
    public JoinRequestStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private TeamJoinRequest() { } // EF Core constructor

    public static TeamJoinRequest Create(Guid teamId, Guid userId, string? message)
    {
        return new TeamJoinRequest
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = userId,
            Message = message?.Trim(),
            Status = JoinRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        if (Status != JoinRequestStatus.Pending)
        {
            throw new InvalidOperationException("Only pending requests can be approved.");
        }

        Status = JoinRequestStatus.Approved;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != JoinRequestStatus.Pending)
        {
            throw new InvalidOperationException("Only pending requests can be rejected.");
        }

        Status = JoinRequestStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }

    public bool IsPending => Status == JoinRequestStatus.Pending;
}

/// <summary>
/// Status of a team join request.
/// </summary>
public enum JoinRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
