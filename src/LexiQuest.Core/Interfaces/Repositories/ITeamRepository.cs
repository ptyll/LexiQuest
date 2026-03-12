using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository for teams.
/// </summary>
public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Team?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Team?> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByMemberAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Team>> GetTopTeamsByWeeklyXPAsync(int top, CancellationToken cancellationToken = default);
    
    Task AddAsync(Team team, CancellationToken cancellationToken = default);
    void Delete(Team team);
    
    // Invites
    Task<TeamInvite?> GetInviteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TeamInvite?> GetPendingInviteAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeamInvite>> GetPendingInvitesForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddInviteAsync(TeamInvite invite, CancellationToken cancellationToken = default);
    
    // Join Requests
    Task<TeamJoinRequest?> GetJoinRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TeamJoinRequest?> GetPendingJoinRequestAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeamJoinRequest>> GetPendingJoinRequestsForTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task AddJoinRequestAsync(TeamJoinRequest request, CancellationToken cancellationToken = default);
}
