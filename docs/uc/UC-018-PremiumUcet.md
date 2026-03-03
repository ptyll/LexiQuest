# UC-018: Prémiový účet (Premium)

## Popis
Placená verze aplikace s extra funkcemi a výhodami.

## Ceník

| Plán | Cena | Vlastnosti |
|------|------|------------|
| Měsíční | 99 Kč | Všechny Premium funkce |
| Roční | 899 Kč | 25% sleva |
| Lifetime | 2 499 Kč | Jednorázově, navždy |

## Premium výhody

### Herní
- [ ] Žádné reklamy
- [ ] Streak Freeze (1x týdně automaticky)
- [ ] Streak Shield (1x týden)
- [ ] Dvojité XP o víkendech
- [ ] Exkluzivní cesty a levely
- [ ] Vlastní slovníky

### Statistiky
- [ ] Podrobné analýzy
- [ ] Export dat (CSV/JSON)
- [ ] Historie všech her
- [ ] Porovnání s přáteli

### Personalizace
- [ ] Vlastní avatar (nahrání fotky)
- [ ] Exkluzivní avatary
- [ ] Exkluzivní avatar frame
- [ ] Exkluzivní téma
- [ ] Custom color scheme

### Multiplayer
- [ ] Přístup do Diamond ligy
- [ ] Exkluzivní turnaje
- [ ] Vytváření týmů

### Podpora
- [ ] Prioritní podpora
- [ ] Beta přístup k novinkám

## Hlavní tok - Nákup

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Uživatel otevře Premium stránku | - |
| 2 | Zobrazení výhod a cen | - |
| 3 | Výběr plánu | Měsíční/Roční/Lifetime |
| 4 | Zobrazení payment form | Stripe/PayPal |
| 5 | Zadání platebních údajů | - |
| 6 | Potvrzení platby | - |
| 7 | Aktivace Premium | Okamžitě |
| 8 | Zobrazení potvrzení | "Vítej mezi Premium!" |
| 9 | Odeslání emailu | Potvrzení nákupu |

## DTOs

```csharp
public record PremiumStatus(
    bool IsPremium,
    DateTime? PremiumUntil,
    PremiumPlan? CurrentPlan,
    List<PremiumFeature> Features
);

public record PremiumPlan(
    string Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    string BillingPeriod,  // monthly, yearly, lifetime
    decimal? OriginalPrice,  // pro slevy
    int? DiscountPercentage
);

public record PurchasePremiumRequest(
    string PlanId,
    string PaymentMethodId
);

public enum PremiumFeature
{
    NoAds,
    StreakFreeze,
    StreakShield,
    DoubleXPWeekends,
    ExclusivePaths,
    CustomDictionaries,
    DetailedStats,
    DataExport,
    CustomAvatar,
    ExclusiveAvatars,
    ExclusiveThemes,
    DiamondLeague,
    TeamCreation,
    PrioritySupport
}
```

## Resource klíče

```
Premium.Title
Premium.Subtitle
Premium.Plan.Monthly
Premium.Plan.Yearly
Premium.Plan.Lifetime
Premium.Price.PerMonth
Premium.Price.PerYear
Premium.Price.OneTime
Premium.Feature.NoAds
Premium.Feature.StreakFreeze
Premium.Feature.StreakShield
Premium.Feature.DoubleXP
Premium.Feature.ExclusivePaths
Premium.Feature.CustomDictionaries
Premium.Feature.DetailedStats
Premium.Feature.DataExport
Premium.Feature.CustomAvatar
Premium.Feature.ExclusiveAvatars
Premium.Feature.ExclusiveThemes
Premium.Feature.DiamondLeague
Premium.Feature.TeamCreation
Premium.Feature.PrioritySupport
Premium.Button.Subscribe
Premium.Button.Upgrade
Premium.Button.Manage
Premium.Success.Upgrade
Premium.Success.Welcome
Premium.Cancel.Confirm
Premium.Cancel.Success
```

## Odhad: 16h (payment integrace je náročná)
