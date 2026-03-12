namespace LexiQuest.Core.Domain.Entities;

/// <summary>
/// Represents an invitation to join a team.
/// </summary>
public class TeamInvite
{
    public Guid Id { get; private set; }
    public Guid TeamId { get; private set; }
    public Guid InvitedUserId { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public InviteStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private TeamInvite() { } // EF Core constructor

    public static TeamInvite Create(Guid teamId, Guid invitedUserId, Guid invitedByUserId)
    {
        return new TeamInvite
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            InvitedUserId = invitedUserId,
            InvitedByUserId = invitedByUserId,
            Status = InviteStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
    }

    public void Accept()
    {
        if (Status != InviteStatus.Pending)
        {
            throw new InvalidOperationException("Only pending invites can be accepted.");
        }

        if (IsExpired)
        {
            throw new InvalidOperationException("Invite has expired.");
        }

        Status = InviteStatus.Accepted;
    }

    public void Reject()
    {
        if (Status != InviteStatus.Pending)
        {
            throw new InvalidOperationException("Only pending invites can be rejected.");
        }

        Status = InviteStatus.Rejected;
    }

    public void Cancel()
    {
        if (Status != InviteStatus.Pending)
        {
            throw new InvalidOperationException("Only pending invites can be cancelled.");
        }

        Status = InviteStatus.Cancelled;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsPending => Status == InviteStatus.Pending && !IsExpired;
}

/// <summary>
/// Status of a team invite.
/// </summary>
public enum InviteStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Cancelled = 3,
    Expired = 4
}
