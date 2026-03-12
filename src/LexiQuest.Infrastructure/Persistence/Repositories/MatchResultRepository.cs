using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Shared.DTOs.Multiplayer;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class MatchResultRepository : IMatchResultRepository
{
    private readonly LexiQuestDbContext _context;

    public MatchResultRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<MatchResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MatchResults
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<MatchResult?> GetByMatchIdAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        return await _context.MatchResults
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.MatchId == matchId, cancellationToken);
    }

    public async Task<IReadOnlyList<MatchResult>> GetByPlayerIdAsync(
        Guid playerId, 
        MatchHistoryFilter filter = MatchHistoryFilter.All, 
        int pageNumber = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.MatchResults
            .AsNoTracking()
            .Where(m => m.Player1Id == playerId || m.Player2Id == playerId);

        // Apply filter
        query = filter switch
        {
            MatchHistoryFilter.QuickMatch => query.Where(m => !m.IsPrivateRoom),
            MatchHistoryFilter.PrivateRoom => query.Where(m => m.IsPrivateRoom),
            _ => query
        };

        return await query
            .OrderByDescending(m => m.CompletedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountByPlayerIdAsync(
        Guid playerId, 
        MatchHistoryFilter filter = MatchHistoryFilter.All, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.MatchResults
            .AsNoTracking()
            .Where(m => m.Player1Id == playerId || m.Player2Id == playerId);

        // Apply filter
        query = filter switch
        {
            MatchHistoryFilter.QuickMatch => query.Where(m => !m.IsPrivateRoom),
            MatchHistoryFilter.PrivateRoom => query.Where(m => m.IsPrivateRoom),
            _ => query
        };

        return await query.CountAsync(cancellationToken);
    }

    public async Task<MultiplayerStats> GetStatsForPlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var matches = await _context.MatchResults
            .AsNoTracking()
            .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
            .ToListAsync(cancellationToken);

        var stats = new MultiplayerStats
        {
            TotalMatchesPlayed = matches.Count,
            TotalXPEarned = matches.Sum(m => m.GetPlayerXPEarned(playerId))
        };

        // Count results
        foreach (var match in matches)
        {
            var result = match.GetResultForPlayer(playerId);
            switch (result)
            {
                case MatchResultType.Win:
                    stats.Wins++;
                    break;
                case MatchResultType.Loss:
                    stats.Losses++;
                    break;
                case MatchResultType.Draw:
                    stats.Draws++;
                    break;
            }

            // Track by type
            if (match.IsPrivateRoom)
            {
                stats.PrivateRoomStats.MatchesPlayed++;
                switch (result)
                {
                    case MatchResultType.Win:
                        stats.PrivateRoomStats.Wins++;
                        break;
                    case MatchResultType.Loss:
                        stats.PrivateRoomStats.Losses++;
                        break;
                    case MatchResultType.Draw:
                        stats.PrivateRoomStats.Draws++;
                        break;
                }
            }
            else
            {
                stats.QuickMatchStats.MatchesPlayed++;
                switch (result)
                {
                    case MatchResultType.Win:
                        stats.QuickMatchStats.Wins++;
                        break;
                    case MatchResultType.Loss:
                        stats.QuickMatchStats.Losses++;
                        break;
                    case MatchResultType.Draw:
                        stats.QuickMatchStats.Draws++;
                        break;
                }
            }
        }

        return stats;
    }

    public async Task AddAsync(MatchResult matchResult, CancellationToken cancellationToken = default)
    {
        await _context.MatchResults.AddAsync(matchResult, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MatchResult matchResult, CancellationToken cancellationToken = default)
    {
        _context.MatchResults.Update(matchResult);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
