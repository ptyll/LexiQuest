# UI-UX-004: Hlavní Dashboard

## Layout Overview

```
┌─────────────────────────────────────────────────────────────┐
│  🔥 LexiQuest          [🏠] [🎮] [🏆] [👤]      [🔔] [👤]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────┐│
│  │    1,240    │ │     15      │ │     89%     │ │   3.2s  ││
│  │     ⭐      │ │     🔥      │ │    📊       │ │   ⏱️    ││
│  │     XP      │ │   STREAK    │ │  ÚSPĚŠNOST  │ │  PRŮMĚR ││
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────┘│
│                                                             │
│  ┌─────────────────────────┐ ┌─────────────────────────────┐│
│  │  📅 Dnešní výzva        │ │  🏆 Tvá liga                ││
│  │                         │ │                             ││
│  │  ⚡ Rychlost (2x XP)    │ │  Zlatá liga                 ││
│  │                         │ │  ━━━━━━━━━━━━━━━━━━━━━━━━   ││
│  │  Slovo dne:             │ │  12. místo z 30             ││
│  │  ┌───────────────────┐  │ │  Ještě 120 XP k postupu     ││
│  │  │ P Ř E S M Y Č K A │  │ │                             ││
│  │  └───────────────────┘  │ │  [Zobrazit žebříček →]      ││
│  │                         │ │                             ││
│  │  [🎮 Hrát denní výzvu]  │ │  Končí za: 2d 14h           ││
│  └─────────────────────────┘ └─────────────────────────────┘│
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐│
│  │  🌱 Tvoje cesty                                         ││
│  │                                                         ││
│  │  Cesta 1: ████████████████████ 100% ✅                  ││
│  │  Cesta 2: ██████████████░░░░░░ 70%  ▶ Pokračovat       ││
│  │  Cesta 3: ████░░░░░░░░░░░░░░░░ 20%  🔒                 ││
│  │  Cesta 4: ░░░░░░░░░░░░░░░░░░░░ 0%   🔒                 ││
│  │                                                         ││
│  │  [🗺️ Zobrazit všechny cesty →]                          ││
│  └─────────────────────────────────────────────────────────┘│
│                                                             │
│  ┌─────────────────────────────────────────────────────────┐│
│  │  📊 Aktivita za posledních 30 dní                       ││
│  │                                                         ││
│  │  Po Út St Čt Pá So Ne                                   ││
│  │  ██░░██░░██░░██░██░                                     ││
│  │  ███░███░███░███░██                                     ││
│  │  ░░░░░░░░░░░░██░░░░░  ← dnes                            ││
│  │                                                         ││
│  └─────────────────────────────────────────────────────────┘│
│                                                             │
│  ┌─────────────────────────┐ ┌─────────────────────────────┐│
│  │  🏅 Nejnovější          │ │  🎯 Rychlý přístup          ││
│  │     achievementy        │ │                             ││
│  │                         │ │  [🎮 Trénink]               ││
│  │  🥇 Streak Master       │ │  [⏱️ Časovka]               ││
│  │  🥈 Word Solver         │ │  [⚔️ 1v1 Souboj]             ││
│  │  🥉 Speed Demon         │ │                             ││
│  │                         │ │  [🛡️ Obchod]                ││
│  │  [Všechny achievementy] │ │                             ││
│  └─────────────────────────┘ └─────────────────────────────┘│
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Komponenty

### 1. Stat Cards (Header)

```
┌─────────────────┐
│     1,240       │  ← Hodnota: --text-3xl, --font-bold
│       ⭐        │  ← Ikona: 32px, gradient
│       XP        │  ← Label: --text-sm, --color-gray-600
└─────────────────┘
```

#### Design Specs
- **Background:** White
- **Border-radius:** --radius-2xl
- **Padding:** --space-5
- **Shadow:** --shadow-md
- **Hover:** --shadow-lg, translateY(-2px)
- **Transition:** all --duration-fast

#### Animace
```
On load: Stagger fade in + slide up
On number change: Count up animation
```

---

### 2. Denní Výzva Card

```
┌────────────────────────────────┐
│ 📅 Dnešní výzva                │
│ ──────────────────────────     │
│                                │
│ ⚡ Rychlost (2x XP)           │  ← Tag s modifikátorem
│                                │
│ ┌──────────────────────────┐   │
│ │                          │   │
│ │   P Ř E S M Y Č K A      │   │  ← Zamíchané slovo
│ │                          │   │     40px letters, spaced
│ │                          │   │
│ └──────────────────────────┘   │
│                                │
│ [🎮 Hrát denní výzvu]          │  ← Primary button
└────────────────────────────────┘
```

#### Stav: Již dokončeno
```
┌────────────────────────────────┐
│ 📅 Denní výzva  ✅             │
│ ──────────────────────────     │
│                                │
│ ⚡ Rychlost (2x XP)           │
│                                │
│   ✓ DOKONČENO!                 │
│   Tvůj čas: 4.2s              │
│   Získáno: 50 XP              │
│                                │
│   [👥 Zobrazit žebříček]       │
└────────────────────────────────┘
```

---

### 3. Liga Card

```
┌────────────────────────────────┐
│ 🏆 Tvá liga                    │
│ ──────────────────────────     │
│                                │
│   🥇 Zlatá liga               │  ← Ikona ligy + název
│                                │
│   ━━━━━━━━━━━━━━━━━━━━━━━━    │  ← Progress k postupu
│                                │
│   12. místo z 30              │
│   Ještě 120 XP k postupu      │  ← Motivační text
│                                │
│   [Zobrazit žebříček →]       │
│                                │
│   Končí za: 2d 14h ⏰         │  ← Odpočet
└────────────────────────────────┘
```

#### Liga ikony
```
Bronze:   🥉 --color-league-bronze
Silver:   🥈 --color-league-silver
Gold:     🥇 --color-league-gold
Diamond:  💎 --color-league-diamond
Legend:   👑 --color-league-legend
```

---

### 4. Cesty Progress

```
Cesta 1: Začátečník
████████████████████ 100% ✅
[Dokončeno]

