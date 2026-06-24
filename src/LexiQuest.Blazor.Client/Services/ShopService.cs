using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Shop;

namespace LexiQuest.Blazor.Services;

public class ShopService : IShopService
{
    private readonly IAuthenticatedApiClient _apiClient;

    public ShopService(IAuthenticatedApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<ShopItemDto>> GetShopItemsAsync(string? category = null)
    {
        var url = string.IsNullOrWhiteSpace(category)
            ? "api/v1/shop/items"
            : $"api/v1/shop/items?category={Uri.EscapeDataString(category)}";

        return await _apiClient.GetFromJsonAsync<List<ShopItemDto>>(url) ?? [];
    }

    public async Task<IEnumerable<UserInventoryItemDto>> GetUserInventoryAsync()
    {
        var inventory = await _apiClient.GetFromJsonAsync<List<InventoryItemDto>>("api/v1/shop/inventory") ?? [];
        return inventory.Select(item => new UserInventoryItemDto(
            item.Id,
            Guid.Empty,
            item.ShopItemId,
            item.Name,
            item.Category,
            item.ImageUrl,
            item.IsEquipped,
            item.PurchasedAt));
    }

    public async Task<int> GetUserCoinsAsync()
    {
        var balance = await _apiClient.GetFromJsonAsync<CoinBalanceDto>("api/v1/shop/coins");
        return balance?.Balance ?? 0;
    }

    public async Task<PurchaseResultDto> PurchaseItemAsync(Guid itemId)
    {
        using var response = await _apiClient.PostAsJsonAsync("api/v1/shop/purchase", new PurchaseRequest(itemId));
        var result = await response.Content.ReadFromJsonAsync<LexiQuest.Shared.DTOs.Shop.PurchaseResult>();

        if (response.IsSuccessStatusCode && result != null)
            return new PurchaseResultDto(result.Success, result.RemainingCoins, null);

        return new PurchaseResultDto(false, result?.RemainingCoins ?? 0, result?.Message);
    }

    public async Task<EquipResultDto> EquipItemAsync(Guid inventoryItemId)
    {
        using var response = await _apiClient.PostAsJsonAsync("api/v1/shop/equip", new EquipItemRequest(inventoryItemId));
        var result = await response.Content.ReadFromJsonAsync<EquipItemResult>();

        if (response.IsSuccessStatusCode && result != null)
            return new EquipResultDto(result.Success, null);

        return new EquipResultDto(false, result?.Message);
    }
}
