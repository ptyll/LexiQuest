# UC-026: Landing Page

## Popis
Hlavní vstupní stránka aplikace LexiQuest. Cílem je představit hru, zaujmout návštěvníky a konvertovat je na registrované uživatele. Landing page musí být rychlá, responzivní a SEO optimalizovaná.

## Aktéři
- **Primary Actor:** Návštěvník (Host)
- **Secondary Actor:** SEO roboti, Marketingové nástroje

## Předpoklady
- Aplikace je nasazená a dostupná
- Statické assety (obrázky, fonty) jsou optimalizované

## Post-conditions
**Úspěch:**
- Návštěvník se zaregistruje/přihlásí
- Nebo vyzkouší hru jako host
- Zůstane na stránce a prozkoumá obsah

**Neúspěch:**
- Návštěvník odejde (bounce)

## Struktura stránky (Sekce)

### 1. Hero Section
**Obsah:**
- Logo LexiQuest (velké, animované)
- Slogan: "Rozlušti slova. Dobývej ligy. Udrž si oheň."
- CTA tlačítka: [🎮 Zkusit zdarma] [✨ Vytvořit účet]
- Animace zamíchaného slova (ukázka hry v reálném čase)
- Statistiky sociálního důkazu: "10,000+ hráčů"

**Funkcionalita:**
- [ ] Logo s pulse animací
- [ ] Interaktivní ukázka hry (JavaScript scramble demo)
- [ ] Parallax efekt na pozadí
- [ ] Sticky navigation po scrollu

### 2. Jak to funguje (3 kroky)
**Obsah:**
1. **Dostaneš slovo** - Přesmyčka k rozluštění
2. **Zamíchej písmena** - Přesuň je na správné místo
3. **Získej XP** - Postupuj v žebříčku

**Funkcionalita:**
- [ ] Ikony s mikro-animacemi
- [ ] Hover efekty na karty
- [ ] Lazy loading obrázků

### 3. Gamifikace Preview
**Obsah:**
- Ukázka cest (4 obtížnosti)
- Ukázka streak systému
- Ukázka ligového systému
- Achievementy

**Funkcionalita:**
- [ ] Interaktivní preview (carousel/tabs)
- [ ] Animace progress barů
- [ ] Ukázka "Fire" animace pro streak

### 4. Cesty (Learning Paths Preview)
**Obsah:**
- 🌱 Začátečník (3-5 písmen)
- 🌿 Mírně pokročilý (5-7 písmen)
- 🌳 Pokročilý (7-10 písmen)
- 🔥 Expert (10+ písmen)

**Funkcionalita:**
- [ ] Vizualizace cesty s uzly
- [ ] Hover odemyká detail
- [ ] Gradient mezi obtížnostmi

### 5. Social Proof (Testimonials)
**Obsah:**
- 3-4 recenze od "hráčů"
- Hodnocení hvězdičkami
- Fotky avatarů

**Funkcionality:**
- [ ] Carousel na mobilu
- [ ] Statické grid na desktopu
- [ ] Auto-rotate každých 5s

### 6. CTA Section (Před footerem)
**Obsah:**
- "Připraven začít?"
- Seznam výhod: Zdarma navždy, Žádné reklamy, 1000+ slov
- Hlavní CTA tlačítko

### 7. Footer
**Obsah:**
- Logo + slogan
- Rychlé odkazy: O hře, Podmínky, Ochrana soukromí, Kontakt
- Sociální sítě (ikony)
- Copyright

## SEO Požadavky

### Meta tagy
```html
<title>LexiQuest - Trénuj slovní zásobu zábavně</title>
<meta name="description" content="Rozlušti slova, dobývej ligy a udrž si oheň v LexiQuest. Bezplatná hra pro trénink slovní zásoby.">
<meta name="keywords" content="slovní hra, přesmyčky, vzdělávací hra, čeština, slovní zásoba">
<meta name="robots" content="index, follow">

<!-- Open Graph -->
<meta property="og:title" content="LexiQuest">
<meta property="og:description" content="Rozlušti slova. Dobývej ligy. Udrž si oheň.">
<meta property="og:image" content="/images/og-image.png">
<meta property="og:url" content="https://lexiquest.cz">

<!-- Twitter Card -->
<meta name="twitter:card" content="summary_large_image">
```

