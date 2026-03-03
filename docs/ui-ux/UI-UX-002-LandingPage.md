# UI-UX-002: Landing Page

## Struktura stránky

### 1. Hero Section

#### Layout
```
┌─────────────────────────────────────────────────────────────┐
│  [Logo]        [O hře] [Funkce] [Kontakt]    [Přihlásit]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│                    🔥 LOGO LEXIQUEST 🔥                     │
│                                                             │
│         „Rozlušti slova. Dobývej ligy. Udrž si oheň.“      │
│                                                             │
│              [🚀 ZAČNI HRÁT ZDARMA]                        │
│                                                             │
│         Již 10,000+ hráčů • Žádná kreditní karta           │
│                                                             │
│     ┌───────────────────────────────────────────┐          │
│     │  [ANIMACE: Zamíchané slovo se skládá]     │          │
│     │                                           │          │
│     │     S L O V O   →   S O L O V             │          │
│     │                                           │          │
│     │  +10 XP ✓  + Rychlost bonus!              │          │
│     └───────────────────────────────────────────┘          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### Design Specs
- **Background:** Gradient (white → --color-primary-50)
- **Logo:** 120px, centered
- **Tagline:** --text-2xl, --color-gray-700
- **CTA Button:** Primary, --text-xl, padding --space-4 --space-8
- **Mockup Game:** Card with shadow, slight rotation (-2deg)

### 2. Jak to funguje (3 kroky)

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│              Jak začít? Je to jednoduché!                   │
│                                                             │
│    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐   │
│    │     1️⃣      │    │     2️⃣      │    │     3️⃣      │   │
│    │             │    │             │    │             │   │
│    │   [ikonka]  │    │   [ikonka]  │    │   [ikonka]  │   │
│    │             │    │             │    │             │   │
│    │  Dostaneš   │    │   Zamíchej  │    │   Získej    │   │
│    │   slovo     │    │    písmena  │    │     XP      │   │
│    │             │    │             │    │             │   │
│    │ Přesmyčka   │    │  Přesuň je  │    │ Postupuj v  │   │
│    │  k rozluštění│    │  na správné │    │   žebříčku  │   │
│    │             │    │   místo     │    │             │   │
│    └─────────────┘    └─────────────┘    └─────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### Design Specs
- **Background:** White
- **Kroky:** 3 cards, grid gap --space-8
- **Čísla:** Circle with gradient background
- **Ikony:** 64px, --color-primary-500

### 3. Gamifikace preview

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│          🎮 Gamifikace, která tě pohltí                     │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  [STREAK]  [LIGY]  [ACHIEVEMENTY]  [CESTY]         │   │
│  │                                                     │   │
│  │  Aktivní obsah podle výběru tabu:                   │   │
│  │                                                     │   │
│  │  🔥 15 dní streak!    🏆 Zlatá liga               │   │
│  │  ━━━━━━━━━━━━━━━━     ━━━━━━━━━━                  │   │
│  │                                                     │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 4. Cesty preview

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│           4 cesty od začátečníka po experta                 │
│                                                             │
│  🌱 Začátečník    🌿 Pokročilý    🌳 Expert    🔥 Mistr    │
│                                                             │
│     3-5 písmen       5-7 písmen      7-10         10+      │
│     Bez limitu       Časovka         Krátký       Expert   │
│                                                             │
│  [Ukázka vizuálu cesty s dokončenými/nedokončenými uzly]   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 5. Social Proof

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│         Co říkají naši hráči?                               │
│                                                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │ ⭐⭐⭐⭐⭐   │  │ ⭐⭐⭐⭐⭐   │  │ ⭐⭐⭐⭐⭐   │         │
│  │             │  │             │  │             │         │
│  │ „Skvělá    │  │ „Denní      │  │ „Konečně   │         │
│  │  appka!    │  │  výzvy mě  │  │  něco co   │         │
│  │  Můj      │  │  nutí      │  │  mě baví   │         │
│  │  syn      │  │  hrát      │  │  učit      │         │
│  │  se       │  │  každý     │  │  se!"      │         │
│  │  učí      │  │  den!"     │  │             │         │
│  │  číst     │  │             │  │             │         │
│  │  líp!"    │  │             │  │             │         │
│  │             │  │             │  │             │         │
│  │ — Petra,  │  │ — Tomáš,  │  │ — Jana,    │         │
│  │   máma    │  │   student │  │   učitelka │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 6. CTA Section

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│         Připraven začít?                                    │
│                                                             │
│    Začni trénovat svůj mozek ještě dnes!                   │
│                                                             │
│         [🚀 ZAREGISTRUJ SE ZDARMA]                         │
│                                                             │
│    ✓ Zdarma navždy    ✓ Žádné reklamy (Premium)           │
│    ✓ 1000+ slov       ✓ Týdenní soutěže                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 7. Footer

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  [Logo]                              [Ikony sociálních sítí]│
│  Rozlušti slova.                                           │
│  Dobývej ligy.                                             │
│  Udrž si oheň.                                             │
│                                                             │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐      │
│  │  Hra     │ │  Společ- │ │  Podpora │ │  Legální │      │
│  │  ─────── │ │  nost    │ │  ─────── │ │  ─────── │      │
│  │  Cesty   │ │  Discord │ │  FAQ     │ │  Podmínky│      │
│  │  Ligy    │ │  Reddit  │ │  Kontakt │ │  Ochrana │      │
│  │  ...     │ │  ...     │ │  ...     │ │  ...     │      │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘      │
│                                                             │
│  © 2024 LexiQuest. Všechna práva vyhrazena.                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Animace a interakce

### Hero Section
- **Logo:** Fade in + scale from 0.8, duration 600ms
- **Tagline:** Fade in, delay 200ms
- **CTA:** Fade in + slide up, delay 400ms
- **Mockup:** Slide in from right, delay 600ms, slight float animation

### Scroll animations
- **Kroky:** Fade in + slide up when in viewport
- **Cards:** Stagger animation (100ms delay between each)

### Hover effects
- **Buttons:** Scale 1.02, shadow increase
- **Cards:** Lift up (translateY -4px), shadow increase
- **Links:** Color transition to primary

## Resource klíče

```
Landing.Hero.Tagline
Landing.Hero.CTA.Primary
Landing.Hero.CTA.Secondary
Landing.Hero.Stats.Players
Landing.HowItWorks.Title
Landing.HowItWorks.Step1.Title
Landing.HowItWorks.Step1.Description
Landing.HowItWorks.Step2.Title
Landing.HowItWorks.Step2.Description
Landing.HowItWorks.Step3.Title
Landing.HowItWorks.Step3.Description
Landing.Gamification.Title
Landing.Paths.Title
Landing.Paths.Beginner
Landing.Paths.Intermediate
Landing.Paths.Advanced
Landing.Paths.Expert
Landing.Testimonials.Title
Landing.CTA.Title
Landing.CTA.Subtitle
Landing.CTA.Button
Landing.Footer.Slogan
Landing.Footer.Links.Game
Landing.Footer.Links.Community
Landing.Footer.Links.Support
Landing.Footer.Links.Legal
```

## Mobile Responsiveness

### Breakpoint < 768px
- Hero: Stack vertically, mockup below CTA
- Kroky: Single column
- Testimonials: Carousel/slider
- Navigation: Hamburger menu

## Accessibility

- [ ] Skip to content link
- [ ] Alt texty pro všechny obrázky
- [ ] Kontrastní barvy (4.5:1 minimum)
- [ ] Focus stavy pro klávesnici
- [ ] Reduced motion respektuje prefers-reduced-motion
