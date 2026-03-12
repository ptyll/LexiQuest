using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class InventoryServiceTests
{
    private readonly IShopItemRepository _shopItemRepository;
    private readonly IUserInventoryRepository _inventoryRepository;
    private readonly IPremiumFeatureService _premiumFeatureService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly InventoryService _service;

    public InventoryServiceTests()
    {
        _shopItemRepository = Substitute.For<IShopItemRepository>();
        _inventoryRepository = Substitute.For<IUserInventoryRepository>();
        _premiumFeatureService = Substitute.For<IPremiumFeatureService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _service = new InventoryService(
            _shopItemRepository,
            _inventoryRepository,
            _premiumFeatureService,
            _unitOfWork);
    }

    [Fact]
    public async Task PurchaseItem_SufficientCoins_AddsToInventory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var shopItem = ShopItem.Create("Test Item", "Description", ShopCategory.Avatar, 100, ItemRarity.Common, "img.png");
        
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(false);
        _premiumFeatureService.IsPremiumAsync(userId).Returns(false);

        // Act
        var result = await _service.PurchaseItemAsync(userId, shopItemId);

        // Assert
        result.Success.Should().BeTrue();
        await _inventoryRepository.Received(1).AddAsync(Arg.Any<UserInventoryItem>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task PurchaseItem_AlreadyOwned_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var shopItem = ShopItem.Create("Test Item", "Description", ShopCategory.Avatar, 100, ItemRarity.Common, "img.png");
        
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(true);

        // Act
        var result = await _service.PurchaseItemAsync(userId, shopItemId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("již vlastníte");
        await _inventoryRepository.Received(0).AddAsync(Arg.Any<UserInventoryItem>());
    }

    [Fact]
    public async Task PurchaseItem_PremiumOnly_FreeUser_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var shopItem = ShopItem.CreatePremiumOnly("Premium Item", "Description", ShopCategory.Avatar, 0, ItemRarity.Legendary, "img.png");
        
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(false);
        _premiumFeatureService.IsPremiumAsync(userId).Returns(false);

        // Act
        var result = await _service.PurchaseItemAsync(userId, shopItemId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Premium");
    }

    [Fact]
    public async Task PurchaseItem_PremiumOnly_PremiumUser_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var shopItem = ShopItem.CreatePremiumOnly("Premium Item", "Description", ShopCategory.Avatar, 0, ItemRarity.Legendary, "img.png");
        
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(false);
        _premiumFeatureService.IsPremiumAsync(userId).Returns(true);

        // Act
        var result = await _service.PurchaseItemAsync(userId, shopItemId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PurchaseItem_NotAvailable_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var shopItem = ShopItem.CreateLimited("Limited Item", "Description", ShopCategory.Boost, 500, ItemRarity.Rare, "img.png", DateTime.UtcNow.AddDays(-5));
        
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);

        // Act
        var result = await _service.PurchaseItemAsync(userId, shopItemId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("dostupná");
    }

    [Fact]
    public async Task EquipItem_Success_SetsEquipped()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = UserInventoryItem.Create(userId, Guid.NewGuid());
        
        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(inventoryItem);

        // Act
        var result = await _service.EquipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeTrue();
        inventoryItem.IsEquipped.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task EquipItem_WrongUser_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = UserInventoryItem.Create(otherUserId, Guid.NewGuid());
        
        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(inventoryItem);

        // Act
        var result = await _service.EquipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UnequipItem_Success_SetsUnequipped()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var inventoryItem = UserInventoryItem.Create(userId, Guid.NewGuid());
        inventoryItem.Equip();
        
        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(inventoryItem);

        // Act
        var result = await _service.UnequipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeTrue();
        inventoryItem.IsEquipped.Should().BeFalse();
    }

    [Fact]
    public async Task GetShopItems_ByCategory_ReturnsFiltered()
    {
        // Arrange
        var category = ShopCategory.Avatar;
        var items = new[]
        {
            ShopItem.Create("Avatar 1", "Desc", ShopCategory.Avatar, 100, ItemRarity.Common, "img1.png"),
            ShopItem.Create("Avatar 2", "Desc", ShopCategory.Avatar, 200, ItemRarity.Rare, "img2.png")
        };
        
        _shopItemRepository.GetByCategoryAsync(category).Returns(items);

        // Act
        var result = await _service.GetShopItemsAsync(category);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserInventory_ReturnsUserItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var items = new[]
        {
            UserInventoryItem.Create(userId, Guid.NewGuid()),
            UserInventoryItem.Create(userId, Guid.NewGuid())
        };
        
        _inventoryRepository.GetByUserIdAsync(userId).Returns(items);

        // Act
        var result = await _service.GetUserInventoryAsync(userId);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task HasItem_WhenOwned_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(true);

        // Act
        var result = await _service.HasItemAsync(userId, shopItemId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasItem_WhenNotOwned_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(false);

        // Act
        var result = await _service.HasItemAsync(userId, shopItemId);

        // Assert
        result.Should().BeFalse();
    }
}
