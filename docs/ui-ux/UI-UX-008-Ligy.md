# UI-UX-008: Ligy žebříček

## Layout

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  Ligy                           🔥 15  💎 1,240      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │                    🥇 ZLATÁ LIGA                    │   │
│  │                                                     │   │
│  │  ═══════════════════════════════════════════════   │   │
│  │                                                     │   │
│  │  Týden 12 / 2024                                    │   │
│  │  Končí za: 2 dny 14 hodin ⏰                        │   │
│  │                                                     │   │
│  │  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━    │   │
│  │                                                     │   │
│  │  Tvoje pozice:                                      │   │
│  │                                                     │   │
│  │        12.                                          │   │
│  │       ┌─────┐                                       │   │
│  │       │ 👤  │  Ty                                   │   │
│  │       │ 12  │  1,240 XP                             │   │
│  │       └─────┘                                       │   │
│  │                                                     │   │
│  │  Ještě 120 XP k postupu ▲                          │   │
│  │  Bezpečná vzdálenost od sestupu ▼: 340 XP          │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  🏆 Žebříček                                        │   │
│  │  ═════════════════                                  │   │
│  │                                                     │   │
│  │  Pozice  Jméno              XP          Trend      │   │
│  │  ─────────────────────────────────────────────────  │   │
│  │                                                     │   │
│  │   🥇 1.  WordMaster      2,450 XP      ▲▲▲         │   │
│  │   🥈 2.  Speedy          2,380 XP      ▲           │   │
│  │   🥉 3.  LexiKing        2,100 XP      ▲           │   │
│  │      4.  ProGamer        1,890 XP      ▲           │   │
│  │      5.  Solver          1,850 XP      ▲           │   │
│  │  ─────────────────────────────────────────────────  │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │ 👤 12.  Ty              1,240 XP   →        │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ─────────────────────────────────────────────────  │   │
│  │                                                     │   │
│  │     26.  Loser99          320 XP       ▼▼          │   │
│  │     27.  Noob             280 XP       ▼           │   │
│  │  ═════════════════════════════════════════════════  │   │
│  │     🔴 Sestupová zóna                              │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  🎁 Odměny za tuto ligu:                            │   │
│  │                                                     │   │
│  │  Postup (1-5):    🥈 Stříbrná liga + 200 XP        │   │
│  │  Zůstat (6-25):   🥇 Zlatá liga + 100 XP           │   │
│  │  Sestup (26-30):  🥉 Bronzová liga                 │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  📜 Historii lig                                    │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Komponenty

### 1. League Header

```
┌─────────────────────────────────────────────┐
│                                             │
│               🥇 ZLATÁ LIGA                 │
│                                             │
│   ═══════════════════════════════════════   │
│                                             │
│   Týden 12 / 2024                           │
│   Končí za: 2 dny 14 hodin ⏰               │
│                                             │
└─────────────────────────────────────────────┘
```

#### Liga ikony a barvy
```
Bronzová:   🥉  #CD7F32
Stříbrná:   🥈  #C0C0C0
Zlatá:      🥇  #FFD700  (Current)
Diamantová: 💎  #B9F2FF
Legenda:    👑  #FF6B9D
```

#### Odpočet času
```
> 24h:  Standardní zobrazení
< 24h:  Oranžová barva, pulsing
< 6h:   Červená barva, rapid pulsing
< 1h:   Červená + sekundy, shake warning
```

---

### 2. User Position Card

```
┌─────────────────────────────────────────────┐
│                                             │
│              Tvoje pozice:                  │
│                                             │
│                 12.                         │
│              ┌─────┐                        │
│              │ 👤  │   Ty                   │
│              │ 12  │   1,240 XP             │
│              └─────┘                        │
│                                             │
│   Ještě 120 XP k postupu ▲                 │
│   [Progress bar to promotion]              │
│                                             │
│   Bezpečná vzdálenost: 340 XP ▼            │
│   [Progress bar to safety]                 │
│                                             │
└─────────────────────────────────────────────┘
```

#### Progress bars
```
K postupu:
██████████████████░░░░░░░░  71%  (120 XP needed)
Barva: --color-success-500

K bezpečí:
██████████████████████████  100%  (Safe!)
Barva: --color-primary-500
```

---

