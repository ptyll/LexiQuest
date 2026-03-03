# UI-UX-006: Cesty a Levely

## Layout - Seznam cest

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  Učební cesty                                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Vyber si cestu podle své úrovně:                          │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 🌱 Začátečník                                        │   │
│  │ ─────────────────                                   │   │
│  │                                                      │   │
│  │  3-5 písmen • Bez časového limitu                   │   │
│  │                                                      │   │
│  │  ████████████████████ 100% ✅                       │   │
│  │                                                      │   │
│  │  [📊 Detaily]  [🔄 Opakovat]                        │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 🌿 Mírně pokročilý                                   │   │
│  │ ─────────────────                                   │   │
│  │                                                      │   │
│  │  5-7 písmen • 30s limit • Nápovědy                  │   │
│  │                                                      │   │
│  │  ██████████████░░░░░░ 70%  ▶                        │   │
│  │                                                      │   │
│  │  [▶️ Pokračovat v levelu 15]                        │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 🌳 Pokročilý                                         │   │
│  │ ─────────────────                                   │   │
│  │                                                      │   │
│  │  7-10 písmen • 20s limit • Nápovědy za XP           │   │
│  │                                                      │   │
│  │  ████░░░░░░░░░░░░░░░░ 20% 🔒                        │   │
│  │                                                      │   │
│  │  🔒 Odemyká se na levelu 10                         │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 🔥 Expert                                            │   │
│  │ ─────────────────                                   │   │
│  │                                                      │   │
│  │  10+ písmen • 10s limit • Falešné stopy             │   │
│  │                                                      │   │
│  │  ░░░░░░░░░░░░░░░░░░░░ 0% 🔒                         │   │
│  │                                                      │   │
│  │  🔒 Odemyká se po dokončení Cesty 3                 │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Detail cesty - Mapa levelů

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  Cesta 2: Mírně pokročilý               70%          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│                        🏁                                   │
│                         │                                   │
│                        25 🎉                                │
│                         │                                   │
│                        24 ◀️ BOSS                           │
│                       ╱ │ ╲                                 │
│                     22   23                                 │
│                      │   │                                  │
│                     20   21                                 │
│                    ╱                                        │
│                  18 ✓                                       │
│                  │                                          │
│                 17 ✓                                        │
│               ╱ │                                           │
│             15 ✓  16 ✓                                      │
│            ╱                                                │
│          13 ✓                                               │
│          │                                                  │
│         12 ✓                                                │
│       ╱ │ ╲                                                 │
│     10 ✓  11 ✓                                              │
│    ╱                                                        │
│   9 ✓                                                       │
│   │                                                         │
│   8 ✓                                                       │
│   │                                                         │
│   7 ✓                                                       │
│  ╱                                                          │
│ 6 ✓                                                         │
│ │                                                           │
│ 5 ✓                                                         │
│ │                                                           │
│ 4 ✓                                                         │
│ │                                                           │
│ 3 ✓                                                         │
│ │                                                           │
│ 2 ✓                                                         │
│ │                                                           │
│ 1 ✓                                                         │
│                                                             │
│ 🌱 START                                                    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Level Node Design

### Stavy nodů

#### Dokončený (zelený)
```
┌─────────────┐
│      ✓      │
│             │
│     15      │
└─────────────┘
```
- **Background:** --color-success-500
- **Border:** none
- **Icon:** Checkmark

#### Aktuální (oranžový, pulzující)
```
┌─────────────┐
│             │
│     18      │
│    ────▶    │
└─────────────┘
```
- **Background:** --color-primary-500
- **Animation:** Pulse + glow
- **Border:** 3px solid white
- **Shadow:** --shadow-glow-primary

#### Zamčený (šedý)
```
┌─────────────┐
│      🔒     │
│             │
│     20      │
└─────────────┘
```
- **Background:** --color-gray-300
- **Opacity:** 0.6
- **Icon:** Lock

#### Nedostupný (neviditelný)
```
┌─────────────┐
│      ?      │
│             │
│     ??      │
└─────────────┘
```
- **Background:** --color-gray-200
- **Content:** Otazník

