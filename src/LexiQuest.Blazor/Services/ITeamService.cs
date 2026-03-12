using LexiQuest.Shared.DTOs.Teams;

namespace LexiQuest.Blazor.Services;

public interface ITeamService
{
    Task<TeamDto?> GetMyTeamAsync();
    Task<TeamDto?> GetTeamByIdAsync(Guid id);
    Task<List<TeamMemberDto>> GetTeamMembersAsync(Guid teamId);
    Task<TeamDto?> CreateTeamAsync(CreateTeamRequest request);
    Task<bool> LeaveTeamAsync(Guid teamId);
    Task<bool> DisbandTeamAsync(Guid teamId);
    Task<bool> InviteMemberAsync(Guid teamId, InviteMemberRequest request);
    Task<bool> KickMemberAsync(Guid teamId, Guid userId);
    Task<bool> TransferLeadershipAsync(Guid teamId, Guid newLeaderId);
    Task<bool> RequestJoinAsync(Guid teamId, CreateJoinRequest request);
    Task<bool> ApproveJoinRequestAsync(Guid requestId);
    Task<bool> RejectJoinRequestAsync(Guid requestId);
    Task<List<TeamInviteDto>> GetMyInvitesAsync();
    Task<List<TeamJoinRequestDto>> GetJoinRequestsAsync(Guid teamId);
    Task<List<TeamRankingDto>> GetRankingAsync();
    Task<bool> CanCreateTeamAsync();
}
