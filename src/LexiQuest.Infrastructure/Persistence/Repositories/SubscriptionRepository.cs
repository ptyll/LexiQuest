using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly LexiQuestDbContext _context;

    public SubscriptionRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public Task<Subscription?> GetByIdAsync(Guid id)
    {
        return _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public Task<Subscription?> GetByUserIdAsync(Guid userId)
    {
        return _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
    {
        return _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);
    }

    public Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync()
    {
        var now = DateTime.UtcNow;
        var expired = _context.Subscriptions
            .Where(s => s.ExpiresAt < now && s.Status == SubscriptionStatus.Active)
            .AsEnumerable();
        return Task.FromResult(expired);
    }

    public async Task<IEnumerable<Subscription>> GetExpiredActiveSubscriptionsAsync(DateTime now)
    {
        return await _context.Subscriptions
            .Where(s => s.ExpiresAt < now && s.Status == SubscriptionStatus.Active)
            .ToListAsync();
    }

    public async Task AddAsync(Subscription subscription)
    {
        await _context.Subscriptions.AddAsync(subscription);
    }

    public void Update(Subscription subscription)
    {
        _context.Subscriptions.Update(subscription);
    }
}
