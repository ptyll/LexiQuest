using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Teams;

namespace LexiQuest.Core.Services;

/// <summary>
/// Service for managing teams.
/// </summary>
public class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TeamService(ITeamRepository teamRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _teamRepository = teamRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TeamDto?> CreateTeamAsync(Guid userId, CreateTeamRequest request, CancellationToken cancellationToken = default)
    {
        // Check if user can create team (premium or has enough coins)
        var canCreate = await CanUserCreateTeamAsync(userId, cancellationToken);
        if (!canCreate)
        {
            return null;
        }

        // Check if user is already in a team
        var existingTeam = await _teamRepository.GetTeamByMemberAsync(userId, cancellationToken);
        if (existingTeam != null)
        {
            throw new InvalidOperationException("User is already a member of a team.");
        }

        // Check for duplicate name
        var existingByName = await _teamRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingByName != null)
        {
            throw new InvalidOperationException("Team with this name already exists.");
        }

        // Check for duplicate tag
        var existingByTag = await _teamRepository.GetByTagAsync(request.Tag, cancellationToken);
        if (existingByTag != null)
        {
            throw new InvalidOperationException("Team with this tag already exists.");
        }

        // Create team
        var team = Team.Create(request.Name, request.Tag, userId);
        if (team == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            team.UpdateDetails(request.Name, request.Description, request.LogoUrl);
        }

        await _teamRepository.AddAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetTeamAsync(team.Id, cancellationToken);
    }

    public async Task<TeamDto?> GetTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            return null;
        }

        var leader = await _userRepository.GetByIdAsync(team.LeaderId, cancellationToken);

        return MapToTeamDto(team, leader?.Username ?? "Unknown");
    }

    public async Task<TeamDto?> GetUserTeamAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetTeamByMemberAsync(userId, cancellationToken);
        if (team == null)
        {
            return null;
        }

        return await GetTeamAsync(team.Id, cancellationToken);
    }

    public async Task<TeamDto?> UpdateTeamAsync(Guid teamId, Guid userId, CreateTeamRequest request, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            return null;
        }

        // Only leader or officers can update team
        if (!team.CanManageMembers(userId))
        {
            throw new InvalidOperationException("Only team leaders and officers can update team details.");
        }

        // Check for duplicate name if changed
        if (!string.Equals(team.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existingByName = await _teamRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingByName != null)
            {
                throw new InvalidOperationException("Team with this name already exists.");
            }
        }

        // Check for duplicate tag if changed
        if (!string.Equals(team.Tag, request.Tag, StringComparison.OrdinalIgnoreCase))
        {
            var existingByTag = await _teamRepository.GetByTagAsync(request.Tag, cancellationToken);
            if (existingByTag != null)
            {
                throw new InvalidOperationException("Team with this tag already exists.");
            }
        }

        team.UpdateDetails(request.Name, request.Description, request.LogoUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetTeamAsync(teamId, cancellationToken);
    }

    public async Task<bool> DisbandTeamAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            return false;
        }

        if (!team.IsLeader(userId))
        {
            throw new InvalidOperationException("Only the team leader can disband the team.");
        }

        _teamRepository.Delete(team);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> InviteMemberAsync(Guid teamId, Guid inviterId, InviteMemberRequest request, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            return false;
        }

        if (!team.CanManageMembers(inviterId))
        {
            throw new InvalidOperationException("Only team leaders and officers can invite members.");
        }

        // Check if user is already in a team
        var userExistingTeam = await _teamRepository.GetTeamByMemberAsync(request.UserId, cancellationToken);
        if (userExistingTeam != null)
        {
            throw new InvalidOperationException("User is already a member of another team.");
        }

        // Check if invite already exists
        var existingInvite = await _teamRepository.GetPendingInviteAsync(teamId, request.UserId, cancellationToken);
        if (existingInvite != null)
        {
            throw new InvalidOperationException("An invite is already pending for this user.");
        }

        var invite = TeamInvite.Create(teamId, request.UserId, inviterId);
        await _teamRepository.AddInviteAsync(invite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> AcceptInviteAsync(Guid inviteId, Guid userId, CancellationToken cancellationToken = default)
    {
        var invite = await _teamRepository.GetInviteByIdAsync(inviteId, cancellationToken);
        if (invite == null || invite.InvitedUserId != userId)
        {
            return false;
        }

        invite.Accept();

        // Add user to team
        var team = await _teamRepository.GetByIdAsync(invite.TeamId, cancellationToken);
        if (team == null)
        {
            return false;
        }

        team.AddMember(userId, TeamRole.Member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RejectInviteAsync(Guid inviteId, Guid userId, CancellationToken cancellationToken = default)
    {
        var invite = await _teamRepository.GetInviteByIdAsync(inviteId, cancellationToken);
        if (invite == null || invite.InvitedUserId != userId)
        {
            return false;
        }

        invite.Reject();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CancelInviteAsync(Guid inviteId, Guid cancellerId, CancellationToken cancellationToken = default)
    {
        var invite = await _teamRepository.GetInviteByIdAsync(inviteId, cancellationToken);
        if (invite == null)
        {
            return false;
        }

        var team = await _teamRepository.GetByIdAsync(invite.TeamId, cancellationToken);
        if (team == null || !team.CanManageMembers(cancellerId))
        {
            throw new InvalidOperationException("Only team leaders and officers can cancel invites.");
        }

        invite.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> KickMemberAsync(Guid teamId, Guid kickerId, Guid userId, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            return false;
        }

        if (!team.CanManageMembers(kickerId))
        {
            throw new InvalidOperationException("Only team leaders and officers can kick members.");
        }

        // Officers cannot kick other officers or the leader
        var kickerRole = team.GetMemberRole(kickerId);
        var targetRole = team.GetMemberRole(userId);

        if (kickerRole == TeamRole.Officer && targetRole != TeamRole.Member)
        {
            throw new InvalidOperationException("Officers can only kick regular members.");
        }

        team.RemoveMember(userId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> LeaveTeamAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            return false;
        }

        if (team.IsLeader(userId))
        {
            // If leader is leaving and there are other members, transfer leadership first
            if (team.Members.Count > 1)
            {
                throw new InvalidOperationException("Leader must transfer leadership before leaving the team.");
            }

            // If leader is the only member, disband the team
            _teamRepository.Delete(team);
        }
        else
        {
            team.RemoveMember(userId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> TransferLeadershipAsync(Guid teamId, Guid currentLeaderId, Guid newLeaderId, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            return false;
        }

        if (!team.IsLeader(currentLeaderId))
        {
            throw new InvalidOperationException("Only the team leader can transfer leadership.");
        }

        team.TransferLeadership(newLeaderId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateMemberRoleAsync(Guid teamId, Guid updaterId, Guid userId, TeamRoleDto newRole, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            return false;
        }

        if (!team.IsLeader(updaterId))
        {
            throw new InvalidOperationException("Only the team leader can update member roles.");
        }

        team.UpdateMemberRole(userId, (TeamRole)newRole);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CreateJoinRequestAsync(Guid teamId, Guid userId, CreateJoinRequest request, CancellationToken cancellationToken = default)
    {
        // Check if user is already in a team
        var existingTeam = await _teamRepository.GetTeamByMemberAsync(userId, cancellationToken);
        if (existingTeam != null)
        {
            throw new InvalidOperationException("User is already a member of a team.");
        }

        // Check if request already exists
        var existingRequest = await _teamRepository.GetPendingJoinRequestAsync(teamId, userId, cancellationToken);
        if (existingRequest != null)
        {
            throw new InvalidOperationException("A join request is already pending.");
        }

        var joinRequest = TeamJoinRequest.Create(teamId, userId, request.Message);
        await _teamRepository.AddJoinRequestAsync(joinRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ApproveJoinRequestAsync(Guid requestId, Guid approverId, CancellationToken cancellationToken = default)
    {
        var joinRequest = await _teamRepository.GetJoinRequestByIdAsync(requestId, cancellationToken);
        if (joinRequest == null)
        {
            return false;
        }

        var team = await _teamRepository.GetByIdAsync(joinRequest.TeamId, cancellationToken);
        if (team == null || !team.CanManageMembers(approverId))
        {
            throw new InvalidOperationException("Only team leaders and officers can approve join requests.");
        }

        joinRequest.Approve();

        // Add user to team
        team.AddMember(joinRequest.UserId, TeamRole.Member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RejectJoinRequestAsync(Guid requestId, Guid rejecterId, CancellationToken cancellationToken = default)
    {
        var joinRequest = await _teamRepository.GetJoinRequestByIdAsync(requestId, cancellationToken);
        if (joinRequest == null)
        {
            return false;
        }

        var team = await _teamRepository.GetByIdAsync(joinRequest.TeamId, cancellationToken);
        if (team == null || !team.CanManageMembers(rejecterId))
        {
            throw new InvalidOperationException("Only team leaders and officers can reject join requests.");
        }

        joinRequest.Reject();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<TeamRankingDto>> GetTeamRankingAsync(int top = 100, CancellationToken cancellationToken = default)
    {
        var teams = await _teamRepository.GetTopTeamsByWeeklyXPAsync(top, cancellationToken);
        var result = new List<TeamRankingDto>();

        int rank = 1;
        foreach (var team in teams)
        {
            result.Add(new TeamRankingDto(
                team.Id,
                team.Name,
                team.Tag,
                rank++,
                team.GetWeeklyXP(),
                team.GetAllTimeXP(),
                team.Members.Count));
        }

        return result.AsReadOnly();
    }

    public async Task<IReadOnlyList<TeamMemberDto>> GetTeamMembersAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team == null)
        {
            return new List<TeamMemberDto>().AsReadOnly();
        }

        var result = new List<TeamMemberDto>();
        foreach (var member in team.Members)
        {
            var user = await _userRepository.GetByIdAsync(member.UserId, cancellationToken);
            if (user != null)
            {
                result.Add(new TeamMemberDto(
                    member.UserId,
                    user.Username,
                    user.AvatarUrl,
                    (TeamRoleDto)member.Role,
                    member.JoinedAt,
                    member.WeeklyXP,
                    member.AllTimeXP));
            }
        }

        return result.AsReadOnly();
    }

    public async Task<IReadOnlyList<TeamInviteDto>> GetPendingInvitesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var invites = await _teamRepository.GetPendingInvitesForUserAsync(userId, cancellationToken);
        var result = new List<TeamInviteDto>();

        foreach (var invite in invites)
        {
            var team = await _teamRepository.GetByIdAsync(invite.TeamId, cancellationToken);
            var inviter = await _userRepository.GetByIdAsync(invite.InvitedByUserId, cancellationToken);

            if (team != null)
            {
                result.Add(new TeamInviteDto(
                    invite.Id,
                    invite.TeamId,
                    team.Name,
                    team.Tag,
                    invite.InvitedByUserId,
                    inviter?.Username ?? "Unknown",
                    (InviteStatusDto)invite.Status,
                    invite.CreatedAt,
                    invite.ExpiresAt,
                    invite.IsExpired));
            }
        }

        return result.AsReadOnly();
    }

    public async Task<IReadOnlyList<TeamJoinRequestDto>> GetPendingJoinRequestsAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, cancellationToken);
        if (team == null || !team.CanManageMembers(userId))
        {
            return new List<TeamJoinRequestDto>().AsReadOnly();
        }

        var requests = await _teamRepository.GetPendingJoinRequestsForTeamAsync(teamId, cancellationToken);
        var result = new List<TeamJoinRequestDto>();

        foreach (var request in requests)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user != null)
            {
                result.Add(new TeamJoinRequestDto(
                    request.Id,
                    request.TeamId,
                    request.UserId,
                    user.Username,
                    request.Message,
                    (JoinRequestStatusDto)request.Status,
                    request.CreatedAt));
            }
        }

        return result.AsReadOnly();
    }

    public async Task<bool> CanUserCreateTeamAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        // Premium users can create teams for free
        if (user.Premium != null && user.Premium.IsActive(DateTime.UtcNow))
        {
            return true;
        }

        // Non-premium users need 1000 coins
        return user.CoinBalance >= 1000;
    }

    public async Task<bool> CanUserJoinTeamAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var existingTeam = await _teamRepository.GetTeamByMemberAsync(userId, cancellationToken);
        return existingTeam == null;
    }

    private TeamDto MapToTeamDto(Team team, string leaderUsername)
    {
        return new TeamDto(
            team.Id,
            team.Name,
            team.Tag,
            team.Description,
            team.LogoUrl,
            team.LeaderId,
            leaderUsername,
            team.CreatedAt,
            team.Members.Count,
            new TeamStatsDto(
                team.GetWeeklyXP(),
                team.GetAllTimeXP(),
                0, // Rank will be calculated separately
                team.Members.Sum(m => m.Wins),
                0, // MatchesPlayed not tracked yet
                0)); // WinRatePercentage not tracked yet
    }
}
