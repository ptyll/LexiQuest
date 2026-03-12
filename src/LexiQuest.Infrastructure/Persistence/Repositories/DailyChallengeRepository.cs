using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class DailyChallengeRepository : IDailyChallengeRepository
{
    private readonly LexiQuestDbContext _context;

    public DailyChallengeRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<DailyChallenge?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.DailyChallenges
            .FirstOrDefaultAsync(dc => dc.Date == date, cancellationToken);
    }

    public async Task AddAsync(DailyChallenge challenge, CancellationToken cancellationToken = default)
    {
        await _context.DailyChallenges.AddAsync(challenge, cancellationToken);
    }

    public async Task<bool> HasUserCompletedAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.DailyChallengeCompletions
            .AnyAsync(dcc => dcc.UserId == userId && dcc.ChallengeDate == date, cancellationToken);
    }

    public async Task<List<DailyLeaderboardEntry>> GetLeaderboardAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var completions = await _context.DailyChallengeCompletions
            .Where(dcc => dcc.ChallengeDate == date)
            .OrderBy(dcc => dcc.TimeTaken)
            .ToListAsync(cancellationToken);

        // Note: In real implementation, you'd join with Users table to get usernames
        return completions.Select(c => new DailyLeaderboardEntry(
            c.UserId,
            $"User_{c.UserId.ToString()[..8]}",
            c.TimeTaken,
            c.XPEarned
        )).ToList();
    }

    public async Task RecordCompletionAsync(DailyChallengeCompletion completion, CancellationToken cancellationToken = default)
    {
        await _context.DailyChallengeCompletions.AddAsync(completion, cancellationToken);
    }
}
