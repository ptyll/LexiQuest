using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.Enums;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class InventoryServiceTests
{
    private readonly IShopItemRepository _shopItemRepository;
    private readonly IUserInventoryRepository _inventoryRepository;
    private readonly IPremiumFeatureService _premiumFeatureService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly InventoryService _service;

    public InventoryServiceTests()
    {
        _shopItemRepository = Substitute.For<IShopItemRepository>();
        _inventoryRepository = Substitute.For<IUserInventoryRepository>();
        _premiumFeatureService = Substitute.For<IPremiumFeatureService>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _service = new InventoryService(
            _shopItemRepository,
            _inventoryRepository,
            _premiumFeatureService,
            _userRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task PurchaseItem_SufficientCoins_AddsToInventory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var user = User.Create("shop@test.local", "shopuser");
        user.SetId(userId);
        user.AddCoins(500);
        var shopItem = ShopItem.Create("Test Item", "Description", ShopCategory.Avatar, 100, ItemRarity.Common, "img.png");
        
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(false);
        _premiumFeatureService.IsPremiumAsync(userId).Returns(false);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

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
        var user = User.Create("premium@test.local", "premiumuser");
        user.SetId(userId);
        var shopItem = ShopItem.CreatePremiumOnly("Premium Item", "Description", ShopCategory.Avatar, 0, ItemRarity.Legendary, "img.png");
        
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _inventoryRepository.HasItemAsync(userId, shopItemId).Returns(false);
        _premiumFeatureService.IsPremiumAsync(userId).Returns(true);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

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
    public async Task EquipItem_AvatarItem_UpdatesUserAvatarUrl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var user = User.Create("avatar@test.local", "avataruser");
        user.SetId(userId);
        var inventoryItem = UserInventoryItem.Create(userId, shopItemId);
        var shopItem = ShopItem.Create("Vlastní avatar", "Desc", ShopCategory.Avatar, 100, ItemRarity.Rare, "/assets/shop/custom-avatar.svg");

        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(inventoryItem);
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _service.EquipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeTrue();
        user.AvatarUrl.Should().Be("/assets/shop/custom-avatar.svg");
        _userRepository.Received(1).Update(user);
    }

    [Fact]
    public async Task EquipItem_DiamondAvatarWithLegacyIcon_UsesDiamondAvatarAsset()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var user = User.Create("diamond@test.local", "diamonduser");
        user.SetId(userId);
        var inventoryItem = UserInventoryItem.Create(userId, shopItemId);
        var shopItem = ShopItem.CreatePremiumOnly("Diamantový avatar", "Desc", ShopCategory.Avatar, 0, ItemRarity.Legendary, "/icon-192.png");

        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(inventoryItem);
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _service.EquipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeTrue();
        user.AvatarUrl.Should().Be("/assets/shop/avatar-diamond.svg");
        _userRepository.Received(1).Update(user);
    }

    [Fact]
    public async Task EquipItem_ThemeItem_UpdatesUserTheme()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var user = User.Create("theme@test.local", "themeuser");
        user.SetId(userId);
        var inventoryItem = UserInventoryItem.Create(userId, shopItemId);
        var shopItem = ShopItem.Create("Noční téma", "Desc", ShopCategory.Theme, 900, ItemRarity.Epic, "/icon-192.png");

        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(inventoryItem);
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _service.EquipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeTrue();
        user.Preferences.Theme.Should().Be(AppTheme.Dark);
        _userRepository.Received(1).Update(user);
    }

    [Fact]
    public async Task EquipItem_FrameItem_KeepsAvatarUnchanged()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var user = User.Create("frame@test.local", "frameuser");
        user.SetId(userId);
        user.UpdateAvatar("/assets/shop/avatar-diamond.svg");
        var inventoryItem = UserInventoryItem.Create(userId, shopItemId);
        var shopItem = ShopItem.Create("Stříbrný rámeček", "Desc", ShopCategory.Frame, 250, ItemRarity.Rare, "/icon-192.png");

        _inventoryRepository.GetByIdAsync(inventoryItemId).Returns(inventoryItem);
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _service.EquipItemAsync(userId, inventoryItemId);

        // Assert
        result.Success.Should().BeTrue();
        user.AvatarUrl.Should().Be("/assets/shop/avatar-diamond.svg");
        _userRepository.DidNotReceive().Update(user);
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
    public async Task GetUserInventory_EquippedDiamondAvatarWithMissingProfileAvatar_RepairsAvatarUrl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var user = User.Create("repair-avatar@test.local", "repairavatar");
        user.SetId(userId);
        var item = UserInventoryItem.Create(userId, shopItemId);
        item.Equip();
        var shopItem = ShopItem.CreatePremiumOnly("Diamantový avatar", "Desc", ShopCategory.Avatar, 0, ItemRarity.Legendary, "/icon-192.png");

        _inventoryRepository.GetByUserIdAsync(userId).Returns(new[] { item });
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _service.GetUserInventoryAsync(userId);

        // Assert
        result.Should().ContainSingle();
        user.AvatarUrl.Should().Be("/assets/shop/avatar-diamond.svg");
        _userRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserInventory_EquippedNightThemeWithLightPreference_RepairsTheme()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var user = User.Create("repair-theme@test.local", "repairtheme");
        user.SetId(userId);
        var item = UserInventoryItem.Create(userId, shopItemId);
        item.Equip();
        var shopItem = ShopItem.Create("Noční téma", "Desc", ShopCategory.Theme, 900, ItemRarity.Epic, "/icon-192.png");

        _inventoryRepository.GetByUserIdAsync(userId).Returns(new[] { item });
        _shopItemRepository.GetByIdAsync(shopItemId).Returns(shopItem);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _service.GetUserInventoryAsync(userId);

        // Assert
        result.Should().ContainSingle();
        user.Preferences.Theme.Should().Be(AppTheme.Dark);
        _userRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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
