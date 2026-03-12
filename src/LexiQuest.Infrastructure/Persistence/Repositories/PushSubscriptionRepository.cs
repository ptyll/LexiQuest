using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class PushSubscriptionRepository : IPushSubscriptionRepository
{
    private readonly LexiQuestDbContext _context;

    public PushSubscriptionRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<List<PushSubscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PushSubscriptions
            .Where(ps => ps.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        return await _context.PushSubscriptions
            .FirstOrDefaultAsync(ps => ps.Endpoint == endpoint, cancellationToken);
    }

    public async Task AddAsync(PushSubscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.PushSubscriptions.AddAsync(subscription, cancellationToken);
    }

    public void Remove(PushSubscription subscription)
    {
        _context.PushSubscriptions.Remove(subscription);
    }

    public void Update(PushSubscription subscription)
    {
        _context.PushSubscriptions.Update(subscription);
    }
}