### 3. Leaderboard

```
Pozice  Jméno              XP          Trend      Akce
────────────────────────────────────────────────────────

🥇 1.   WordMaster      2,450 XP      ▲▲▲        [Profil]
       [Avatar] 👤
       
🥈 2.   Speedy          2,380 XP      ▲          [Profil]
       [Avatar] 👤
       
🥉 3.   LexiKing        2,100 XP      ▲          [Profil]
       [Avatar] 👤

────────────────────────────────────────────────────────

┌─────────────────────────────────────────────────────┐
│ 👤 12.  Ty              1,240 XP   →               │
│     [Avatar]  Právě teď aktualizováno!             │
└─────────────────────────────────────────────────────┘

────────────────────────────────────────────────────────

26.    Loser99          320 XP       ▼▼          [Profil]
27.    Noob             280 XP       ▼           [Profil]
════════════════════════════════════════════════════════
🔴 Sestupová zóna
```

#### Row design
- **Height:** 64px
- **Hover:** --color-gray-100 background
- **Current user:** --color-primary-50 background, 2px border

#### Trend icons
```
▲▲▲  Rychle stoupá  (> 100 XP za hodinu)
▲    Stoupá          (aktivní)
→    Stagnuje        (neaktivní > 1h)
▼    Klesá           (již předstižen)
```

---

### 4. Promotion/Demotion Zones

```
┌─────────────────────────────────────────────┐
│                                             │
│   ZÓNY:                                     │
│                                             │
│   ┌─────────────┐                           │
│   │  1-5 ▲      │  🥈 Stříbrná liga         │
│   │  Postup     │  + 200 XP                 │
│   └─────────────┘                           │
│                                             │
│   ┌─────────────────────────────────────┐   │
│   │  6-25 ─────────────────────────     │   │
│   │  Zůstat v Zlaté lize                │   │
│   │  + 100 XP                           │   │
│   └─────────────────────────────────────┘   │
│          ↑                                  │
│          Jsi zde (12.)                      │
│                                             │
│   ┌─────────────┐                           │
│   │  26-30 ▼    │  🥉 Bronzová liga         │
│   │  Sestup     │                           │
│   └─────────────┘                           │
│                                             │
└─────────────────────────────────────────────┘
```

---

### 5. League History

```
┌─────────────────────────────────────────────────────┐
│                                                     │
│  📜 Historie lig                                    │
│  ═════════════════                                  │
│                                                     │
│  Týden 11:  🥈 Stříbrná liga  (3. místo)           │
│  Týden 10:  🥈 Stříbrná liga  (8. místo)           │
│  Týden 9:   🥉 Bronzová liga  (1. místo ▲)         │
│  Týden 8:   🥉 Bronzová liga  (5. místo)           │
│                                                     │
│  [Zobrazit všechny →]                               │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## Animace

### Real-time updates
```
Když se změní pořadí:
1. Highlight změněné řádky (yellow flash)
2. Slide animation na novou pozici
3. Update čísla s count animation
```

### Promotion warning
```
Když je uživatel blízko sestupu (< 3 místa):
- Border pulsing red
- Warning toast: "Pozor! Hrozí ti sestup!"
- Motivační CTA: "Zahraj si a zachraň pozici!"
```

### League end countdown
```
Poslední hodina:
- Odpočet sekund
- Intenzivnější barva
- Push notifikace každých 15 minut
```

---

## Responzivita

### Mobile
- Leaderboard: Horizontální scroll
- User card: Full width
- Zóny: Stack vertically

---

## Resource klíče

```
League.Title
League.Current.Week
League.Time.Remaining
League.Time.Remaining.Days
League.Time.Remaining.Hours
League.Time.Remaining.Minutes
League.User.Position
League.User.XP
League.User.ToPromotion
League.User.SafetyMargin
League.Leaderboard.Title
League.Leaderboard.Position
League.Leaderboard.Name
League.Leaderboard.XP
League.Leaderboard.Trend
League.Zone.Promotion
League.Zone.Stay
League.Zone.Demotion
League.Rewards.Title
League.Rewards.Promotion
League.Rewards.Stay
League.Rewards.Demotion
League.History.Title
League.History.ViewAll
League.Warning.PromotionClose
League.Warning.DemotionRisk
```
