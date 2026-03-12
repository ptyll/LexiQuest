using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class LeagueRepository : ILeagueRepository
{
    private readonly LexiQuestDbContext _context;

    public LeagueRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<League?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Leagues
            .Include(l => l.Participants)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<League?> GetActiveLeagueForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Leagues
            .Include(l => l.Participants)
            .Where(l => l.IsActive)
            .Where(l => l.Participants.Any(p => p.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<League?> GetActiveLeagueForTierAsync(LeagueTier tier, CancellationToken cancellationToken = default)
    {
        return await _context.Leagues
            .Include(l => l.Participants)
            .Where(l => l.Tier == tier && l.IsActive)
            .OrderBy(l => l.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<League>> GetActiveLeaguesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Leagues
            .Include(l => l.Participants)
            .Where(l => l.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<League>> GetLeaguesByWeekAsync(DateTime weekStart, CancellationToken cancellationToken = default)
    {
        return await _context.Leagues
            .Include(l => l.Participants)
            .Where(l => l.WeekStart == weekStart)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(League league, CancellationToken cancellationToken = default)
    {
        await _context.Leagues.AddAsync(league, cancellationToken);
    }

    public void Update(League league)
    {
        _context.Leagues.Update(league);
    }
}
