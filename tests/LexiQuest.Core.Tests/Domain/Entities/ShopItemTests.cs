using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class ShopItemTests
{
    [Fact]
    public void ShopItem_Create_SetsProperties()
    {
        // Arrange
        var name = "Golden Frame";
        var description = "A shiny golden frame for your avatar";
        var category = ShopCategory.Frame;
        var price = 500;
        var rarity = ItemRarity.Epic;
        var imageUrl = "https://cdn.lexiquest.com/frames/golden.png";

        // Act
        var item = ShopItem.Create(name, description, category, price, rarity, imageUrl);

        // Assert
        item.Name.Should().Be(name);
        item.Description.Should().Be(description);
        item.Category.Should().Be(category);
        item.Price.Should().Be(price);
        item.Rarity.Should().Be(rarity);
        item.ImageUrl.Should().Be(imageUrl);
        item.IsPremiumOnly.Should().BeFalse();
        item.IsLimited.Should().BeFalse();
        item.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ShopItem_Create_PremiumOnly_SetsIsPremiumOnly()
    {
        // Arrange & Act
        var item = ShopItem.CreatePremiumOnly("Diamond Avatar", "Exclusive diamond avatar", ShopCategory.Avatar, 0, ItemRarity.Legendary, "diamond.png");

        // Assert
        item.IsPremiumOnly.Should().BeTrue();
        item.Price.Should().Be(0); // Premium items can be free
    }

    [Fact]
    public void ShopItem_Create_Limited_SetsAvailableUntil()
    {
        // Arrange
        var availableUntil = DateTime.UtcNow.AddDays(7);

        // Act
        var item = ShopItem.CreateLimited("Holiday Theme", "Limited holiday theme", ShopCategory.Theme, 1000, ItemRarity.Legendary, "holiday.png", availableUntil);

        // Assert
        item.IsLimited.Should().BeTrue();
        item.AvailableUntil.Should().Be(availableUntil);
    }

    [Fact]
    public void ShopItem_IsAvailable_NotLimited_ReturnsTrue()
    {
        // Arrange
        var item = ShopItem.Create("Common Frame", "Basic frame", ShopCategory.Frame, 100, ItemRarity.Common, "common.png");

        // Act
        var result = item.IsAvailable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShopItem_IsAvailable_LimitedNotExpired_ReturnsTrue()
    {
        // Arrange
        var availableUntil = DateTime.UtcNow.AddDays(5);
        var item = ShopItem.CreateLimited("Limited Item", "Limited", ShopCategory.Boost, 500, ItemRarity.Rare, "limited.png", availableUntil);

        // Act
        var result = item.IsAvailable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShopItem_IsAvailable_LimitedExpired_ReturnsFalse()
    {
        // Arrange
        var availableUntil = DateTime.UtcNow.AddDays(-5);
        var item = ShopItem.CreateLimited("Expired Item", "Expired", ShopCategory.Boost, 500, ItemRarity.Rare, "expired.png", availableUntil);

        // Act
        var result = item.IsAvailable();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(ItemRarity.Common, "#9E9E9E")]
    [InlineData(ItemRarity.Rare, "#2196F3")]
    [InlineData(ItemRarity.Epic, "#9C27B0")]
    [InlineData(ItemRarity.Legendary, "#FFD700")]
    public void ShopItem_GetRarityColor_ReturnsCorrectColor(ItemRarity rarity, string expectedColor)
    {
        // Arrange
        var item = ShopItem.Create("Item", "Desc", ShopCategory.Avatar, 100, rarity, "img.png");

        // Act
        var color = item.GetRarityColor();

        // Assert
        color.Should().Be(expectedColor);
    }
}
