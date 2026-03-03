# UC-015: Statistiky a Dashboard

## Popis
Kompletní přehled o výkonu, pokroku a aktivitě hráče.

## Sekce dashboardu

### 1. Header Cards
```
┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
│   1,240  │ │   15     │ │   89%    │ │   3.2s   │
│    XP    │ │  STREAK  │ │ ÚSPĚŠNOST│ │  PRŮMĚR  │
│  Celkem  │ │   dní    │ │ správně  │ │  čas     │
└──────────┘ └──────────┘ └──────────┘ └──────────┘
```

### 2. Aktivitní heatmapa (GitHub style)
```
Aktivita za posledních 30 dní:

Po Út St Čt Pá So Ne
██░░██░░██░░██░██░
███░███░███░███░██
░░░░░░░░░░██░░░░░░  ← dnes
```

### 3. Progress cest
```
Cesta 1: ████████████████████ 100% ✅
Cesta 2: ██████████████░░░░░░ 70%
Cesta 3: ████░░░░░░░░░░░░░░░░ 20% 🔒
```

### 4. Nejlepší slova
```
Nejtěžší vyřešená slova:
1. "PROGRAMÁTOR" - 3.2s
2. "KONSTITUCE"  - 4.1s
3. "BIOLOGIE"    - 2.8s
```

## Statistiky k zobrazení

| Statistika | Popis | Výpočet |
|------------|-------|---------|
| TotalXP | Celkové XP | Sum všech získaných XP |
| CurrentStreak | Aktuální streak | Dny od posledního vynechání |
| LongestStreak | Nejdelší streak | Max historický streak |
| Accuracy | Úspěšnost | Správné / Celkem * 100 |
| AvgTime | Průměrný čas | Sum časů / Počet slov |
| TotalWords | Celkem slov | Count řešených slov |
| UniqueWords | Unikátních slov | Count distinct slov |
| CurrentLevel | Aktuální level | Z TotalXP |
| CurrentLeague | Aktuální liga | - |
| BestTime | Nejrychlejší odpověď | Min čas |
| HardestWord | Nejdelší vyřešené slovo | Max délka |

## Heatmapa data

```csharp
public record ActivityHeatmap(
    List<HeatmapDay> Days,
    int MaxStreak,
    int CurrentStreak,
    int TotalActiveDays
);

public record HeatmapDay(
    DateTime Date,
    int ActivityLevel,  // 0-4 (barva)
    int LevelsCompleted,
    int XP
);
```

## Barvy heatmapy

```css
.activity-0 { background: #ebedf0; }  /* Nic */
.activity-1 { background: #9be9a8; }  /* Málo */
.activity-2 { background: #40c463; }  /* Středně */
.activity-3 { background: #30a14e; }  /* Hodně */
.activity-4 { background: #216e39; }  /* Extrém */
```

## DTOs

```csharp
public record UserDashboard(
    UserStatsSummary Stats,
    ActivityHeatmap Heatmap,
    List<PathProgress> PathProgresses,
    List<RecentAchievement> RecentAchievements,
    List<BestWord> BestWords,
    LeagueStatus LeagueStatus
);

public record UserStatsSummary(
    int TotalXP,
    int CurrentLevel,
    int CurrentStreak,
    int LongestStreak,
    double AccuracyPercentage,
    double AverageResponseTimeMs,
    int TotalWordsSolved,
    string CurrentLeague,
    string HardestWordSolved
);
```

## Resource klíče

```
Dashboard.Title
Dashboard.Stats.XP
Dashboard.Stats.Streak
Dashboard.Stats.Accuracy
Dashboard.Stats.AvgTime
Dashboard.Stats.TotalWords
Dashboard.Stats.Level
Dashboard.Stats.League
Dashboard.Heatmap.Title
Dashboard.Heatmap.Tooltip
Dashboard.Paths.Title
Dashboard.Paths.Completed
Dashboard.Paths.InProgress
Dashboard.Paths.Locked
Dashboard.Achievements.Recent
Dashboard.Words.Best
Dashboard.Words.Hardest
```

## Odhad: 10h
