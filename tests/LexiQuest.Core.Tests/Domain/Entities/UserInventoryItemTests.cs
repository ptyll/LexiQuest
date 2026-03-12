using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class UserInventoryItemTests
{
    [Fact]
    public void UserInventoryItem_Create_SetsProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();

        // Act
        var item = UserInventoryItem.Create(userId, shopItemId);

        // Assert
        item.UserId.Should().Be(userId);
        item.ShopItemId.Should().Be(shopItemId);
        item.IsEquipped.Should().BeFalse();
        item.PurchasedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        item.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void UserInventoryItem_Equip_SetsIsEquippedTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var item = UserInventoryItem.Create(userId, shopItemId);

        // Act
        item.Equip();

        // Assert
        item.IsEquipped.Should().BeTrue();
    }

    [Fact]
    public void UserInventoryItem_Unequip_SetsIsEquippedFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var item = UserInventoryItem.Create(userId, shopItemId);
        item.Equip();

        // Act
        item.Unequip();

        // Assert
        item.IsEquipped.Should().BeFalse();
    }

    [Fact]
    public void UserInventoryItem_ToggleEquipped_SwitchesState()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopItemId = Guid.NewGuid();
        var item = UserInventoryItem.Create(userId, shopItemId);

        // Act & Assert
        item.IsEquipped.Should().BeFalse();
        
        item.ToggleEquipped();
        item.IsEquipped.Should().BeTrue();
        
        item.ToggleEquipped();
        item.IsEquipped.Should().BeFalse();
    }
}
