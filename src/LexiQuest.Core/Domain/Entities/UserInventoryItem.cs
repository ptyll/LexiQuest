namespace LexiQuest.Core.Domain.Entities;

public class UserInventoryItem
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ShopItemId { get; private set; }
    public DateTime PurchasedAt { get; private set; }
    public bool IsEquipped { get; private set; }

    private UserInventoryItem() { }

    public static UserInventoryItem Create(Guid userId, Guid shopItemId)
    {
        return new UserInventoryItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ShopItemId = shopItemId,
            PurchasedAt = DateTime.UtcNow,
            IsEquipped = false
        };
    }

    public void Equip()
    {
        IsEquipped = true;
    }

    public void Unequip()
    {
        IsEquipped = false;
    }

    public void ToggleEquipped()
    {
        IsEquipped = !IsEquipped;
    }
}
