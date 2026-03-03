# UI-UX-009: Statistiky a Heatmapa

## Layout

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  Statistiky                     🔥 15  💎 1,240      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  📊 Celkové statistiky                              │   │
│  │  ════════════════════════                           │   │
│  │                                                     │   │
│  │  ┌───────────┐ ┌───────────┐ ┌───────────┐         │   │
│  │  │  1,240    │ │   15      │ │    89%    │         │   │
│  │  │    ⭐     │ │    🔥     │ │    📊     │         │   │
│  │  │    XP     │ │   Streak  │ │  Úspěšnost│         │   │
│  │  └───────────┘ └───────────┘ └───────────┘         │   │
│  │                                                     │   │
│  │  ┌───────────┐ ┌───────────┐ ┌───────────┐         │   │
│  │  │   847     │ │   3.2s    │ │  1,245    │         │   │
│  │  │  📝       │ │    ⏱️     │ │    💎     │         │   │
│  │  │  Slov     │ │  Průměr   │ │  Od začátku│         │   │
│  │  └───────────┘ └───────────┘ └───────────┘         │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  📅 Aktivita za posledních 30 dní                   │   │
│  │  ═════════════════════════════════                  │   │
│  │                                                     │   │
│  │           Po Út St Čt Pá So Ne                      │   │
│  │     Týden 1  ██░░██░░██░░██░██░                    │   │
│  │     Týden 2  ███░███░███░███░██                    │   │
│  │     Týden 3  ░░░░░░░░░░░░██░░░░░  ← dnes          │   │
│  │     Týden 4  ██░░██░░██░░░░░░██░                   │   │
│  │                                                     │   │
│  │  Legenda:                                           │   │
│  │  ░ Žádná aktivita  ▓ Málo  ██ Středně  ███ Hodně   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  📈 Průběh v čase                                   │   │
│  │  ═════════════════                                  │   │
│  │                                                     │   │
│  │  [Line chart: XP za posledních 30 dní]             │   │
│  │                                                     │   │
│  │     XP                                              │   │
│  │  100 ┤         ╭─╮                                  │   │
│  │   80 ┤    ╭────╯ ╰──╮                               │   │
│  │   60 ┤╭───╯         ╰────╮                          │   │
│  │   40 ┤╯                  ╰────                     │   │
│  │   20 ┤                                              │   │
│  │    0 ┼────┬────┬────┬────┬────┬────┬────           │   │
│  │         1    5   10   15   20   25   30            │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────┐ ┌─────────────────────────────┐│
│  │  🎯 Nejlepší slova      │ │  📊 Podle obtížnosti        ││
│  │  ═══════════════════    │ │  ═══════════════════        ││
│  │                         │ │                             ││
│  │  1. PROGRAMÁTOR         │ │  🌱 Začátečník:             ││
│  │     8 písmen, 3.2s      │ │     98% úspěšnost          ││
│  │                         │ │     [██████████████░░]      ││
│  │  2. KONSTITUCE          │ │                             ││
│  │     10 písmen, 4.1s     │ │  🌿 Pokročilý:              ││
│  │                         │ │     85% úspěšnost          ││
│  │  3. BIOLOGIE            │ │     [████████████░░░░]      ││
│  │     8 písmen, 2.8s      │ │                             ││
│  │                         │ │  🌳 Expert:                 ││
│  │  [Zobrazit všechna →]   │ │     72% úspěšnost          ││
│  │                         │ │     [██████████░░░░░░]      ││
│  └─────────────────────────┘ └─────────────────────────────┘│
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  🏆 Rekordy                                         │   │
│  │  ═════════════════                                  │   │
│  │                                                     │   │
│  │  Nejrychlejší odpověď:    1.2s  - SLOVO            │   │
│  │  Nejdelší streak:         45 dní                   │   │
│  │  Nejvyšší combo:          15x                      │   │
│  │  Nejvíce XP za den:       1,450 XP                 │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Heatmap Component

### Design

```css
.heatmap {
  display: grid;
  grid-template-columns: auto repeat(7, 1fr);
  gap: 4px;
}

.heatmap-day {
  width: 16px;
  height: 16px;
  border-radius: 3px;
  transition: transform 0.2s;
}

.heatmap-day:hover {
  transform: scale(1.3);
  z-index: 10;
  box-shadow: 0 0 10px rgba(0,0,0,0.2);
}
```

### Colors

