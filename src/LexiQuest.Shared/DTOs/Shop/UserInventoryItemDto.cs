namespace LexiQuest.Shared.DTOs.Shop;

public record UserInventoryItemDto(
    Guid Id,
    Guid UserId,
    Guid ShopItemId,
    string ShopItemName,
    string ShopItemCategory,
    string ShopItemImageUrl,
    bool IsEquipped,
    DateTime PurchasedAt);
