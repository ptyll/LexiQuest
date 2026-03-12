using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly LexiQuestDbContext _context;

    public TeamRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Team?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    public async Task<Team?> GetByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Tag == tag, cancellationToken);
    }

    public async Task<Team?> GetTeamByMemberAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Members.Any(m => m.UserId == userId), cancellationToken);
    }

    public async Task<IReadOnlyList<Team>> GetTopTeamsByWeeklyXPAsync(int top, CancellationToken cancellationToken = default)
    {
        return await _context.Teams
            .Include(t => t.Members)
            .OrderByDescending(t => t.Members.Sum(m => m.WeeklyXP))
            .Take(top)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        await _context.Teams.AddAsync(team, cancellationToken);
    }

    public void Delete(Team team)
    {
        _context.Teams.Remove(team);
    }

    // Invites
    public async Task<TeamInvite?> GetInviteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TeamInvites
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<TeamInvite?> GetPendingInviteAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamInvites
            .FirstOrDefaultAsync(i => i.TeamId == teamId && i.InvitedUserId == userId && i.Status == InviteStatus.Pending, cancellationToken);
    }

    public async Task<IReadOnlyList<TeamInvite>> GetPendingInvitesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamInvites
            .Where(i => i.InvitedUserId == userId && i.Status == InviteStatus.Pending)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddInviteAsync(TeamInvite invite, CancellationToken cancellationToken = default)
    {
        await _context.TeamInvites.AddAsync(invite, cancellationToken);
    }

    // Join Requests
    public async Task<TeamJoinRequest?> GetJoinRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TeamJoinRequests
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<TeamJoinRequest?> GetPendingJoinRequestAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamJoinRequests
            .FirstOrDefaultAsync(r => r.TeamId == teamId && r.UserId == userId && r.Status == JoinRequestStatus.Pending, cancellationToken);
    }

    public async Task<IReadOnlyList<TeamJoinRequest>> GetPendingJoinRequestsForTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.TeamJoinRequests
            .Where(r => r.TeamId == teamId && r.Status == JoinRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddJoinRequestAsync(TeamJoinRequest request, CancellationToken cancellationToken = default)
    {
        await _context.TeamJoinRequests.AddAsync(request, cancellationToken);
    }
}
