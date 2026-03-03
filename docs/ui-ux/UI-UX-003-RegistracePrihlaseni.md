# UI-UX-003: Registrace a Přihlášení

## Registrace

### Layout

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  [Zpět]                                                     │
│                                                             │
│                    🔥 LOGO 🔥                               │
│                                                             │
│              Vytvoř si účet                                 │
│         Začni svou cestu k mistrovství                      │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │  📧 Email                                           │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │ tvuj@email.cz                               │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │  ⚠️ Tento email je již registrován                │   │
│  │                                                     │   │
│  │  👤 Uživatelské jméno                             │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │ tvojejmeno                                  │   │   │
│  │  └─────────────────────────────────────────┘───┘   │   │
│  │  ✓ Dostupné                                       │   │
│  │                                                     │   │
│  │  🔒 Heslo                                         │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │ ••••••••                          [👁️]     │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │  [▓▓▓▓░░░░░░░░░░] Střední                        │   │
│  │                                                     │   │
│  │  🔒 Potvrď heslo                                  │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │ ••••••••                                    │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ☑️ Souhlasím s [podmínkami] a [ochranou osob.údajů]│  │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │         🚀 VYTVOŘIT ÚČET                    │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ─────────── nebo ───────────                     │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  [G] Registrovat přes Google                │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│         Už máš účet? [Přihlásit se]                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Design Specs

#### Form Card
- Background: White
- Border-radius: --radius-3xl (24px)
- Padding: --space-8 (32px)
- Shadow: --shadow-xl
- Max-width: 420px
- Margin: auto (centered)

#### Input Fields
- Height: 56px
- Border: 2px solid --color-gray-300
- Border-radius: --radius-xl (12px)
- Padding: 0 --space-4
- Font-size: --text-base
- Transition: border-color --duration-fast

#### Input States
```
Default: border-color: --color-gray-300
Focus:   border-color: --color-primary-500, shadow: 0 0 0 3px rgba(255,152,0,0.2)
Valid:   border-color: --color-success-500
Error:   border-color: --color-error-500, background: --color-error-50
```

#### Password Strength Indicator
```
Weak:    [▓░░░░░░░░░] Červená
Medium:  [▓▓▓▓░░░░░░] Oranžová
Strong:  [▓▓▓▓▓▓▓▓▓▓] Zelená
```

#### Validace v reálném čase
- Email: Validace formátu při blur
- Username: Kontrola dostupnosti po 500ms debounce
- Heslo: Síla se počítá při každém keystroke

### Animace

#### Page Load
```
Logo:     scale(0.8) opacity(0) → scale(1) opacity(1), 400ms
Form:     translateY(20px) opacity(0) → translateY(0) opacity(1), 500ms, delay 100ms
Button:   scale(0.95) → scale(1), 300ms, delay 300ms
```

#### Error Shake
```css
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  20%, 60% { transform: translateX(-10px); }
  40%, 80% { transform: translateX(10px); }
}
/* Apply when validation fails */
```

#### Success
```
Button: background flash green
Spinner: inline loading indicator
Redirect: fade out page
```

---

## Přihlášení

### Layout

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  [Zpět]                                                     │
│                                                             │
│                    🔥 LOGO 🔥                               │
│                                                             │
│              Vítej zpět!                                    │
│         Pokračuj ve svém streaku                            │
│                                                             │
│                    🔥 15 dní 🔥                              │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │  📧 Email                                           │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │ tvuj@email.cz                               │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  🔒 Heslo                                           │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │ ••••••••                          [👁️]     │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ☑️ Zapamatovat si mě                               │   │
│  │                                                     │   │
│  │  [Zapomněl jsi heslo?]                    →       │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │         🔐 PŘIHLÁSIT SE                       │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ─────────── nebo ───────────                     │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  [G] Přihlásit přes Google                  │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│         Nemáš účet? [Zaregistruj se]                       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Design Specs

#### Streak Indicator (pokud má uživatel aktivní streak)
```
Background: linear-gradient(135deg, #FF6B35, #F7931E)
Border-radius: --radius-full
Padding: --space-2 --space-4
Color: white
Font-weight: --font-bold
Icon: Flame (animated pulse)
```

#### Zapamatovat si mě
```
Checkbox custom styled:
- Size: 20px
- Border-radius: --radius-md
- Checked: background --color-primary-500, checkmark white
- Transition: all --duration-fast
```

#### Error State
```
Toast notification:
- Position: top-center
- Background: --color-error-500
- Color: white
- Icon: AlertCircle
- Auto-dismiss: 5s
- Slide in from top
```

### Loading States

