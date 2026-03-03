# UI-UX-011: Achievementy

## Layout

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  Achievementy                   🔥 15  💎 1,240      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Tvůj postup                                        │   │
│  │  ═════════════════                                  │   │
│  │                                                     │   │
│  │  12 / 50 odemčeno                                   │   │
│  │  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━    │   │
│  │  ▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  24% │   │
│  │                                                     │   │
│  │  ⭐ Získáno XP z achievementů: 650                 │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  [Všechny] [Výkonnostní] [Streak] [Obtížnostní] [Speciální]│
│  ════════════════════════════════════════════════════════   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  🏆 STREAKY                      4 / 6              │   │
│  │  ═══════════════════════════════════════            │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  🔥                                         │   │   │
│  │  │  Začátek cesty                             │   │   │
│  │  │  3 dny streak                              │   │   │
│  │  │  ✅ Odemčeno 12. 3. 2024                   │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  🔥🔥                                       │   │   │
│  │  │  Týden věrnosti                             │   │   │
│  │  │  7 dní streak                               │   │   │
│  │  │  ✅ Odemčeno 18. 3. 2024                   │   │   │
│  │  │  🎁 50 XP • Avatar Frame                    │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  🔥🔥🔥                                     │   │   │
│  │  │  Měsíc mistra                               │   │   │
│  │  │  30 dní streak                              │   │   │
│  │  │  ░░░░░░░░░░░░░░░░░░░  15 / 30 dní         │   │   │
│  │  │  Ještě 15 dní!                            │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  🔒                                         │   │   │
│  │  │  Půl roku                                   │   │   │
│  │  │  183 dní streak                             │   │   │
│  │  │  🔒 Zamčeno                                │   │   │
│  │  │  🎁 500 XP • Speciální téma                 │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  🎯 VÝKONNOSTNÍ                  5 / 8              │   │
│  │  ═══════════════════════════════════════            │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  🥇                                         │   │   │
│  │  │  Streak Master                              │   │   │
│  │  │  10 slov bez chyby                          │   │   │
│  │  │  ✅ Odemčeno dnes                          │   │   │
│  │  │  🎁 50 XP                                   │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  🥈                                         │   │   │
│  │  │  Word Solver                                │   │   │
│  │  │  1000 slov                                  │   │   │
│  │  │  ▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░  847 / 1000          │   │   │
│  │  │  Ještě 153 slov!                          │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Achievement Card States

### Unlocked (Gold)

```
┌─────────────────────────────────────────────┐
│                                             │
│  ┌───────────┐                              │
│  │    🏆     │  Streak Master               │
│  │           │  10 slov bez chyby           │
│  └───────────┘                              │
│                                             │
│  ✅ Odemčeno 15. 3. 2024                   │
│  🎁 50 XP                                   │
│                                             │
└─────────────────────────────────────────────┘
```

**Design:**
- Background: --color-success-50
- Border: 2px solid --color-success-500
- Icon: Full color, no filter
- Badge: Gold shine animation

### In Progress

```
┌─────────────────────────────────────────────┐
│                                             │
│  ┌───────────┐                              │
│  │    🥈     │  Word Solver                 │
│  │           │  1000 slov                   │
│  └───────────┘                              │
│                                             │
│  ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░  847 / 1000         │
│  Ještě 153 slov!                           │
│                                             │
└─────────────────────────────────────────────┘
```

**Design:**
- Background: white
- Border: 1px solid --color-gray-200
- Icon: Grayscale 30%
- Progress bar: --color-primary-500

### Locked

```
┌─────────────────────────────────────────────┐
│                                             │
│  ┌───────────┐                              │
│  │    🔒     │  Rok v řadě                  │
│  │           │  365 dní streak              │
│  └───────────┘                              │
│                                             │
│  🔒 Zamčeno                                 │
│  🎁 Ultimate Badge + Title                  │
│                                             │
└─────────────────────────────────────────────┘
```

**Design:**
- Background: --color-gray-100
- Border: 1px dashed --color-gray-300
- Icon: Grayscale 100%, opacity 0.5
- Text: --color-gray-500

---

## Achievement Unlock Animation

### Modal

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                                                             │
│                    [Backdrop blur]                          │
│                                                             │
│         ┌─────────────────────────────────────┐             │
│         │                                     │             │
│         │              🎉                     │             │
│         │                                     │             │
│         │      ┌───────────┐                  │             │
│         │      │           │                  │             │
│         │      │    🏆     │  [Pop + glow]    │             │
│         │      │           │                  │             │
│         │      └───────────┘                  │             │
│         │                                     │             │
│         │       Achievement odemčen!          │             │
│         │                                     │             │
│         │      ═══════════════════            │             │
│         │                                     │             │
│         │         Streak Master               │             │
│         │       10 slov bez chyby             │             │
│         │                                     │             │
│         │         🎁 Odměna:                  │             │
│         │         50 XP                       │             │
│         │                                     │             │
│         │      [Skvělé! 🎊]                   │             │
│         │                                     │             │
│         └─────────────────────────────────────┘             │
│                                                             │
│              [Confetti rain animation]                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Animation sequence
```
1. Backdrop fade in (0.3s)
2. Modal scale up + bounce (0.5s, ease-elastic)
3. Badge icon pop + glow pulse (0.3s, delay 0.4s)
4. Confetti burst (delay 0.5s)
5. Text fade in stagger (delay 0.6s, 0.7s, 0.8s)
6. Button slide up (delay 1s)
```

---

## Category Tabs

```
[Všechny] [Výkonnostní] [Streak] [Obtížnostní] [Speciální]
```

### Active tab
```
Background: --color-primary-500
Color: white
Border-radius: --radius-full
Padding: --space-2 --space-4
```

### Inactive tab
```
Background: transparent
Color: --color-gray-600
Hover: --color-gray-100
```

---

## Category Icons

| Kategorie | Ikona | Barva |
|-----------|-------|-------|
| Výkonnostní | 🎯 | Green |
| Streak | 🔥 | Orange/Red |
| Obtížnostní | 🧠 | Purple |
| Speciální | 🏆 | Gold |

---

## Resource klíče

```
Achievements.Title
Achievements.Progress.Title
Achievements.Progress.Count
Achievements.Progress.Percentage
Achievements.Progress.XP
Achievements.Category.All
Achievements.Category.Performance
Achievements.Category.Streak
Achievements.Category.Difficulty
Achievements.Category.Special
Achievements.Status.Unlocked
Achievements.Status.Locked
Achievements.Status.InProgress
Achievements.Unlock.Date
Achievements.Reward.XP
Achievements.Reward.Badge
Achievements.Reward.Frame
Achievements.Reward.Theme
Achievements.Reward.Title
Achievements.Modal.Title
Achievements.Modal.Button
Achievements.Progress.Remaining
```
