# UC-013: Ligy (Týdenní soutěž)

## Popis
Týdenní soutěžní systém kde uživatelé soutěží o postup do vyšších lig.

## Struktura lig

| Liga | Počet uživatelů | Postup | Sestup | Odměna |
|------|-----------------|--------|--------|--------|
| Bronzová | 30 | TOP 5 | - | 50 XP |
| Stříbrná | 30 | TOP 5 | Spodních 5 | 100 XP |
| Zlatá | 30 | TOP 5 | Spodních 5 | 200 XP |
| Diamantová | 30 | TOP 3 | Spodních 5 | 500 XP |
| Legenda | 50 | - | Spodních 10 | 1000 XP |

## Týdenní cyklus

```
Pondělí 00:00 - Reset lig
├── Uživatelé seřazeni podle XP z minulého týdne
├── TOP X postupuje
├── Spodní Y sestupuje
├── Nové ligy se naplní
└── XP se resetuje na 0

Neděle 23:59 - Konec týdne
├── Vyhodnocení výsledků
├── Odeslání odměn
└── Příprava nových lig
```

## Hlavní tok

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Uživatel získá XP | Hraním libovolného režimu |
| 2 | XP se přičte do týdenního součtu | Real-time aktualizace |
| 3 | Systém aktualizuje žebříček | SignalR broadcast |
| 4 | Uživatel otevře stránku Ligy | - |
| 5 | Zobrazení aktuální pozice | "Jsi 12. z 30" |
| 6 | Zobrazení gap k TOP 5 | "Ještě 120 XP k postupu" |
| 7 | Zobrazení náskok před sestupem | "Náskok 80 XP" |

## Žebříček

```
┌────────────────────────────────────────┐
│ 🏆 ZLATÁ LIGA - Týden 12               │
│ Končí za: 2 dny 14 h                   │
├────────────────────────────────────────┤
│ 1.  🥇 Hráč123      2450 XP            │
│ 2.  🥈 WordMaster   2380 XP            │
│ 3.  🥉 LexiKing     2100 XP            │
│                                        │
│ 4.      Speedy      1890 XP  ▲ postup  │
│ 5.      Solver      1850 XP  ▲ postup  │
│ ─────────────────────────────────────  │
│ 12. 👤 Ty           1240 XP            │
│ ─────────────────────────────────────  │
│ 26.     Loser99     320 XP   ▼ sestup  │
│ 27.     Noob        280 XP   ▼ sestup  │
└────────────────────────────────────────┘
```

## DTOs

```csharp
public record LeagueInfo(
    Guid Id,
    LeagueTier Tier,
    string Name,
    DateTime WeekStart,
    DateTime WeekEnd,
    TimeSpan TimeRemaining,
    List<LeagueParticipant> Participants,
    int UserRank,
    int UserWeeklyXP,
    int XPToPromotion,
    int XPToDemotionSafety,
    bool IsPromoted,
    bool IsDemoted
);

public record LeagueParticipant(
    int Rank,
    Guid UserId,
    string Username,
    string AvatarUrl,
    int WeeklyXP,
    bool IsCurrentUser,
    LeagueChangeStatus ChangeStatus  // Promoted, Demoted, Same, New
);

public enum LeagueTier
{
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    Diamond = 4,
    Legend = 5
}

public enum LeagueChangeStatus
{
    Promoted,    // ⬆️
    Demoted,     // ⬇️
    Same,        // ➡️
    New          // 🆕
}
```

## Background Job - Reset lig

```csharp
public class LeagueResetJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // 1. Vyhodnotit všechny aktuální ligy
        // 2. Přiřadit postup/sestup
        // 3. Vytvořit nové ligy pro nový týden
        // 4. Odeslat notifikace s výsledky
        // 5. Přičíst odměny
    }
}
```

## Resource klíče

```
League.Title
League.Current.Bronze
League.Current.Silver
League.Current.Gold
League.Current.Diamond
League.Current.Legend
League.Time.Remaining
League.Rank.Current
League.Rank.PromotionZone
League.Rank.DemotionZone
League.XP.NeededForPromotion
League.XP.SafeFromDemotion
League.Participant.You
League.Result.Promoted
League.Result.Demoted
League.Result.Same
League.Reward.Claimed
```

## Odhad: 14h
