# UI-UX-001: Design System a Tokens

## Filozofie designu

**LexiQuest Design Principles:**
1. **Playful** - Hra má být zábavná, barvy jsou živé
2. **Clear** - Okamžitá zpětná vazba, žádné nejasnosti
3. **Motivating** - Progress je vždy viditelný
4. **Accessible** - WCAG 2.1 AA kompatibilní

## Color Palette

### Primary Colors
```css
--color-primary-50:  #FFF3E0;
--color-primary-100: #FFE0B2;
--color-primary-200: #FFCC80;
--color-primary-300: #FFB74D;
--color-primary-400: #FFA726;
--color-primary-500: #FF9800;  /* Main Orange */
--color-primary-600: #FB8C00;
--color-primary-700: #F57C00;
--color-primary-800: #EF6C00;
--color-primary-900: #E65100;
```

### Secondary Colors (Green - Success)
```css
--color-success-50:  #E8F5E9;
--color-success-100: #C8E6C9;
--color-success-500: #4CAF50;
--color-success-700: #388E3C;
```

### Error Colors (Red)
```css
--color-error-50:  #FFEBEE;
--color-error-100: #FFCDD2;
--color-error-500: #F44336;
--color-error-700: #D32F2F;
```

### Warning Colors (Amber)
```css
--color-warning-50:  #FFF8E1;
--color-warning-100: #FFECB3;
--color-warning-500: #FFC107;
--color-warning-700: #FFA000;
```

### Neutral Colors
```css
--color-gray-50:  #FAFAFA;
--color-gray-100: #F5F5F5;
--color-gray-200: #EEEEEE;
--color-gray-300: #E0E0E0;
--color-gray-400: #BDBDBD;
--color-gray-500: #9E9E9E;
--color-gray-600: #757575;
--color-gray-700: #616161;
--color-gray-800: #424242;
--color-gray-900: #212121;
```

### League Colors
```css
--color-league-bronze: #CD7F32;
--color-league-silver: #C0C0C0;
--color-league-gold: #FFD700;
--color-league-diamond: #B9F2FF;
--color-league-legend: #FF6B9D;
```

### Fire Gradient (Streak)
```css
--color-fire-1: #FF6B35;
--color-fire-2: #F7931E;
--color-fire-3: #FFD23F;
--gradient-fire: linear-gradient(135deg, #FF6B35 0%, #F7931E 50%, #FFD23F 100%);
```

## Typography

### Font Family
```css
--font-primary: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
--font-display: 'Poppins', sans-serif;  /* Nadpisy */
--font-mono: 'JetBrains Mono', monospace; /* Čísla, kódy */
```

### Type Scale
```css
--text-xs:   0.75rem;   /* 12px */
--text-sm:   0.875rem;  /* 14px */
--text-base: 1rem;      /* 16px */
--text-lg:   1.125rem;  /* 18px */
--text-xl:   1.25rem;   /* 20px */
--text-2xl:  1.5rem;    /* 24px */
--text-3xl:  1.875rem;  /* 30px */
--text-4xl:  2.25rem;   /* 36px */
--text-5xl:  3rem;      /* 48px */
--text-6xl:  3.75rem;   /* 60px */
```

### Font Weights
```css
--font-normal:   400;
--font-medium:   500;
--font-semibold: 600;
--font-bold:     700;
--font-extrabold: 800;
```

## Spacing System

```css
--space-0:  0;
--space-1:  0.25rem;   /* 4px */
--space-2:  0.5rem;    /* 8px */
--space-3:  0.75rem;   /* 12px */
--space-4:  1rem;      /* 16px */
--space-5:  1.25rem;   /* 20px */
--space-6:  1.5rem;    /* 24px */
--space-8:  2rem;      /* 32px */
--space-10: 2.5rem;    /* 40px */
--space-12: 3rem;      /* 48px */
--space-16: 4rem;      /* 64px */
--space-20: 5rem;      /* 80px */
--space-24: 6rem;      /* 96px */
```

## Border Radius

```css
--radius-none: 0;
--radius-sm:   0.125rem;  /* 2px */
--radius-md:   0.375rem;  /* 6px */
--radius-lg:   0.5rem;    /* 8px */
--radius-xl:   0.75rem;   /* 12px */
--radius-2xl:  1rem;      /* 16px */
--radius-3xl:  1.5rem;    /* 24px */
--radius-full: 9999px;
```

## Shadows

```css
--shadow-sm:   0 1px 2px 0 rgba(0, 0, 0, 0.05);
--shadow-md:   0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
--shadow-lg:   0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
--shadow-xl:   0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
--shadow-2xl:  0 25px 50px -12px rgba(0, 0, 0, 0.25);
--shadow-inner: inset 0 2px 4px 0 rgba(0, 0, 0, 0.06);

/* Glow effects */
--shadow-glow-success: 0 0 20px rgba(76, 175, 80, 0.4);
--shadow-glow-error:   0 0 20px rgba(244, 67, 54, 0.4);
--shadow-glow-fire:    0 0 30px rgba(255, 107, 53, 0.5);
```