```css
/* Activity levels */
.level-0 { background: #ebedf0; }  /* 0 */
.level-1 { background: #9be9a8; }  /* 1-2 levels */
.level-2 { background: #40c463; }  /* 3-5 levels */
.level-3 { background: #30a14e; }  /* 6-10 levels */
.level-4 { background: #216e39; }  /* 10+ levels */
```

### Tooltip

```
┌─────────────────────────────┐
│  15. února 2024            │
│  ─────────────────          │
│  ✅ 3 levely dokončeny      │
│  ⭐ 45 XP získáno           │
│  🔥 Streak pokračoval       │
└─────────────────────────────┘
```

---

## Charts

### XP Progress Line Chart

```
XP
100 ┤                              ╭──╮
 80 ┤                    ╭────────╯  │
 60 ┤         ╭─────────╯            ╰───
 40 ┤   ╭────╯
 20 ┤───╯
  0 ┼────┬────┬────┬────┬────┬────┬────
      1    5   10   15   20   25   30
```

#### Chart colors
```
Line: --color-primary-500
Fill: rgba(255, 152, 0, 0.1)
Grid: --color-gray-200
Text: --color-gray-600
```

---

## Stat Cards

### Design

```
┌─────────────────┐
│                 │
│      1,240      │  ← Value: 28px, bold
│        ⭐       │  ← Icon: 32px
│        XP       │  ← Label: 14px, gray
│                 │
└─────────────────┘
```

### Animations
```
On load: Count up from 0
On hover: Scale 1.02, shadow increase
```

---

## Best Words Section

```
┌─────────────────────────────────────┐
│                                     │
│  🎯 Nejlepší slova                  │
│  ═══════════════════                │
│                                     │
│  ┌─────────────────────────────┐   │
│  │  1. PROGRAMÁTOR             │   │
│  │     ████████░░  8 písmen   │   │
│  │     ⚡ 3.2s                  │   │
│  └─────────────────────────────┘   │
│                                     │
│  ┌─────────────────────────────┐   │
│  │  2. KONSTITUCE              │   │
│  │     ██████████  10 písmen  │   │
│  │     ⚡ 4.1s                  │   │
│  └─────────────────────────────┘   │
│                                     │
│  ┌─────────────────────────────┐   │
│  │  3. BIOLOGIE                │   │
│  │     ████████░░  8 písmen   │   │
│  │     ⚡ 2.8s                  │   │
│  └─────────────────────────────┘   │
│                                     │
└─────────────────────────────────────┘
```

---

## Difficulty Breakdown

```
┌─────────────────────────────────────┐
│                                     │
│  📊 Podle obtížnosti                │
│  ═══════════════════                │
│                                     │
│  🌱 Začátečník                      │
│  98% úspěšnost                      │
│  [████████████████░░]               │
│  245 slov                           │
│                                     │
│  🌿 Pokročilý                       │
│  85% úspěšnost                      │
│  [████████████░░░░░░]               │
│  412 slov                           │
│                                     │
│  🌳 Expert                          │
│  72% úspěšnost                      │
│  [██████████░░░░░░░░]               │
│  190 slov                           │
│                                     │
└─────────────────────────────────────┘
```

---

## Resource klíče

```
Statistics.Title
Statistics.Total.XP
Statistics.Total.Streak
Statistics.Total.Accuracy
Statistics.Total.Words
Statistics.Total.AvgTime
Statistics.Total.SinceStart
Statistics.Activity.Title
Statistics.Activity.Last30Days
Statistics.Activity.Tooltip.Date
Statistics.Activity.Tooltip.Levels
Statistics.Activity.Tooltip.XP
Statistics.Activity.Tooltip.Streak
Statistics.Activity.Legend.None
Statistics.Activity.Legend.Low
Statistics.Activity.Legend.Medium
Statistics.Activity.Legend.High
Statistics.Activity.Legend.VeryHigh
Statistics.Chart.XPOverTime
Statistics.BestWords.Title
Statistics.BestWords.Length
Statistics.BestWords.Time
Statistics.BestWords.ViewAll
Statistics.ByDifficulty.Title
Statistics.ByDifficulty.Beginner
Statistics.ByDifficulty.Intermediate
Statistics.ByDifficulty.Expert
Statistics.Records.Title
Statistics.Records.FastestAnswer
Statistics.Records.LongestStreak
Statistics.Records.HighestCombo
Statistics.Records.MostXPDay
```
