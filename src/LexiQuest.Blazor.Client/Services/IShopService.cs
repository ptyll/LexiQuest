using LexiQuest.Shared.DTOs.Shop;

namespace LexiQuest.Blazor.Services;

public interface IShopService
{
    Task<IEnumerable<ShopItemDto>> GetShopItemsAsync(string? category = null);
    Task<IEnumerable<UserInventoryItemDto>> GetUserInventoryAsync();
    Task<int> GetUserCoinsAsync();
    Task<PurchaseResultDto> PurchaseItemAsync(Guid itemId);
    Task<EquipResultDto> EquipItemAsync(Guid inventoryItemId);
}

public record PurchaseResultDto(bool Success, int RemainingCoins, string? ErrorMessage);
public record EquipResultDto(bool Success, string? ErrorMessage);