#### Button Loading
```
Content: [Spinner] Přihlašuji...
Spinner: Rotating circle, 20px, white
Disabled: opacity 0.7, no pointer events
```

#### Skeleton Screen (alternativa)
```
Pokud načítáme data po přihlášení:
- Skeleton placeholders pro dashboard
- Shimmer animation
- Postupně se nahrazuje reálnými daty
```

---

## Obnova hesla

### Layout (3 kroky)

#### Krok 1: Zadání emailu
```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  [Zpět]                                                     │
│                                                             │
│                    🔥 LOGO 🔥                               │
│                                                             │
│              Obnova hesla                                   │
│         Zadej email pro zaslání odkazu                      │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │  📧 Email                                           │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │ tvuj@email.cz                               │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │     ODESLAT ODKAZ NA OBNOVU                 │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│         [Zpět na přihlášení]                               │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### Krok 2: Potvrzení odeslání
```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                    📧                                       │
│                                                             │
│              Zkontroluj svůj email                          │
│                                                             │
│  Odeslali jsme ti odkaz na obnovu hesla na:                │
│  t***@email.cz                                              │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  [📧 Otevřít emailovou aplikaci]                    │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ─────────────────────────────────────────                  │
│                                                             │
│  Email nedorazil? [Odeslat znovu] (za 59s)                 │
│                                                             │
│  [Zkusit jiný email]                                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### Krok 3: Nové heslo
```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                    🔒                                       │
│                                                             │
│              Nastav nové heslo                              │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                                                     │   │
│  │  🔒 Nové heslo                                      │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │ ••••••••                                    │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  🔒 Potvrď nové heslo                               │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │ ••••••••                                    │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │     ULOŽIT NOVÉ HESLO                       │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Animace

#### Success Checkmark
```
Circle with checkmark:
- Scale from 0 to 1 with bounce easing
- Checkmark draws in (stroke-dashoffset animation)
- Confetti burst optional
```

#### Countdown timer
```
"Odeslat znovu (za 59s)"
- Postupné odpočítávání
- Po 0s enable tlačítka
- Fade transition
```

---

## Responzivita

### Desktop (> 1024px)
- Form card: 420px max-width
- Centered on page
- Full background with subtle pattern

### Tablet (768px - 1024px)
- Form card: 90% width
- Padding reduced

### Mobile (< 768px)
- Full width, no card border-radius on sides
- Stacked layout
- Larger touch targets (min 48px)

## Resource klíče

```
Auth.Register.Title
Auth.Register.Subtitle
Auth.Register.Email.Label
Auth.Register.Email.Placeholder
Auth.Register.Username.Label
Auth.Register.Username.Placeholder
Auth.Register.Password.Label
Auth.Register.Password.Placeholder
Auth.Register.ConfirmPassword.Label
Auth.Register.Terms.Label
Auth.Register.Terms.Link
Auth.Register.Button.Submit
Auth.Register.Button.Google
Auth.Register.Link.Login
Auth.Register.Link.Login.Text

Auth.Login.Title
Auth.Login.Subtitle
Auth.Login.Streak.Badge
Auth.Login.Email.Label
Auth.Login.Password.Label
Auth.Login.RememberMe.Label
Auth.Login.ForgotPassword.Link
Auth.Login.Button.Submit
Auth.Login.Button.Google
Auth.Login.Link.Register

Auth.ForgotPassword.Title
Auth.ForgotPassword.Description
Auth.ForgotPassword.Email.Label
Auth.ForgotPassword.Button.Submit
Auth.ForgotPassword.Success.Title
Auth.ForgotPassword.Success.Description
Auth.ForgotPassword.Success.OpenEmail
Auth.ForgotPassword.Resend.Countdown
Auth.ForgotPassword.Resend.Button

Auth.ResetPassword.Title
Auth.ResetPassword.NewPassword.Label
Auth.ResetPassword.ConfirmPassword.Label
Auth.ResetPassword.Button.Submit
Auth.ResetPassword.Success.Title
Auth.ResetPassword.Success.Description

Auth.Validation.Email.Invalid
Auth.Validation.Email.Required
Auth.Validation.Email.Exists
Auth.Validation.Username.Invalid
Auth.Validation.Username.Taken
Auth.Validation.Password.Weak
Auth.Validation.Password.Mismatch
Auth.Validation.Terms.Required
```

## Accessibility

- [ ] Focus trap v modalech
- [ ] ARIA labels pro všechny inputy
- [ ] Error announcements pro screen readers
- [ ] Keyboard navigation (Tab order)
- [ ] Password visibility toggle s aria-pressed
- [ ] Autocomplete atributy pro hesla
