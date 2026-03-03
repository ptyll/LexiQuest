# UC-005: Životy systém

## Popis
Systém životů omezuje počet pokusů v náročnějších režimech a přidává napětí do hry.

## Aktéři
- **Primary Actor:** Hráč

## Pravidla životů

| Režim | Počet životů | Obnova |
|-------|--------------|--------|
| Trénink | ∞ | - |
| Začátečník cesta | 5 | Mezi levely |
| Mírně pokročilý | 4 | Mezi levely |
| Pokročilý | 3 | Každých 30 min |
| Expert | 3 | Každých 60 min |
| Boss level | 1-3 | Žádná (maraton) |

## Obnova životů

### Automatická obnova (časová)
```csharp
public class LifeRegenerationService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await RegenerateLivesAsync();
            await Task.Delay(TimeSpan.FromMinutes(5), ct);
        }
    }
    
    private async Task RegenerateLivesAsync()
    {
        // Najdi uživatele s LastLifeLostAt > 30 minut
        // Přidej život pokud < max
    }
}
```

### Okamžité doplnění (Premium/Mince)
- **Premium:** Denně 1x zdarma doplnění na plno
- **Mince:** 100 mincí za doplnění (volitelná měna)
- **Reklama:** Doplnění po zhlédnutí reklamy (free uživatelé)

## Hlavní tok - Ztráta života

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Hráč odpoví špatně | - |
| 2 | Systém odečte 1 život | Animace praskání srdce |
| 3 | Kontrola zbylých životů | - |
| 4a | Životy > 0 | Pokračování ve hře |
| 4b | Životy = 0 | Konec hry, Game Over obrazovka |

## Hlavní tok - Obnova života

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Systém detekuje nárok na obnovu | Kontrola času |
| 2 | Přičte život | +1 do LivesRemaining |
| 3 | Aktualizuje LastLifeRegeneratedAt | - |
| 4 | Notifikace uživateli | "+1 život obnoven!" |

## Game Over scénář

```
Když LivesRemaining = 0:
1. Ukončit GameSession
2. Zobrazit Game Over obrazovku
3. Možnosti:
   - Začít znovu (od začátku levelu)
   - Doplnit životy (Premium/mince)
   - Vrátit se na Dashboard
4. Zachovat získané XP z předchozích kol
```

## DTOs

```csharp
public record LivesStatus(
    int Current,
    int Max,
    DateTime? NextRegenerationAt,
    TimeSpan? TimeUntilNextRegen,
    bool CanRefillForFree,
    int? RefillCost
);

public record RefillLivesRequest(
    RefillMethod Method  // Free, Coins, Ad, Premium
);
```

## Resource klíče

```
Lives.Indicator.Label
Lives.Status.Full
Lives.Status.Partial
Lives.Status.Empty
Lives.Regen.NextIn
Lives.Regen.Ready
Lives.Refill.Button
Lives.Refill.Free
Lives.Refill.Cost
Lives.Refill.WatchAd
Lives.GameOver.Title
Lives.GameOver.Message
Lives.GameOver.Retry
Lives.GameOver.BackToDashboard
```

## Odhad: 8h
