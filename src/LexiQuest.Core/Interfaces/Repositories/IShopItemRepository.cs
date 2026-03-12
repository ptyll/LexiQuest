using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface IShopItemRepository
{
    Task<ShopItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<ShopItem>> GetAllAsync();
    Task<IEnumerable<ShopItem>> GetByCategoryAsync(ShopCategory category);
    Task<IEnumerable<ShopItem>> GetAvailableAsync();
    Task AddAsync(ShopItem item);
    void Update(ShopItem item);
}