#### Boss (speciální)
```
┌─────────────┐
│     👹      │
│             │
│    BOSS     │
└─────────────┘
```
- **Background:** Gradient (red → orange)
- **Border:** 2px solid gold
- **Animation:** Shake periodically
- **Icon:** Crown/Monster

---

## Detail levelu (Modal)

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │               Level 18                              │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │                                             │   │   │
│  │  │  [Preview slova - zamíchané]                │   │   │
│  │  │                                             │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  📋 Informace:                                      │   │
│  │                                                     │   │
│  │  📝 Počet slov: 10                                  │   │
│  │  ⏱️ Čas na slovo: 30 sekund                         │   │
│  │  💡 Nápovědy: 3                                     │   │
│  │  ❤️ Životy: 5                                       │   │
│  │                                                     │   │
│  │  🏆 Odměny:                                         │   │
│  │     ⭐ 100 XP                                        │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │           ▶️ STARTOVAT LEVEL                  │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │           [✕ Zavřít]                               │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Path Progress Bar

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  Cesta 2: Mírně pokročilý                                   │
│                                                             │
│  ████████████████████░░░░░░░░░░░░░░░░  18 / 25 levelů      │
│                                                             │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━    │
│  ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░░░░░░░ 72%                 │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### Design
- **Track height:** 16px
- **Border-radius:** --radius-full
- **Track:** --color-gray-200
- **Fill:** Gradient (--color-success-500 → --color-primary-500)
- **Markers:** Boss levels značeny zlatými tečkami

---

## Boss Level Detail

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │                  👹 BOSS LEVEL                      │   │
│  │                                                     │   │
│  │               Level 24 - Maraton                    │   │
│  │                                                     │   │
│  │  ⚠️ VAROVÁNÍ: Tento level je obtížný!              │   │
│  │                                                     │   │
│  │  Pravidla:                                          │   │
│  │  • 20 slov bez obnovy životů                       │   │
│  │  • 3 životy na celý maraton                        │   │
│  │  • Čas na slovo: 15 sekund                         │   │
│  │                                                     │   │
│  │  💪 Jsi připraven?                                  │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │        ⚔️ VZÍT BOSSA                         │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  [🏃 Tréninkový mód]                                │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Animace

### Node Unlock
```
1. Node starts gray
2. Ripple effect from previous node
3. Gray node scales down
4. Color burst
5. Node scales up with new color
6. Particles emanate
```

### Path Connection
```css
@keyframes path-draw {
  from { stroke-dashoffset: 100%; }
  to { stroke-dashoffset: 0; }
}

.path-line {
  stroke-dasharray: 100%;
  animation: path-draw 1s ease-out forwards;
}
```

### Level Complete
```
1. Node pulses
2. Checkmark draws in
3. Confetti burst from node
4. Next node unlocks with animation
5. Camera pans to next node (smooth scroll)
```

---

## Responzivita

### Mobile
- Nodes: 40px (místo 60px)
- Vertical scroll for path
- Collapsible path cards
- Modal becomes full-screen

### Tablet
- 2 columns for paths list
- Full map view

---

## Resource klíče

```
Paths.Title
Paths.Select
Paths.Beginner.Title
Paths.Beginner.Description
Paths.Beginner.Difficulty
Paths.Intermediate.Title
Paths.Intermediate.Description
Paths.Intermediate.Difficulty
Paths.Advanced.Title
Paths.Advanced.Description
Paths.Advanced.Difficulty
Paths.Expert.Title
Paths.Expert.Description
Paths.Expert.Difficulty
Paths.Progress.Completed
Paths.Progress.Continue
Paths.Progress.Locked
Paths.Locked.Requirement
Paths.Level.Start
Paths.Level.Info
Paths.Level.Words
Paths.Level.Time
Paths.Level.Hints
Paths.Level.Lives
Paths.Level.Rewards
Paths.Boss.Title
Paths.Boss.Warning
Paths.Boss.Rules
Paths.Boss.Start
Paths.Boss.Training
Paths.Node.Completed
Paths.Node.Current
Paths.Node.Locked
Paths.Node.Boss
```
