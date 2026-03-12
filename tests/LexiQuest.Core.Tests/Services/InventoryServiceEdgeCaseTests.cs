using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class InventoryServiceEdgeCaseTests
{
    private readonly IShopItemRepository _shopItemRepository = Substitute.For<IShopItemRepository>();
    private readonly IUserInventoryRepository _inventoryRepository = Substitute.For<IUserInventoryRepository>();
    private readonly IPremiumFeatureService _premiumFeatureService = Substitute.For<IPremiumFeatureService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly InventoryService _sut;

    public InventoryServiceEdgeCaseTests()
    {
        _sut = new InventoryService(_shopItemRepository, _inventoryRepository, _premiumFeatureService, _unitOfWork);
    }

    // --- Purchasing already owned item ---

    [Fact]
    public async Task PurchaseItem_AlreadyOwned_ReturnsFailure_DoesNotAddToInventory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var shopItem = ShopItem.Create("Avatar", "Desc", ShopCategory.Avatar, 100, ItemRarity.Common, "img.png");
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(true);

        // Act
        var result = await _sut.PurchaseItemAsync(userId, shopItemId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("již vlastníte");
        await _inventoryRepository.DidNotReceive().AddAsync(Arg.Any<UserInventoryItem>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- Purchasing premium item as non-premium user ---

    [Fact]
    public async Task PurchaseItem_PremiumOnly_NonPremiumUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var shopItem = ShopItem.CreatePremiumOnly("Premium Avatar", "Desc", ShopCategory.Avatar, 0, ItemRarity.Legendary, "img.png");
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(false);
        _premiumFeatureService.IsPremiumAsync(userId).Returns(false);

        // Act
        var result = await _sut.PurchaseItemAsync(userId, shopItemId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Premium");
        await _inventoryRepository.DidNotReceive().AddAsync(Arg.Any<UserInventoryItem>());
    }

    [Fact]
    public async Task PurchaseItem_PremiumOnly_PremiumUser_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var shopItem = ShopItem.CreatePremiumOnly("Premium Avatar", "Desc", ShopCategory.Avatar, 0, ItemRarity.Legendary, "img.png");
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(false);
        _premiumFeatureService.IsPremiumAsync(userId).Returns(true);

        // Act
        var result = await _sut.PurchaseItemAsync(userId, shopItemId);

        // Assert
        result.Success.Should().BeTrue();
        result.InventoryItemId.Should().NotBeNull();
    }

    // --- Purchasing item that doesn't exist ---

    [Fact]
    public async Task PurchaseItem_ItemNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        _shopItemRepository.GetByIdAsync(shopItemId).Returns((ShopItem?)null);

        // Act
        var result = await _sut.PurchaseItemAsync(userId, shopItemId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("nebyla nalezena");
    }

    // --- Purchasing unavailable item ---

    [Fact]
    public async Task PurchaseItem_ExpiredLimitedItem_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var shopItem = ShopItem.CreateLimited("Limited", "Desc", ShopCategory.Boost, 500, ItemRarity.Rare, "img.png", DateTime.UtcNow.AddDays(-1));
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);

        // Act
        var result = await _sut.PurchaseItemAsync(userId, shopItemId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("dostupná");
    }

    // --- Equipping / Unequipping ---

    [Fact]
    public async Task EquipItem_Success_SetsIsEquippedTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var item = UserInventoryItem.Create(userId, Guid.NewGuid());
        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(item);

        // Act
        var result = await _sut.EquipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeTrue();
        result.IsEquipped.Should().BeTrue();
        item.IsEquipped.Should().BeTrue();
        _inventoryRepository.Received(1).Update(item);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnequipItem_Success_SetsIsEquippedFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var item = UserInventoryItem.Create(userId, Guid.NewGuid());
        item.Equip();
        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(item);

        // Act
        var result = await _sut.UnequipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeTrue();
        result.IsEquipped.Should().BeFalse();
        item.IsEquipped.Should().BeFalse();
    }

    [Fact]
    public async Task EquipItem_ItemNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns((UserInventoryItem?)null);

        // Act
        var result = await _sut.EquipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("nebyla nalezena");
    }

    [Fact]
    public async Task UnequipItem_ItemNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns((UserInventoryItem?)null);

        // Act
        var result = await _sut.UnequipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task EquipItem_WrongUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var item = UserInventoryItem.Create(otherUserId, Guid.NewGuid());
        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(item);

        // Act
        var result = await _sut.EquipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("oprávnění");
    }

    [Fact]
    public async Task UnequipItem_WrongUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var item = UserInventoryItem.Create(otherUserId, Guid.NewGuid());
        item.Equip();
        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(item);

        // Act
        var result = await _sut.UnequipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("oprávnění");
    }

    // --- IsPremiumOnly check ---

    [Fact]
    public async Task IsPremiumOnly_PremiumItem_ReturnsTrue()
    {
        // Arrange
        var shopItemId = Guid.NewGuid();
        var item = ShopItem.CreatePremiumOnly("Premium", "Desc", ShopCategory.Avatar, 0, ItemRarity.Legendary, "img.png");
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(item);

        // Act
        var result = await _sut.IsPremiumOnlyAsync(shopItemId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPremiumOnly_RegularItem_ReturnsFalse()
    {
        // Arrange
        var shopItemId = Guid.NewGuid();
        var item = ShopItem.Create("Regular", "Desc", ShopCategory.Avatar, 100, ItemRarity.Common, "img.png");
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(item);

        // Act
        var result = await _sut.IsPremiumOnlyAsync(shopItemId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsPremiumOnly_ItemNotFound_ReturnsFalse()
    {
        // Arrange
        var shopItemId = Guid.NewGuid();
        _shopItemRepository.GetByIdAsync(shopItemId).Returns((ShopItem?)null);

        // Act
        var result = await _sut.IsPremiumOnlyAsync(shopItemId);

        // Assert
        result.Should().BeFalse();
    }

    // --- GetShopItems ---

    [Fact]
    public async Task GetShopItems_NoCategoryFilter_ReturnsAll()
    {
        // Arrange
        var items = new[]
        {
            ShopItem.Create("Item1", "Desc", ShopCategory.Avatar, 100, ItemRarity.Common, "img.png"),
            ShopItem.Create("Item2", "Desc", ShopCategory.Boost, 200, ItemRarity.Rare, "img.png")
        };
        _shopItemRepository.GetAllAsync().Returns(items);

        // Act
        var result = await _sut.GetShopItemsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetShopItems_WithCategoryFilter_ReturnsFiltered()
    {
        // Arrange
        var items = new[]
        {
            ShopItem.Create("Avatar1", "Desc", ShopCategory.Avatar, 100, ItemRarity.Common, "img.png")
        };
        _shopItemRepository.GetByCategoryAsync(ShopCategory.Avatar).Returns(items);

        // Act
        var result = await _sut.GetShopItemsAsync(ShopCategory.Avatar);

        // Assert
        result.Should().HaveCount(1);
        await _shopItemRepository.DidNotReceive().GetAllAsync();
    }
}