Cesta 2: Pokročilý  
████████████░░░░░░░░ 70%
[Pokračovat v levelu 18 →]

Cesta 3: Expert
████░░░░░░░░░░░░░░░░ 20% 🔒
[Odemyká se na levelu 10]
```

#### Progress bar
```
Height: 12px
Border-radius: --radius-full
Track: --color-gray-200
Fill: gradient (--color-primary-500 → --color-primary-600)
```

---

### 5. Heatmapa Aktivity

```css
/* GitHub-style heatmap */
.heatmap {
  display: grid;
  grid-template-columns: repeat(30, 1fr);
  gap: 3px;
}

.heatmap-day {
  width: 12px;
  height: 12px;
  border-radius: 2px;
}

/* Colors by activity level */
.level-0 { background: #ebedf0; }
.level-1 { background: #9be9a8; }
.level-2 { background: #40c463; }
.level-3 { background: #30a14e; }
.level-4 { background: #216e39; }
```

#### Tooltip na hover
```
┌────────────────────┐
│ 15. února 2024     │
│ 3 levely dokončeny │
│ 45 XP získáno      │
└────────────────────┘
```

---

### 6. Streak Indicator (v headeru nebo samostatně)

```
┌──────────────────────────┐
│     🔥                   │
│   15 dní                │
│  v řadě!                │
│                          │
│ Další den za: 8h 32m    │
│ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░     │
│                          │
│ 🛡️ Shield dostupný      │
└──────────────────────────┘
```

#### Fire animation
```css
@keyframes flame-flicker {
  0%, 100% { transform: scale(1) rotate(-2deg); }
  25% { transform: scale(1.05) rotate(2deg); }
  50% { transform: scale(0.95) rotate(-1deg); }
  75% { transform: scale(1.02) rotate(1deg); }
}

.fire-icon {
  animation: flame-flicker 1s ease-in-out infinite;
}
```

---

### 7. Quick Actions

```
┌──────────────────────────┐
│ 🎯 Rychlý přístup        │
│ ──────────────────────   │
│                          │
│ ┌──────────────────────┐ │
│ │ 🎮 Trénink           │ │
│ └──────────────────────┘ │
│ ┌──────────────────────┐ │
│ │ ⏱️ Časovka (3 min)   │ │
│ └──────────────────────┘ │
│ ┌──────────────────────┐ │
│ │ ⚔️ 1v1 Souboj        │ │
│ └──────────────────────┘ │
│ ┌──────────────────────┐ │
│ │ 🛡️ Obchod            │ │
│ └──────────────────────┘ │
└──────────────────────────┘
```

---

## Responzivita

### Desktop (> 1200px)
- 4 stat cards v řadě
- 2 sloupce: Denní výzva + Liga
- Heatmap plná šířka

### Tablet (768px - 1200px)
- 2 stat cards v řadě
- Denní výzva a Liga pod sebou

### Mobile (< 768px)
- Stat cards: 2x2 grid
- Vše pod sebou
- Horizontal scroll pro heatmap

## Animace

### Page Load Sequence
```
1. Header stats: Stagger 100ms
2. Daily challenge: Fade in, delay 400ms
3. League card: Fade in, delay 500ms
4. Paths: Slide up, delay 600ms
5. Heatmap: Fade in, delay 700ms
```

### Number Animations
```javascript
// Count up animation for stats
animateValue(element, start, end, duration) {
  const range = end - start;
  const increment = range / (duration / 16);
  let current = start;
  
  const timer = setInterval(() => {
    current += increment;
    element.textContent = Math.floor(current);
    if (current >= end) clearInterval(timer);
  }, 16);
}
```

### Streak Warning
```
Když streak končí do 6 hodin:
- Card border: pulsing orange
- Badge: "⚠️ Poslední šance!"
- Push notifikace
```

## Resource klíče

```
Dashboard.Title
Dashboard.Stats.XP
Dashboard.Stats.Streak
Dashboard.Stats.Accuracy
Dashboard.Stats.AvgTime
Dashboard.DailyChallenge.Title
Dashboard.DailyChallenge.Completed
Dashboard.DailyChallenge.Time
Dashboard.DailyChallenge.XP
Dashboard.DailyChallenge.PlayButton
Dashboard.League.Title
Dashboard.League.Rank
Dashboard.League.XPToPromotion
Dashboard.League.TimeRemaining
Dashboard.League.ViewLeaderboard
Dashboard.Paths.Title
Dashboard.Paths.Completed
Dashboard.Paths.Continue
Dashboard.Paths.Locked
Dashboard.Paths.LevelRequirement
Dashboard.Paths.ViewAll
Dashboard.Activity.Title
Dashboard.Achievements.Title
Dashboard.Achievements.ViewAll
Dashboard.QuickActions.Title
Dashboard.QuickActions.Training
Dashboard.QuickActions.TimeAttack
Dashboard.QuickActions.PvP
Dashboard.QuickActions.Shop
```
