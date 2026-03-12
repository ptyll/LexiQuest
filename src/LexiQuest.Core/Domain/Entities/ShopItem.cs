using LexiQuest.Core.Domain.Enums;

namespace LexiQuest.Core.Domain.Entities;

public class ShopItem
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ShopCategory Category { get; private set; }
    public int Price { get; private set; }
    public ItemRarity Rarity { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsPremiumOnly { get; private set; }
    public bool IsLimited { get; private set; }
    public DateTime? AvailableUntil { get; private set; }

    private ShopItem() { }

    public static ShopItem Create(string name, string description, ShopCategory category, int price, ItemRarity rarity, string imageUrl)
    {
        return new ShopItem
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Category = category,
            Price = price,
            Rarity = rarity,
            ImageUrl = imageUrl,
            IsPremiumOnly = false,
            IsLimited = false,
            AvailableUntil = null
        };
    }

    public static ShopItem CreatePremiumOnly(string name, string description, ShopCategory category, int price, ItemRarity rarity, string imageUrl)
    {
        return new ShopItem
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Category = category,
            Price = price,
            Rarity = rarity,
            ImageUrl = imageUrl,
            IsPremiumOnly = true,
            IsLimited = false,
            AvailableUntil = null
        };
    }

    public static ShopItem CreateLimited(string name, string description, ShopCategory category, int price, ItemRarity rarity, string imageUrl, DateTime availableUntil)
    {
        return new ShopItem
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Category = category,
            Price = price,
            Rarity = rarity,
            ImageUrl = imageUrl,
            IsPremiumOnly = false,
            IsLimited = true,
            AvailableUntil = availableUntil
        };
    }

    public bool IsAvailable()
    {
        if (!IsLimited)
            return true;

        return AvailableUntil.HasValue && AvailableUntil.Value > DateTime.UtcNow;
    }

    public string GetRarityColor()
    {
        return Rarity switch
        {
            ItemRarity.Common => "#9E9E9E",
            ItemRarity.Rare => "#2196F3",
            ItemRarity.Epic => "#9C27B0",
            ItemRarity.Legendary => "#FFD700",
            _ => "#9E9E9E"
        };
    }
}
