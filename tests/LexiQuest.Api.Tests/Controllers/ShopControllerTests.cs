using System.Security.Claims;
using FluentAssertions;
using LexiQuest.Api.Controllers;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Shop;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace LexiQuest.Api.Tests.Controllers;

public class ShopControllerTests
{
    private readonly IInventoryService _inventoryService;
    private readonly IPremiumFeatureService _premiumFeatureService;
    private readonly ShopController _controller;
    private readonly Guid _testUserId;

    public ShopControllerTests()
    {
        _inventoryService = Substitute.For<IInventoryService>();
        _premiumFeatureService = Substitute.For<IPremiumFeatureService>();
        _controller = new ShopController(_inventoryService, _premiumFeatureService);
        _testUserId = Guid.NewGuid();

        // Setup authenticated user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetItems_ReturnsAllItems()
    {
        // Arrange
        var items = new List<ShopItem>
        {
            ShopItem.Create("Avatar 1", "Description", ShopCategory.Avatar, 100, ItemRarity.Common, "url1.jpg"),
            ShopItem.Create("Avatar 2", "Description", ShopCategory.Avatar, 200, ItemRarity.Rare, "url2.jpg")
        };
        _inventoryService.GetShopItemsAsync(Arg.Any<ShopCategory?>(), Arg.Any<CancellationToken>())
            .Returns(items);

        // Act
        var result = await _controller.GetItems(cancellationToken: CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var dtos = okResult!.Value as IEnumerable<ShopItemDto>;
        dtos!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetItems_WithCategory_ReturnsFilteredItems()
    {
        // Arrange
        var items = new List<ShopItem>
        {
            ShopItem.Create("Avatar 1", "Description", ShopCategory.Avatar, 100, ItemRarity.Common, "url1.jpg")
        };
        _inventoryService.GetShopItemsAsync(ShopCategory.Avatar, Arg.Any<CancellationToken>())
            .Returns(items);

        // Act
        var result = await _controller.GetItems("avatar", CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var dtos = okResult!.Value as IEnumerable<ShopItemDto>;
        dtos!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetItem_ExistingItem_ReturnsItemDetail()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = ShopItem.Create("Avatar 1", "Description", ShopCategory.Avatar, 100, ItemRarity.Common, "url1.jpg");
        typeof(ShopItem).GetProperty("Id")?.SetValue(item, itemId);

        _inventoryService.GetShopItemAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(item);
        _inventoryService.HasItemAsync(_testUserId, itemId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _controller.GetItem(itemId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var detail = okResult!.Value as ShopItemDetailDto;
        detail!.Name.Should().Be("Avatar 1");
        detail.IsOwned.Should().BeFalse();
    }

    [Fact]
    public async Task GetItem_NotFound_ReturnsNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _inventoryService.GetShopItemAsync(itemId, Arg.Any<CancellationToken>())
            .Returns((ShopItem?)null);

        // Act
        var result = await _controller.GetItem(itemId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void GetCategories_ReturnsAllCategories()
    {
        // Act
        var result = _controller.GetCategories();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var categories = okResult!.Value as IEnumerable<ShopCategoryDto>;
        categories!.Should().HaveCount(4);
        categories.Select(c => c.Id).Should().Contain(new[] { "Avatar", "Frame", "Theme", "Boost" });
    }

    [Fact]
    public async Task Purchase_Success_ReturnsOk()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var request = new PurchaseRequest(itemId);

        _inventoryService.PurchaseItemAsync(_testUserId, itemId, Arg.Any<CancellationToken>())
            .Returns(new Core.Interfaces.Services.PurchaseResult(Success: true, Message: "Purchased", InventoryItemId: inventoryItemId));
        _inventoryService.GetCoinBalanceAsync(_testUserId, Arg.Any<CancellationToken>())
            .Returns(500);

        // Act
        var result = await _controller.Purchase(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as Shared.DTOs.Shop.PurchaseResult;
        response!.Success.Should().BeTrue();
        response.RemainingCoins.Should().Be(500);
    }

    [Fact]
    public async Task Purchase_Failure_ReturnsBadRequest()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var request = new PurchaseRequest(itemId);

        _inventoryService.PurchaseItemAsync(_testUserId, itemId, Arg.Any<CancellationToken>())
            .Returns(new Core.Interfaces.Services.PurchaseResult(Success: false, Message: "Insufficient funds"));

        // Act
        var result = await _controller.Purchase(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result.Result as BadRequestObjectResult;
        var response = badRequest!.Value as Shared.DTOs.Shop.PurchaseResult;
        response!.Success.Should().BeFalse();
    }
}
