using LexiQuest.Api.Extensions;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/shop")]
[Authorize]
public class ShopController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IPremiumFeatureService _premiumFeatureService;

    public ShopController(
        IInventoryService inventoryService,
        IPremiumFeatureService premiumFeatureService)
    {
        _inventoryService = inventoryService;
        _premiumFeatureService = premiumFeatureService;
    }

    [HttpGet("items")]
    public async Task<ActionResult<IEnumerable<ShopItemDto>>> GetItems(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _inventoryService.GetShopItemsAsync(
            category != null ? Enum.Parse<ShopCategory>(category, true) : (ShopCategory?)null,
            cancellationToken);

        var dtos = items.Select(i => new ShopItemDto(
            i.Id,
            i.Name,
            i.Description,
            i.Category.ToString(),
            i.Price,
            i.Rarity.ToString(),
            i.GetRarityColor(),
            i.ImageUrl,
            i.IsPremiumOnly,
            i.IsLimited,
            i.IsAvailable(),
            i.AvailableUntil));

        return Ok(dtos);
    }

    [HttpGet("items/{id:guid}")]
    public async Task<ActionResult<ShopItemDetailDto>> GetItem(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var item = await _inventoryService.GetShopItemAsync(id, cancellationToken);

        if (item == null)
            return NotFound();

        var isOwned = await _inventoryService.HasItemAsync(userId, id, cancellationToken);

        return Ok(new ShopItemDetailDto(
            item.Id,
            item.Name,
            item.Description,
            item.Category.ToString(),
            item.Price,
            item.Rarity.ToString(),
            item.GetRarityColor(),
            item.ImageUrl,
            item.IsPremiumOnly,
            item.IsLimited,
            item.IsAvailable(),
            item.AvailableUntil,
            isOwned));
    }

    [HttpGet("categories")]
    public ActionResult<IEnumerable<ShopCategoryDto>> GetCategories()
    {
        var categories = new[]
        {
            new ShopCategoryDto("Avatar", "Avatary", "User", 12),
            new ShopCategoryDto("Frame", "Rámečky", "Square", 8),
            new ShopCategoryDto("Theme", "Témata", "Palette", 6),
            new ShopCategoryDto("Boost", "Boosty", "Zap", 4)
        };

        return Ok(categories);
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<Shared.DTOs.Shop.PurchaseResult>> Purchase(
        [FromBody] PurchaseRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _inventoryService.PurchaseItemAsync(userId, request.ShopItemId, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new Shared.DTOs.Shop.PurchaseResult(
                false,
                result.Message,
                null,
                0));
        }

        var coins = await _inventoryService.GetCoinBalanceAsync(userId, cancellationToken);

        return Ok(new Shared.DTOs.Shop.PurchaseResult(
            true,
            result.Message,
            result.InventoryItemId,
            coins));
    }

    [HttpPost("equip")]
    public async Task<ActionResult<EquipItemResult>> Equip(
        [FromBody] EquipItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _inventoryService.EquipItemAsync(userId, request.InventoryItemId, cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("unequip")]
    public async Task<ActionResult<EquipItemResult>> Unequip(
        [FromBody] EquipItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _inventoryService.UnequipItemAsync(userId, request.InventoryItemId, cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetInventory(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var items = await _inventoryService.GetUserInventoryAsync(userId, cancellationToken);

        // TODO: Load shop item details for each inventory item
        var dtos = items.Select(i => new InventoryItemDto(
            i.Id,
            i.ShopItemId,
            "Item Name", // TODO: Load from ShopItem
            "Description", // TODO: Load from ShopItem
            "Category", // TODO: Load from ShopItem
            "Rarity", // TODO: Load from ShopItem
            "#000000", // TODO: Load from ShopItem
            "image.png", // TODO: Load from ShopItem
            i.IsEquipped,
            i.PurchasedAt));

        return Ok(dtos);
    }

    [HttpGet("coins")]
    public async Task<ActionResult<CoinBalanceDto>> GetCoinBalance(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var balance = await _inventoryService.GetCoinBalanceAsync(userId, cancellationToken);
        return Ok(new CoinBalanceDto(balance));
    }

}
