using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class UserInventoryRepository : IUserInventoryRepository
{
    private readonly LexiQuestDbContext _context;

    public UserInventoryRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<UserInventoryItem?> GetByIdAsync(Guid id)
    {
        return await _context.UserInventoryItems.FindAsync(id);
    }

    public async Task<UserInventoryItem?> GetByUserAndItemAsync(Guid userId, Guid shopItemId)
    {
        return await _context.UserInventoryItems
            .FirstOrDefaultAsync(i => i.UserId == userId && i.ShopItemId == shopItemId);
    }

    public async Task<IEnumerable<UserInventoryItem>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserInventoryItems
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.IsEquipped)
            .ThenByDescending(i => i.PurchasedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserInventoryItem>> GetEquippedByUserIdAsync(Guid userId)
    {
        return await _context.UserInventoryItems
            .Where(i => i.UserId == userId && i.IsEquipped)
            .ToListAsync();
    }

    public async Task<bool> HasItemAsync(Guid userId, Guid shopItemId)
    {
        return await _context.UserInventoryItems
            .AnyAsync(i => i.UserId == userId && i.ShopItemId == shopItemId);
    }

    public async Task AddAsync(UserInventoryItem item)
    {
        await _context.UserInventoryItems.AddAsync(item);
    }

    public void Update(UserInventoryItem item)
    {
        _context.UserInventoryItems.Update(item);
    }
}
