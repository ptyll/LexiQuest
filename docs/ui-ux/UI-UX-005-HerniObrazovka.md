# UI-UX-005: Herní obrazovka

## Layout

### Standardní herní režim

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  Cesta 2 - Level 18           🔥 15  💎 1,240        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│                                                             │
│              ┌───────────────────────┐                      │
│              │    🔥 x3 COMBO!       │                      │
│              └───────────────────────┘                      │
│                                                             │
│              2 / 10 slov                                    │
│                                                             │
│     ⏱️ 18s              ❤️❤️❤️🖤🖤              💡 2         │
│                                                             │
│              ┌───────────────────────┐                      │
│              │                       │                      │
│              │                       │                      │
│              │    P Ř E S M Y Č K A  │                      │
│              │                       │                      │
│              │                       │                      │
│              └───────────────────────┘                      │
│                                                             │
│         [?] Nápověda (-5 XP)                                │
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
│              [Přeskočit (-1 život)]                         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Komponenty

### 1. Header

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  Cesta 2 - Level 18           🔥 15  💎 1,240        │
└─────────────────────────────────────────────────────────────┘
```

#### Design Specs
- **Height:** 64px
- **Background:** White
- **Border-bottom:** 1px solid --color-gray-200
- **Back button:** 48px touch target
- **Stats:** Compact icons with numbers

---

### 2. Timer

```
⏱️ 18s
```

#### Varianty
```
Více než 10s:  ⏱️ 18s    (green)
5-10s:         ⏱️ 7s     (orange) + pulsing
Méně než 5s:   ⏱️ 3s     (red) + rapid pulse + shake
```

#### Animace low time
```css
@keyframes timer-urgency {
  0%, 100% { 
    transform: scale(1); 
    color: var(--color-error-500);
  }
  50% { 
    transform: scale(1.1); 
    color: var(--color-error-700);
  }
}

.timer-danger {
  animation: timer-urgency 0.5s ease-in-out infinite;
}
```

---

### 3. Životy (Lives)

```
❤️❤️❤️🖤🖤
```

#### Design
- **Heart full:** Red gradient, pulsing slightly
- **Heart empty:** Gray outline
- **Size:** 28px each
- **Gap:** 4px

#### Animace ztráty života
```
1. Heart scales up rapidly
2. Heart breaks animation (crack)
3. Heart turns gray with shake
4. Particles falling
```

---

### 4. Zamíchané slovo

```
┌───────────────────────────────────┐
│                                   │
│      P   Ř   E   S   M   Y   Č   K   A  │
│                                   │
└───────────────────────────────────┘
```

#### Letter styling
```css
.scrambled-letter {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 64px;
  font-size: 32px;
  font-weight: 700;
  background: white;
  border: 2px solid var(--color-gray-300);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  margin: 0 4px;
}
```

#### Shuffle Animation (na začátku kola)
```
1. Všechna písmena jsou na sobě
2. Rozletí se na pozice
3. Každé písmeno rotuje během letu
4. Přistání s bounce efektem
5. Stabilizace
```

```css
@keyframes letter-deal {
  0% {
    transform: translate(0, -100px) rotate(0deg) scale(0.5);
    opacity: 0;
  }
  60% {
    transform: translate(var(--tx), var(--ty)) rotate(360deg) scale(1.1);
    opacity: 1;
  }
  80% {
    transform: translate(var(--tx), var(--ty)) rotate(360deg) scale(0.95);
  }
  100% {
    transform: translate(var(--tx), var(--ty)) rotate(360deg) scale(1);
  }
}
```

---

### 5. Answer Input

```
┌─────────────────────────────────────────┐
│                                         │
│   ┌─────────────────────────────────┐   │
│   │ S O L V E                       │   │
│   └─────────────────────────────────┘   │
│                                         │
└─────────────────────────────────────────┘
```

#### Design
- **Background:** --color-gray-100
- **Border:** 2px solid transparent (focus: --color-primary-500)
- **Border-radius:** --radius-2xl
- **Height:** 72px
- **Font:** 40px, --font-bold, uppercase, letter-spacing 8px
- **Text-align:** Center

#### Focus state
```
- Border: --color-primary-500
- Shadow: 0 0 0 4px rgba(255, 152, 0, 0.2)
- Background: white
```

---

### 6. Submit Button

```
┌─────────────────────────────────────────┐
│           POTVRDIT  ⏎                  │
└─────────────────────────────────────────┘
```

#### States
```
Default:   Primary gradient, white text
Hover:     Scale 1.02, brighter gradient
Active:    Scale 0.98
Disabled:  Gray, no interaction
Loading:   Spinner inside
```

---

### 7. Nápověda (Hint)

```
┌─────────────────────────────────────────┐
│     💡 Nápověda (-5 XP)                 │
└─────────────────────────────────────────┘
```

#### Po kliknutí
```
1. Potvrzovací dialog:
   "Opravdu chceš použít nápovědu?
    Bude tě to stát 5 XP"
    
