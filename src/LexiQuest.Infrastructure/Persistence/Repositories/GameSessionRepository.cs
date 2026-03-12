using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class GameSessionRepository : IGameSessionRepository
{
    private readonly LexiQuestDbContext _context;

    public GameSessionRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GameSession>> GetByUserIdWithRoundsAsync(
        Guid userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _context.GameSessions
            .Include(s => s.Rounds)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
