using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class ShopItemRepository : IShopItemRepository
{
    private readonly LexiQuestDbContext _context;

    public ShopItemRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<ShopItem?> GetByIdAsync(Guid id)
    {
        return await _context.ShopItems.FindAsync(id);
    }

    public async Task<IEnumerable<ShopItem>> GetAllAsync()
    {
        return await _context.ShopItems
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Rarity)
            .ThenBy(s => s.Price)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShopItem>> GetByCategoryAsync(ShopCategory category)
    {
        return await _context.ShopItems
            .Where(s => s.Category == category)
            .OrderBy(s => s.Rarity)
            .ThenBy(s => s.Price)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShopItem>> GetAvailableAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.ShopItems
            .Where(s => !s.IsLimited || (s.AvailableUntil.HasValue && s.AvailableUntil.Value > now))
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Rarity)
            .ToListAsync();
    }

    public async Task AddAsync(ShopItem item)
    {
        await _context.ShopItems.AddAsync(item);
    }

    public void Update(ShopItem item)
    {
        _context.ShopItems.Update(item);
    }
}
