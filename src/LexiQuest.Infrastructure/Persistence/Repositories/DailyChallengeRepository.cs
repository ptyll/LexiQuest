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

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DailyChallenges.CountAsync(cancellationToken);
    }

    public async Task<bool> HasUserCompletedAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.DailyChallengeCompletions
            .AnyAsync(dcc => dcc.UserId == userId && dcc.ChallengeDate == date, cancellationToken);
    }

    public async Task<List<DailyLeaderboardEntry>> GetLeaderboardAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.DailyChallengeCompletions
            .Where(dcc => dcc.ChallengeDate == date)
            .OrderBy(dcc => dcc.TimeTaken)
            .Join(
                _context.Users,
                completion => completion.UserId,
                user => user.Id,
                (completion, user) => new DailyLeaderboardEntry(
                    completion.UserId,
                    user.Username,
                    completion.TimeTaken,
                    completion.XPEarned))
            .ToListAsync(cancellationToken);
    }

    public async Task RecordCompletionAsync(DailyChallengeCompletion completion, CancellationToken cancellationToken = default)
    {
        await _context.DailyChallengeCompletions.AddAsync(completion, cancellationToken);
    }
}
