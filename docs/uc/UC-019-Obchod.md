# UC-019: Obchod (Shop)

## Popis
Nákup kosmetických předmětů a boostů pomocí herní měny (mince).

## Měna

### Získání mincí
| Zdroj | Množství |
|-------|----------|
| Dokončení levelu | 10 mincí |
| Boss level | 50 mincí |
| Denní výzva | 20 mincí |
| Achievement | 50-200 mincí |
| Liga (TOP 3) | 100-500 mincí |
| Nákup za peníze | 1000 = 49 Kč |

### Kategorie předmětů

#### Avatary (50-500 mincí)
- Základní set (zdarma)
- Animální set (100 mincí)
- Profesní set (150 mincí)
- Fantazy set (200 mincí)
- Sezónní (limitované, 300 mincí)

#### Avatar Frames (100-1000 mincí)
- Bronze frame (100 mincí)
- Silver frame (250 mincí)
- Gold frame (500 mincí)
- Diamond frame (1000 mincí)
- Speciální (eventové)

#### Témata (200-500 mincí)
- Dark mode (zdarma)
- Nature theme (200 mincí)
- Cyberpunk theme (300 mincí)
- Retro theme (250 mincí)
- Seasonal themes (limitované)

#### Boosty (consumables)
- XP Boost (+50% na 1h) - 100 mincí
- Streak Shield navíc - 150 mincí
- Extra životy (okamžité) - 50 mincí

## DTOs

```csharp
public record ShopItem(
    Guid Id,
    string Name,
    string Description,
    string Type,  // Avatar, Frame, Theme, Boost
    int Price,
    string Currency,  // coins / premium
    string IconUrl,
    bool IsOwned,
    bool IsLimited,
    DateTime? AvailableUntil,
    bool IsPremiumOnly
);

public record UserInventory(
    int CoinBalance,
    List<OwnedItem> OwnedItems,
    List<EquippedItem> EquippedItems
);

public record PurchaseRequest(
    Guid ItemId
);

public record PurchaseResult(
    bool Success,
    int RemainingBalance,
    string? ErrorMessage
);
```

## Resource klíče

```
Shop.Title
Shop.Balance.Coins
Shop.Category.Avatars
Shop.Category.Frames
Shop.Category.Themes
Shop.Category.Boosts
Shop.Item.Owned
Shop.Item.Equipped
Shop.Item.Limited
Shop.Item.PremiumOnly
Shop.Button.Buy
Shop.Button.Equip
Shop.Button.Unequip
Shop.Button.BuyCoins
Shop.Success.Purchase
Shop.Success.Equip
Shop.Error.InsufficientFunds
Shop.Error.AlreadyOwned
```

## Odhad: 10h
