namespace LexiQuest.Shared.DTOs.Shop;

public record ShopItemDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    int Price,
    string Rarity,
    string RarityColor,
    string ImageUrl,
    bool IsPremiumOnly,
    bool IsLimited,
    bool IsAvailable,
    DateTime? AvailableUntil);

public record ShopItemDetailDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    int Price,
    string Rarity,
    string RarityColor,
    string ImageUrl,
    bool IsPremiumOnly,
    bool IsLimited,
    bool IsAvailable,
    DateTime? AvailableUntil,
    bool IsOwned);

public record InventoryItemDto(
    Guid Id,
    Guid ShopItemId,
    string Name,
    string Description,
    string Category,
    string Rarity,
    string RarityColor,
    string ImageUrl,
    bool IsEquipped,
    DateTime PurchasedAt);

public record PurchaseRequest(Guid ShopItemId);

public record PurchaseResult(
    bool Success,
    string Message,
    Guid? InventoryItemId,
    int RemainingCoins);

public record EquipItemRequest(Guid InventoryItemId);

public record EquipItemResult(
    bool Success,
    string Message,
    bool IsEquipped);

public record CoinBalanceDto(int Balance);

public record ShopCategoryDto(
    string Id,
    string Name,
    string Icon,
    int ItemCount);
