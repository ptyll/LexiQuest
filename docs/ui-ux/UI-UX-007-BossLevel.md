# UI-UX-007: Boss Level obrazovky

## Maraton Boss

### Layout

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  BOSS MARATON                    🔥 15  💎 1,240     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │              🔥 BOSS MARATON 🔥                     │   │
│  │                                                     │   │
│  │  Postup: ████████████████░░░░  16 / 20              │   │
│  │                                                     │   │
│  │  Životy:  ❤️ ❤️ 🖤                                   │   │
│  │                                                     │   │
│  │  ⚠️ Životy se neobnovují mezi koly!                 │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│              16 / 20 slov                                   │
│                                                             │
│     ⏱️ 12s              Combo: 🔥 x6                       │
│                                                             │
│              ┌───────────────────────┐                      │
│              │                       │                      │
│              │    K O N S T I T U C E│                      │
│              │                       │                      │
│              └───────────────────────┘                      │
│                                                             │
│              ┌───────────────────────┐                      │
│              │                       │                      │
│              │   [______________]   │                      │
│              │                       │                      │
│              └───────────────────────┘                      │
│                                                             │
│              ┌───────────────────────┐                      │
│              │    POTVRDIT  ⏎       │                      │
│              └───────────────────────┘                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Header Boss Info

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  🔥 BOSS MARATON                                    │   │
│  │  ═════════════════                                  │   │
│  │                                                     │   │
│  │  Postup:                                            │   │
│  │  ████████████████░░░░  16/20                        │   │
│  │                                                     │   │
│  │  ❤️ Životy:  2 / 3                                 │   │
│  │                                                     │   │
│  │  ⚠️ Neobnovují se!                                 │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### Design
- **Background:** Gradient (dark red → orange)
- **Border:** 2px solid gold
- **Progress bar:** Red gradient with flame icons
- **Warning:** Pulsing red text

---

## Podmínka Boss (Zakázané písmeno)

### Layout

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  BOSS PODMÍNKA                   🔥 15  💎 1,240     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │              🚫 BOSS PODMÍNKA                       │   │
│  │                                                     │   │
│  │  Postup: ████████████░░░░░░░░  12 / 15              │   │
│  │                                                     │   │
│  │  ❤️ Životy:  ❤️ ❤️ ❤️ ❤️ ❤️                        │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│              12 / 15 slov                                   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │         ⚠️ ZAKÁZANÉ PÍSMENO:  "B"                  │   │
│  │                                                     │   │
│  │    Odpověď nesmí obsahovat písmeno B!              │   │
│  │                                                     │   │
│  │    Trest: -5 XP za porušení                        │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│     ⏱️ 20s              Combo: 🔥 x3                       │
│                                                             │
│              ┌───────────────────────┐                      │
│              │                       │                      │
│              │    P R O G R A M      │                      │
│              │                       │                      │
│              └───────────────────────┘                      │
│                                                             │
│              ┌───────────────────────┐                      │
│              │                       │                      │
│              │   [______________]   │                      │
│              │                       │                      │
│              └───────────────────────┘                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Zakázané písmeno varování

```
┌─────────────────────────────────────────────┐
│                                             │
│         🚫 ZAKÁZANÉ PÍSMENO: "R"           │
│                                             │
│    Odpověď nesmí obsahovat písmeno R!      │
│                                             │
│    ┌─────────────────────────────────┐     │
│    │  TREST: -5 XP za porušení       │     │
│    └─────────────────────────────────┘     │
│                                             │
└─────────────────────────────────────────────┘
```

#### Design
- **Background:** Warning gradient (yellow → orange)
- **Forbidden letter:** 48px, red, crossed out
- **Border:** 2px dashed red
- **Animation:** Periodic pulse/shake

### Chyba - porušení podmínky

```
┌─────────────────────────────────────────────┐
│                                             │
│           ⚠️ POZOR!                         │
│                                             │
│  Tvá odpověď obsahuje zakázané písmeno "B" │
│                                             │
│  Správná odpověď byla: KAPR                │
│                                             │
│  📉 -5 XP penalizace                        │
│  💔 -1 život                                │
│                                             │
│  [➡️ Pokračovat]                            │
│                                             │
└─────────────────────────────────────────────┘
```

---

## Twist Boss (Odkrývání)

### Layout

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  BOSS TWIST                      🔥 15  💎 1,240     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │              👁️ BOSS TWIST                         │   │
│  │                                                     │   │
│  │  Postup: ██████████░░░░░░░░░░░░  10 / 16            │   │
│  │                                                     │   │
│  │  ❤️ Životy:  ❤️ ❤️ ❤️                              │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│              10 / 16 slov                                   │
│                                                             │
│     ⏱️ Celkový čas: 2:34                                  │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │         Vidíš jen část slova:                       │   │
│  │                                                     │   │
│  │    ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐                │   │
│  │    │  P  │ │  Ř  │ │  ?  │ │  ?  │                │   │
│  │    └─────┘ └─────┘ └─────┘ └─────┘                │   │
│  │       ↑      ↑      ↑      ↑                      │   │
│  │    odkryto  odkryto  skryto  skryto               │   │
│  │                                                     │   │
│  │    Další písmeno za: 2s ⏱️                         │   │
│  │    ━━━━━━━━━━░░░░░░  60%                          │   │
│  │                                                     │   │
│  │    💡 Tip: Za brzký tip získáš bonus XP!          │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│              ┌───────────────────────┐                      │
│              │                       │                      │
│              │   [______________]   │                      │
│              │                       │                      │
│              └───────────────────────┘                      │
│                                                             │
│              ┌───────────────────────┐                      │
│              │  💡 TIPNOUT HNED!     │                      │
│              │    (+10 XP bonus)     │                      │
│              └───────────────────────┘                      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Odkrývání animace

