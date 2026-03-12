using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Shop;
using LexiQuest.Shared.DTOs.Shop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Tempo.Blazor.Components.DataDisplay;
using Tempo.Blazor.Components.Feedback;
using Tempo.Blazor.Components.Buttons;

namespace LexiQuest.Blazor.Tests.Components.Shop;

public class ShopItemCardTests : BunitContext
{
    private readonly IStringLocalizer<ShopItemCard> _localizer;
    private readonly ITmLocalizer _tmLocalizer;

    public ShopItemCardTests()
    {
        _localizer = Substitute.For<IStringLocalizer<ShopItemCard>>();
        _localizer["Item_Owned"].Returns(new LocalizedString("Item_Owned", "Vlastněno"));
        _localizer["Item_PremiumOnly"].Returns(new LocalizedString("Item_PremiumOnly", "Premium"));
        _localizer["Button_Equip"].Returns(new LocalizedString("Button_Equip", "Nasadit"));
        _localizer["Button_Buy"].Returns(new LocalizedString("Button_Buy", "Koupit"));
        _localizer["Button_Equipped"].Returns(new LocalizedString("Button_Equipped", "Nasazeno"));

        _tmLocalizer = Substitute.For<ITmLocalizer>();
        _tmLocalizer["TmCard.Title"].Returns(new LocalizedString("TmCard.Title", ""));
        _tmLocalizer["TmCard.Description"].Returns(new LocalizedString("TmCard.Description", ""));
        _tmLocalizer["TmBadge.Text"].Returns(new LocalizedString("TmBadge.Text", ""));
        _tmLocalizer["TmButton.Text"].Returns(new LocalizedString("TmButton.Text", ""));

        Services.AddSingleton(_localizer);
        Services.AddSingleton(_tmLocalizer);
        Services.AddSingleton(_localizer);
    }

    [Fact]
    public void ShopItemCard_Renders_NameAndPrice()
    {
        // Arrange
        var item = new ShopItemDto(
            Guid.NewGuid(),
            "Golden Frame",
            "A shiny frame",
            "Frame",
            500,
            "Epic",
            "#9C27B0",
            "frame.png",
            false,
            false,
            true,
            null);

        // Act
        var cut = Render<ShopItemCard>(parameters =>
            parameters.Add(p => p.Item, item)
                      .Add(p => p.IsOwned, false));

        // Assert
        cut.Markup.Should().Contain("Golden Frame");
        cut.Markup.Should().Contain("500");
    }

    [Fact]
    public void ShopItemCard_Owned_ShowsEquippedBadge()
    {
        // Arrange
        var item = new ShopItemDto(
            Guid.NewGuid(),
            "Test Item",
            "Description",
            "Avatar",
            100,
            "Common",
            "#9E9E9E",
            "avatar.png",
            false,
            false,
            true,
            null);

        // Act
        var cut = Render<ShopItemCard>(parameters =>
            parameters.Add(p => p.Item, item)
                      .Add(p => p.IsOwned, true)
                      .Add(p => p.IsEquipped, true));

        // Assert
        cut.Markup.Should().Contain("Vlastněno");
        cut.Markup.Should().Contain("Nasazeno");
    }

    [Fact]
    public void ShopItemCard_PremiumOnly_ShowsLockIcon()
    {
        // Arrange
        var item = new ShopItemDto(
            Guid.NewGuid(),
            "Diamond Avatar",
            "Exclusive avatar",
            "Avatar",
            0,
            "Legendary",
            "#FFD700",
            "diamond.png",
            true,
            false,
            true,
            null);

        // Act
        var cut = Render<ShopItemCard>(parameters =>
            parameters.Add(p => p.Item, item)
                      .Add(p => p.IsOwned, false));

        // Assert
        cut.Markup.Should().Contain("Premium");
    }

    [Fact]
    public void ShopItemCard_ClickBuy_TriggersOnPurchase()
    {
        // Arrange
        var item = new ShopItemDto(
            Guid.NewGuid(),
            "Test Item",
            "Description",
            "Avatar",
            100,
            "Common",
            "#9E9E9E",
            "avatar.png",
            false,
            false,
            true,
            null);
        
        var purchaseTriggered = false;
        var cut = Render<ShopItemCard>(parameters =>
            parameters.Add(p => p.Item, item)
                      .Add(p => p.IsOwned, false)
                      .Add(p => p.OnPurchase, () => { purchaseTriggered = true; }));

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert
        purchaseTriggered.Should().BeTrue();
    }

    [Fact]
    public void ShopItemCard_ClickEquip_TriggersOnEquip()
    {
        // Arrange
        var item = new ShopItemDto(
            Guid.NewGuid(),
            "Test Item",
            "Description",
            "Avatar",
            100,
            "Common",
            "#9E9E9E",
            "avatar.png",
            false,
            false,
            true,
            null);
        
        var equipTriggered = false;
        var cut = Render<ShopItemCard>(parameters =>
            parameters.Add(p => p.Item, item)
                      .Add(p => p.IsOwned, true)
                      .Add(p => p.IsEquipped, false)
                      .Add(p => p.OnEquip, () => { equipTriggered = true; }));

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert
        equipTriggered.Should().BeTrue();
    }

    [Theory]
    [InlineData("Common", "#9E9E9E")]
    [InlineData("Rare", "#2196F3")]
    [InlineData("Epic", "#9C27B0")]
    [InlineData("Legendary", "#FFD700")]
    public void ShopItemCard_RarityColor_AppliedCorrectly(string rarity, string color)
    {
        // Arrange
        var item = new ShopItemDto(
            Guid.NewGuid(),
            "Test Item",
            "Description",
            "Avatar",
            100,
            rarity,
            color,
            "avatar.png",
            false,
            false,
            true,
            null);

        // Act
        var cut = Render<ShopItemCard>(parameters =>
            parameters.Add(p => p.Item, item)
                      .Add(p => p.IsOwned, false));

        // Assert
        cut.Markup.Should().Contain(color);
    }
}