### Structured Data (JSON-LD)
```json
{
  "@context": "https://schema.org",
  "@type": "WebApplication",
  "name": "LexiQuest",
  "description": "Webová hra pro trénink slovní zásoby",
  "applicationCategory": "Game",
  "operatingSystem": "Any",
  "offers": {
    "@type": "Offer",
    "price": "0",
    "priceCurrency": "CZK"
  }
}
```

## Performance Požadavky

- [ ] Lighthouse score > 90 (Performance)
- [ ] First Contentful Paint < 1.5s
- [ ] Time to Interactive < 3.5s
- [ ] Optimalizované obrázky (WebP, lazy loading)
- [ ] Minifikované CSS/JS
- [ ] Preconnect pro externí fonty

## Responzivita

### Breakpointy
- **Mobile:** < 640px (single column)
- **Tablet:** 640px - 1024px (2 columns)
- **Desktop:** > 1024px (full layout)

### Mobile-first přístup
- Hero: Vertikální stack
- 3 kroky: Vertikální stack
- Testimonials: Carousel
- Footer: Collapsible sections

## Animace

### Page Load Sequence
```
1. Logo fade in (0ms delay)
2. Slogan fade in + slide up (200ms delay)
3. CTA buttons fade in + slide up (400ms delay)
4. Hero animation start (600ms delay)
```

### Scroll Animations
```
- Každá sekce fade in when entering viewport
- Stagger 100ms mezi elementy v sekci
- Easing: ease-out
- Duration: 400ms
```

### Hero Interactive Demo
```
- Slovo se zamíchá každých 5 sekund
- Uživatel může kliknout na písmena (nic se nestane, jen vizuální feedback)
- Ukázat "+10 XP" bublinu
```

## Resource klíče

```
Landing.Hero.Title
Landing.Hero.Slogan
Landing.Hero.CTA.TryFree
Landing.Hero.CTA.Register
Landing.Hero.Stats.Players
Landing.HowItWorks.Title
Landing.HowItWorks.Step1.Title
Landing.HowItWorks.Step1.Description
Landing.HowItWorks.Step2.Title
Landing.HowItWorks.Step2.Description
Landing.HowItWorks.Step3.Title
Landing.HowItWorks.Step3.Description
Landing.Gamification.Title
Landing.Gamification.Streak
Landing.Gamification.Leagues
Landing.Gamification.Achievements
Landing.Paths.Title
Landing.Paths.Beginner
Landing.Paths.Intermediate
Landing.Paths.Advanced
Landing.Paths.Expert
Landing.Testimonials.Title
Landing.CTA.Title
Landing.CTA.Subtitle
Landing.CTA.Button
Landing.CTA.Feature1
Landing.CTA.Feature2
Landing.CTA.Feature3
Landing.Footer.Slogan
Landing.Footer.Links.About
Landing.Footer.Links.Terms
Landing.Footer.Links.Privacy
Landing.Footer.Links.Contact
```

## Technical Implementation

### Blazor Static Rendering
```
Landing page bude prerenderovaná (Static Server-Side Rendering)
pro lepší SEO a rychlejší načtení.
```

### Code Structure
```
Pages/
├── Index.razor              (Landing page)
├── Index.razor.css          (Scoped styles)
└── Index.razor.js           (JavaScript interop pro animace)

Components/Landing/
├── HeroSection.razor
├── HowItWorksSection.razor
├── GamificationPreview.razor
├── PathsPreview.razor
├── TestimonialsSection.razor
├── CTASection.razor
└── LandingFooter.razor
```

## Analytics

### Tracked Events
- [ ] Page view
- [ ] CTA click ("Zkusit zdarma")
- [ ] CTA click ("Vytvořit účet")
- [ ] Hero demo interaction
- [ ] Scroll depth (25%, 50%, 75%, 100%)
- [ ] Time on page

## A/B Testing Možnosti
- [ ] Varianta A: Video background vs. Static image
- [ ] Varianta B: CTA button color (Orange vs. Green)
- [ ] Varianta C: Pořadí sekcí (Gamifikace vs. Cesty)

## Odhad implementace
| Část | Hodiny |
|------|--------|
| Struktura a layout | 3h |
| Animace a interakce | 3h |
| SEO optimalizace | 1h |
| Responzivita | 1h |
| **Celkem** | **8h** |

## Dependencies
- Žádné externí dependencies (vyjma Google Fonts, Analytics)
- Použít Blazor built-in funkcionalitu
- CSS animations místo JS kde možné
