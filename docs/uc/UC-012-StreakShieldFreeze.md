# UC-012: Streak Shield a Freeze

## Popis
Premium funkce pro ochranu streaku před ztrátou při vynechání dne.

## Shield vs Freeze

| Funkce | Popis | Dostupnost |
|--------|-------|------------|
| **Streak Shield** | Ruční aktivace, chrání před 1 ztrátou | Free: 1x/měsíc, Premium: 1x/týden |
| **Streak Freeze** | Automatická ochrana, aktivuje se sama | Premium: 1 den/týden |

## Shield tok

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Uživatel otevře streak detail | - |
| 2 | Vidí možnost "Aktivovat Shield" | Ikonka štítu |
| 3 | Klikne na aktivaci | Confirm dialog |
| 4 | Systém aktivuje Shield | Záznam v DB |
| 5 | Zobrazení potvrzení | "Streak aktivně chráněn" |
| 6 | Při příštím vynechání | Shield se spotřebuje, streak zachován |

## Freeze tok

```
Když uživatel nesplní denní požadavek:
1. Systém detekuje konec dne
2. Kontroluje dostupný Freeze
3. Pokud ano → automaticky spotřebuje Freeze
4. Uživatel dostane notifikaci: "Streak zachráněn Freeze!"
5. Streak pokračuje
```

## DTOs

```csharp
public record StreakProtection(
    int ShieldsAvailable,
    int ShieldsUsedThisMonth,
    int MaxShieldsPerMonth,
    int FreezesAvailable,
    DateTime? FreezeExpiresAt,
    bool HasActiveShield,
    DateTime? ShieldActivatedAt,
    bool IsPremium
);

public record ActivateShieldRequest();
public record ActivateShieldResponse(
    bool Success,
    int RemainingShields,
    DateTime? NextShieldAvailableAt
);
```

## Monetizace

```
Free uživatel:
- 1 Shield za měsíc
- Žádný Freeze

Premium uživatel:
- 1 Shield za týden
- 1 Freeze za týden (automatický)
- Možnost koupit extra Shields za mince

Extra nákup:
- 3 Shields = 500 mincí
- Emergency Shield (okamžitá ochrana) = 300 mincí
```

## Resource klíče

```
Shield.Title
Shield.Description
Shield.Button.Activate
Shield.Button.Buy
Shield.Status.Active
Shield.Status.Used
Shield.Status.Available
Shield.Status.Unavailable
Shield.Confirm.Title
Shield.Confirm.Message
Shield.Success.Activated
Shield.Consumed.Message
Freeze.Title
Freeze.Description
Freeze.Status.Active
Freeze.Status.Consumed
Freeze.Notification.Saved
```

## Odhad: 6h
