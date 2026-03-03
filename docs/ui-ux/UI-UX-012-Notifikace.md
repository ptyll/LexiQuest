# UI-UX-012: Notifikace a Toasty

## Toast Notifications

### Success Toast

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│     ┌─────────────────────────────────────────────────┐    │
│     │  ✅  +50 XP získáno!                            │    │
│     │     Za dokončení denní výzvy                    │    │
│     │                                    [✕]          │    │
│     └─────────────────────────────────────────────────┘    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Design:**
- Background: --color-success-500
- Text: white
- Icon: CheckCircle, 24px
- Border-radius: --radius-lg
- Shadow: --shadow-lg
- Animation: Slide in from top

### Error Toast

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│     ┌─────────────────────────────────────────────────┐    │
│     │  ❌  Něco se pokazilo                           │    │
│     │     Zkus to prosím znovu                        │    │
│     │                                    [✕]          │    │
│     └─────────────────────────────────────────────────┘    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Design:**
- Background: --color-error-500
- Text: white
- Icon: XCircle, 24px

### Warning Toast

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│     ┌─────────────────────────────────────────────────┐    │
│     │  ⚠️  Streak končí za 3 hodiny!                  │    │
│     │     Zahraj si a udrž si oheň!                   │    │
│     │                              [🔥 Hrát hned] [✕] │    │
│     └─────────────────────────────────────────────────┘    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Design:**
- Background: --color-warning-500
- Text: --color-gray-900
- Icon: AlertTriangle, 24px

### Info Toast

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│     ┌─────────────────────────────────────────────────┐    │
│     │  ℹ️  Nová liga začala!                          │    │
│     │     Jsi v Zlaté lize                            │    │
│     │                                    [✕]          │    │
│     └─────────────────────────────────────────────────┘    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Design:**
- Background: --color-primary-500
- Text: white
- Icon: Info, 24px

---

## Toast Positioning

```
┌─────────────────────────────────────────────────────────────┐
│  [Toast] [Toast]                                           │
│  [Toast]                                                   │
│                                                             │
│                                                             │
│                          [Page Content]                     │
│                                                             │
│                                                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘

Stack: Top-right (desktop), Top-center (mobile)
Max visible: 3 toasts
Newest: Top
```

---

## Toast Animation

### Enter
```css
@keyframes toast-enter {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

/* Duration: 300ms, Ease: ease-out */
```

### Exit
```css
@keyframes toast-exit {
  from {
    transform: translateX(0);
    opacity: 1;
  }
  to {
    transform: translateX(100%);
    opacity: 0;
  }
}

/* Duration: 200ms, Ease: ease-in */
```

### Progress bar (auto-dismiss)
```
┌──────────────────────────────────────┐
│  ✅  Zpráva...              [✕]      │
│  ████████████████████████████░░░░   │  ← 5 seconds
└──────────────────────────────────────┘
```

---

## Modal Notifications

### Streak Warning Modal

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │                   ⚠️ POZOR!                         │   │
│  │                                                     │   │
│  │                🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥🔥        │   │
│  │                                                     │   │
│  │              Tvůj streak končí!                     │   │
│  │                                                     │   │
│  │         Zbývá ti už jen 3 hodiny                    │   │
│  │         na splnění dnešní výzvy!                    │   │
│  │                                                     │   │
│  │         15 dní v řadě...                           │   │
│  │         Nechceš přijít o svůj postup!               │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │        🔥 ZAHRÁT A UDRŽET STREAK             │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │         [Připomenout později]                       │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Level Up Modal

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │                   🎉 LEVEL UP!                      │   │
│  │                                                     │   │
│  │                   ┌─────────┐                       │   │
│  │                   │         │                       │   │
│  │                   │   12    │                       │   │
│  │                   │         │                       │   │
│  │                   └─────────┘                       │   │
│  │                                                     │   │
│  │              Gratulujeme!                           │   │
│  │         Dosáhl jsi levelu 12!                       │   │
│  │                                                     │   │
│  │              🎁 Odemčeno:                           │   │
│  │         • Nová cesta: Expert                       │   │
│  │         • Avatar: Fire Master                      │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │              SKVĚLÉ! 🎊                      │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│              [Confetti animation]                           │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## In-App Notification Center

```
┌─────────────────────────────────────────────────────────────┐
│  [⬅️]  Notifikace                     🔥 15  💎 1,240      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  🔔 Dnes                                            │   │
│  │  ═════════════════                                  │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  🏆  Achievement odemčen!                   │   │   │
│  │  │      Streak Master                          │   │   │
│  │  │      Před 2 hodinami               [•]      │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  🥇  Liga: Posunul ses na 3. místo!        │   │   │
│  │  │      Zlatá liga                             │   │   │
│  │  │      Před 5 hodinami               [•]      │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  📅 Včera                                           │   │
│  │  ═════════════════                                  │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  ✅  Denní výzva dokončena                  │   │   │
│  │  │      Získáno 50 XP                          │   │   │
│  │  │      Včera                         [ ]      │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  📅 Tento týden                                     │   │
│  │  ═════════════════                                  │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  🎉  Level Up!                              │   │   │
│  │  │      Dosáhl jsi levelu 11                   │   │   │
│  │  │      Před 3 dny                    [ ]      │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│         [Vymazat všechny]                                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Resource klíče

```
Notifications.Title
Notifications.Today
Notifications.Yesterday
Notifications.ThisWeek
Notifications.Older
Notifications.ClearAll
Notifications.Empty
Notifications.MarkAsRead
Notifications.Unread
Toast.Success.XP
Toast.Success.LevelUp
Toast.Success.Achievement
Toast.Error.Generic
Toast.Error.Connection
Toast.Error.Validation
Toast.Warning.StreakEnding
Toast.Warning.LowLives
Toast.Info.LeagueUpdate
Toast.Info.DailyChallenge
Modal.StreakWarning.Title
Modal.StreakWarning.Message
Modal.StreakWarning.TimeRemaining
Modal.StreakWarning.Action
Modal.LevelUp.Title
Modal.LevelUp.Message
Modal.LevelUp.Unlocked
```
