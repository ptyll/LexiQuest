using LexiQuest.Shared.DTOs.Teams;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing teams.
/// </summary>
public interface ITeamService
{
    // Team Management
    Task<TeamDto?> CreateTeamAsync(Guid userId, CreateTeamRequest request, CancellationToken cancellationToken = default);
    Task<TeamDto?> GetTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<TeamDto?> GetUserTeamAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TeamDto?> UpdateTeamAsync(Guid teamId, Guid userId, CreateTeamRequest request, CancellationToken cancellationToken = default);
    Task<bool> DisbandTeamAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);

    // Member Management
    Task<bool> InviteMemberAsync(Guid teamId, Guid inviterId, InviteMemberRequest request, CancellationToken cancellationToken = default);
    Task<bool> AcceptInviteAsync(Guid inviteId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RejectInviteAsync(Guid inviteId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CancelInviteAsync(Guid inviteId, Guid cancellerId, CancellationToken cancellationToken = default);
    Task<bool> KickMemberAsync(Guid teamId, Guid kickerId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> LeaveTeamAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> TransferLeadershipAsync(Guid teamId, Guid currentLeaderId, Guid newLeaderId, CancellationToken cancellationToken = default);
    Task<bool> UpdateMemberRoleAsync(Guid teamId, Guid updaterId, Guid userId, TeamRoleDto newRole, CancellationToken cancellationToken = default);

    // Join Requests
    Task<bool> CreateJoinRequestAsync(Guid teamId, Guid userId, CreateJoinRequest request, CancellationToken cancellationToken = default);
    Task<bool> ApproveJoinRequestAsync(Guid requestId, Guid approverId, CancellationToken cancellationToken = default);
    Task<bool> RejectJoinRequestAsync(Guid requestId, Guid rejecterId, CancellationToken cancellationToken = default);

    // Queries
    Task<IReadOnlyList<TeamRankingDto>> GetTeamRankingAsync(int top = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeamMemberDto>> GetTeamMembersAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeamInviteDto>> GetPendingInvitesForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeamJoinRequestDto>> GetPendingJoinRequestsAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CanUserCreateTeamAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CanUserJoinTeamAsync(Guid userId, CancellationToken cancellationToken = default);
}