## Animations

### Durations
```css
--duration-instant: 0ms;
--duration-fast:    150ms;
--duration-normal:  250ms;
--duration-slow:    350ms;
--duration-slower:  500ms;
```

### Easings
```css
--ease-linear:      linear;
--ease-in:          cubic-bezier(0.4, 0, 1, 1);
--ease-out:         cubic-bezier(0, 0, 0.2, 1);
--ease-in-out:      cubic-bezier(0.4, 0, 0.2, 1);
--ease-bounce:      cubic-bezier(0.68, -0.55, 0.265, 1.55);
--ease-elastic:     cubic-bezier(0.175, 0.885, 0.32, 1.275);
```

### Keyframe Animations

```css
/* Shake - pro špatnou odpověď */
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-10px); }
  75% { transform: translateX(10px); }
}

/* Pulse - pro streak */
@keyframes pulse-fire {
  0%, 100% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.1); opacity: 0.8; }
}

/* Bounce - pro správnou odpověď */
@keyframes bounce-success {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-20px); }
}

/* Fade in up - pro modals */
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Confetti - pro level up */
@keyframes confetti-fall {
  0% { transform: translateY(-100%) rotate(0deg); }
  100% { transform: translateY(100vh) rotate(720deg); }
}

/* Progress bar fill */
@keyframes progress-fill {
  from { width: 0%; }
  to { width: var(--progress-width); }
}

/* Letter shuffle */
@keyframes letter-shuffle {
  0% { transform: translateY(0) rotate(0deg); }
  25% { transform: translateY(-10px) rotate(-10deg); }
  50% { transform: translateY(0) rotate(0deg); }
  75% { transform: translateY(10px) rotate(10deg); }
  100% { transform: translateY(0) rotate(0deg); }
}
```

## Components

### Buttons

#### Primary Button
```
Background: --color-primary-500
Text: white
Padding: --space-3 --space-6
Border-radius: --radius-xl
Font-weight: --font-semibold
Hover: --color-primary-600
Active: --color-primary-700
Shadow: --shadow-md
Hover-shadow: --shadow-lg
Transition: all --duration-fast --ease-out
```

#### Secondary Button
```
Background: transparent
Border: 2px solid --color-primary-500
Text: --color-primary-500
Hover: Background --color-primary-50
```

#### Danger Button
```
Background: --color-error-500
Text: white
Hover: --color-error-600
```

### Cards

```
Background: white
Border-radius: --radius-2xl
Padding: --space-6
Shadow: --shadow-md
Hover-shadow: --shadow-lg
Transition: box-shadow --duration-fast
```

### Inputs

```
Border: 2px solid --color-gray-300
Border-radius: --radius-xl
Padding: --space-3 --space-4
Font-size: --text-base
Focus: border-color --color-primary-500, shadow-glow
Error: border-color --color-error-500
```

### Progress Bar

```
Background track: --color-gray-200
Fill: gradient(--color-primary-500, --color-primary-600)
Height: 12px
Border-radius: --radius-full
Transition: width --duration-slow --ease-out
```

## Icons

### Icon Set
- **Library:** Phosphor Icons nebo Heroicons
- **Size small:** 16px
- **Size medium:** 20px
- **Size large:** 24px
- **Size xlarge:** 32px

### Key Icons Mapping
| Usage | Icon |
|-------|------|
| Streak | Flame (Phosphor: Fire) |
| XP | Star |
| Lives | Heart |
| Level | Trophy |
| League | Crown |
| Time | Clock |
| Hint | Lightbulb |
| Correct | CheckCircle |
| Wrong | XCircle |
| Settings | Gear |
| Profile | User |
| Shop | ShoppingBag |

## Responsive Breakpoints

```css
--breakpoint-sm: 640px;
--breakpoint-md: 768px;
--breakpoint-lg: 1024px;
--breakpoint-xl: 1280px;
--breakpoint-2xl: 1536px;
```

## Dark Mode

```css
/* Dark mode overrides */
[data-theme="dark"] {
  --color-bg-primary: #121212;
  --color-bg-secondary: #1E1E1E;
  --color-bg-tertiary: #2D2D2D;
  --color-text-primary: #FFFFFF;
  --color-text-secondary: #B0B0B0;
  --color-text-tertiary: #808080;
  --color-border: #404040;
}
```

## Z-index Scale

```css
--z-base:      0;
--z-dropdown:  100;
--z-sticky:    200;
--z-fixed:     300;
--z-modal:     400;
--z-popover:   500;
--z-tooltip:   600;
--z-toast:     700;
```
