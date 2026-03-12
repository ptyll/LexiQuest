using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;

namespace LexiQuest.Core.Interfaces.Services;

public interface IInventoryService
{
    Task<IEnumerable<ShopItem>> GetShopItemsAsync(ShopCategory? category = null, CancellationToken cancellationToken = default);
    Task<ShopItem?> GetShopItemAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserInventoryItem>> GetUserInventoryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasItemAsync(Guid userId, Guid shopItemId, CancellationToken cancellationToken = default);
    Task<bool> IsPremiumOnlyAsync(Guid shopItemId, CancellationToken cancellationToken = default);
    
    Task<PurchaseResult> PurchaseItemAsync(Guid userId, Guid shopItemId, CancellationToken cancellationToken = default);
    Task<EquipResult> EquipItemAsync(Guid userId, Guid inventoryItemId, CancellationToken cancellationToken = default);
    Task<EquipResult> UnequipItemAsync(Guid userId, Guid inventoryItemId, CancellationToken cancellationToken = default);
    
    Task<int> GetCoinBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddCoinsAsync(Guid userId, int amount, string reason, CancellationToken cancellationToken = default);
    Task<bool> SpendCoinsAsync(Guid userId, int amount, string reason, CancellationToken cancellationToken = default);
}

public record PurchaseResult(bool Success, string Message, Guid? InventoryItemId = null);
public record EquipResult(bool Success, string Message, bool IsEquipped);