2. Po potvrzení:
   - XP se odečte s animací
   - Odhalí se jedno písmeno na správné pozici
   - Animace odhalení (flip)
```

#### Nápověda nedostupná
```
Grayed out with tooltip:
"Už jsi použil všechny nápovědy"
```

---

### 8. Feedback (správná/špatná odpověď)

#### Správná odpověď ✅
```
┌─────────────────────────────────────────┐
│  ✓ SPRÁVNĚ!                             │
│  +15 XP                                 │
│  +5 XP rychlost!                        │
│  🔥 Combo x4!                           │
└─────────────────────────────────────────┘
```

#### Animace
```css
/* Correct feedback */
@keyframes correct-bounce {
  0%, 100% { transform: scale(1); }
  25% { transform: scale(1.1) translateY(-10px); }
  50% { transform: scale(1.05) translateY(-5px); }
  75% { transform: scale(1.02) translateY(-2px); }
}

/* Background flash */
@keyframes success-flash {
  0% { background-color: transparent; }
  50% { background-color: rgba(76, 175, 80, 0.3); }
  100% { background-color: transparent; }
}

/* XP numbers fly up */
@keyframes xp-float {
  0% { 
    transform: translateY(0) scale(1); 
    opacity: 1;
  }
  100% { 
    transform: translateY(-50px) scale(1.2); 
    opacity: 0;
  }
}
```

#### Špatná odpověď ❌
```
┌─────────────────────────────────────────┐
│  ✗ ŠPATNĚ!                              │
│  Správná odpověď: SLOVO                 │
│  -1 život                               │
└─────────────────────────────────────────┘
```

#### Animace
```css
/* Shake effect */
@keyframes wrong-shake {
  0%, 100% { transform: translateX(0); }
  20%, 60% { transform: translateX(-15px); }
  40%, 80% { transform: translateX(15px); }
}

/* Red flash */
@keyframes error-flash {
  0%, 100% { background-color: transparent; }
  50% { background-color: rgba(244, 67, 54, 0.3); }
}
```

---

## Level Complete Modal

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                         🎉                                  │
│                   LEVEL DOKONČEN!                          │
│                                                             │
│                     Level 18                                │
│                   ████████████ 100%                        │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Získáno:                                           │   │
│  │                                                     │   │
│  │  ⭐ 150 XP                                          │   │
│  │  🔥 Streak pokračuje (16 dní)                       │   │
│  │  🏅 Achievement: Level Master                       │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  [🏠 Dashboard]  [➡️ Další level]                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### Confetti Animation
```css
@keyframes confetti-fall {
  0% {
    transform: translateY(-100vh) rotate(0deg);
    opacity: 1;
  }
  100% {
    transform: translateY(100vh) rotate(720deg);
    opacity: 0;
  }
}
```

---

## Game Over Modal

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                       💔                                    │
│                    KONEC HRY                               │
│                                                             │
│                  Došly životy!                             │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Dosáhl jsi:                                        │   │
│  │                                                     │   │
│  │  🎯 Slova: 8 / 10                                   │   │
│  │  ⭐ XP: 120                                         │   │
│  │  🔥 Streak zachráněn! (Shield použit)               │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  [🔄 Zkusit znovu]  [🛒 Koupit životy]  [🏠 Dashboard]     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Responzivita

### Mobile (< 768px)
- Letters: 36px (místo 48px)
- Input: 56px height
- Letters stack pokud slovo příliš dlouhé
- Hint button: Full width pod inputem

### Tablet (768px - 1024px)
- Standard layout
- Větší touch targets

---

## Resource klíče

```
Game.Header.Back
Game.Header.Path
Game.Header.Level
Game.Timer.Label
Game.Timer.Seconds
Game.Lives.Label
Game.Hint.Button
Game.Hint.Cost
Game.Hint.Confirm.Title
Game.Hint.Confirm.Message
Game.Hint.NotAvailable
Game.Word.Placeholder
Game.Answer.Placeholder
Game.Answer.Submit
Game.Answer.Skip
Game.Answer.Skip.Cost
Game.Feedback.Correct
Game.Feedback.Wrong
Game.Feedback.XP
Game.Feedback.SpeedBonus
Game.Feedback.Combo
Game.Feedback.CorrectAnswer
Game.LevelComplete.Title
Game.LevelComplete.XP
Game.LevelComplete.Streak
Game.LevelComplete.Achievement
Game.LevelComplete.NextLevel
Game.LevelComplete.Dashboard
Game.GameOver.Title
Game.GameOver.NoLives
Game.GameOver.WordsSolved
Game.GameOver.Retry
Game.GameOver.BuyLives
Game.Combo.Multiplier
```