```
Step 1: P Ř ? ?
Step 2: P Ř O ?
Step 3: P Ř O G
```

#### Letter reveal animation
```css
@keyframes letter-reveal {
  0% {
    transform: rotateY(90deg);
    background: var(--color-gray-300);
  }
  100% {
    transform: rotateY(0deg);
    background: white;
  }
}

.letter-hidden {
  background: var(--color-gray-400);
  color: transparent;
}

.letter-revealed {
  animation: letter-reveal 0.5s ease-out;
}
```

### Bonusy za rychlost

```
┌─────────────────────────────────────────────┐
│                                             │
│     🎉 VÝBORNĚ! Tipnul jsi brzy!           │
│                                             │
│     Odkrytá písmena: 2 / 5                 │
│     Bonus: +10 XP                          │
│                                             │
│     Celkem získáno: 20 XP                  │
│                                             │
└─────────────────────────────────────────────┘
```

---

## Boss Victory Screen

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                                                             │
│                         🎉                                  │
│                   BOSS PORAŽEN!                            │
│                                                             │
│                      🏆                                     │
│                                                             │
│              ┌───────────────────────┐                      │
│              │    👹 MARATON 👹      │                      │
│              └───────────────────────┘                      │
│                                                             │
│              [Confetti animation]                           │
│                                                             │
│              ┌───────────────────────────────────────┐     │
│              │  Výsledky:                            │     │
│              │                                       │     │
│              │  ⏱️ Čas: 4:32                         │     │
│              │  🎯 Správně: 20/20                    │     │
│              │  ❤️ Zbývající životy: 1               │     │
│              │  🔥 Nejvyšší combo: 8                 │     │
│              │                                       │     │
│              │  ════════════════════════             │     │
│              │                                       │     │
│              │  ⭐ Celkem XP: 350                    │     │
│              │  🏅 Achievement: Marathon Master       │     │
│              │                                       │     │
│              │  🌟 PERFECT RUN! (bez ztráty života)  │     │
│              │                                       │     │
│              └───────────────────────────────────────┘     │
│                                                             │
│              [🎊 Oslavit]  [➡️ Pokračovat]                  │
│                                                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Boss Defeat Screen

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                                                             │
│                         💔                                  │
│                   BOSS VYHRÁL...                           │
│                                                             │
│                      👹                                     │
│                                                             │
│              [Dark, stormy animation]                       │
│                                                             │
│              ┌───────────────────────────────────────┐     │
│              │  Výsledky:                            │     │
│              │                                       │     │
│              │  🎯 Splněno: 14 / 20                  │     │
│              │  ⏱️ Čas do prohry: 3:45               │     │
│              │  🔥 Nejvyšší combo: 5                 │     │
│              │                                       │     │
│              │  ════════════════════════             │     │
│              │                                       │     │
│              │  ⭐ Získáno XP: 180                   │     │
│              │                                       │     │
│              │  💪 Důležité je se nevzdávat!         │     │
│              │                                       │     │
│              └───────────────────────────────────────┘     │
│                                                             │
│         [🔄 Zkusit znovu]  [📚 Zpět na cestu]              │
│                                                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Resource klíče

```
Boss.Marathon.Title
Boss.Marathon.Subtitle
Boss.Marathon.Progress
Boss.Marathon.Lives
Boss.Marathon.LivesWarning
Boss.Condition.Title
Boss.Condition.Subtitle
Boss.Condition.ForbiddenLetter
Boss.Condition.Warning
Boss.Condition.Penalty
Boss.Condition.Violation.Title
Boss.Condition.Violation.Message
Boss.Twist.Title
Boss.Twist.Subtitle
Boss.Twist.RevealedLetters
Boss.Twist.HiddenLetters
Boss.Twist.NextReveal
Boss.Twist.GuessEarly
Boss.Twist.Bonus.Title
Boss.Victory.Title
Boss.Victory.Stats.Time
Boss.Victory.Stats.Correct
Boss.Victory.Stats.Lives
Boss.Victory.Stats.Combo
Boss.Victory.XP
Boss.Victory.Achievement
Boss.Victory.PerfectRun
Boss.Defeat.Title
Boss.Defeat.Stats.Solved
Boss.Defeat.Stats.Time
Boss.Defeat.Stats.Combo
Boss.Defeat.XP
Boss.Defeat.Encouragement
Boss.Defeat.Retry
Boss.Defeat.Back
```
