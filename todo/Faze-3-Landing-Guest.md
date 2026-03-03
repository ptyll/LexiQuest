# Fáze 3: Landing Page & Guest Mode (Týden 5)

> **Cíl:** Marketingová landing page pro konverzi a testovací hra bez registrace
> **Závislost:** Fáze 1 (herní smyčka) dokončena
> **Tempo.Blazor komponenty:** TmButton, TmCard, TmIcon, TmBadge, TmAlert, TmModal, TmProgressBar, TmTooltip, TmSkeleton, ToastService

---

## ⚠️ KRITICKÁ PRAVIDLA

- **TDD:** Test FIRST → RED → GREEN → REFACTOR
- **Žádné hardcoded texty** → vše z `.resx`
- **Produkční kód** od prvního řádku
- **SEO optimalizace** na landing page
- **Performance:** Lighthouse > 90, FCP < 1.5s

---

## T-300: UC-026 Landing Page

### T-300.1: Landing Page Resources
- [ ] Vytvořit `LexiQuest.Blazor/Resources/Pages/Index.resx` s klíči:
  - Hero.Tagline, Hero.Subtitle, Hero.CTA.Register, Hero.CTA.TryFree, Hero.SocialProof
  - HowItWorks.Title, HowItWorks.Step1.Title/Description, Step2.Title/Description, Step3.Title/Description
  - Features.Title, Features.Streak.Title/Description, Features.Leagues.Title/Description, Features.Achievements.Title/Description, Features.Paths.Title/Description
  - Paths.Title, Path1.Name/Description, Path2.Name/Description, Path3.Name/Description, Path4.Name/Description
  - Testimonials.Title, Testimonial1.Quote/Author/Role, Testimonial2..., Testimonial3...
  - CTA.Title, CTA.Subtitle, CTA.Benefit1-5, CTA.Button
  - Footer.About, Footer.Terms, Footer.Privacy, Footer.Contact, Footer.Copyright

### T-300.2: Hero Section (Tempo.Blazor)
- [ ] **TEST (bUnit):** `HeroSection_Renders_Tagline` → RED
- [ ] **TEST (bUnit):** `HeroSection_Renders_CTAButtons` → RED
- [ ] **TEST (bUnit):** `HeroSection_Renders_AnimatedDemo` → RED
- [ ] Vytvořit `HeroSection.razor` komponentu
- [ ] `@inject IStringLocalizer<Index> L`
- [ ] Logo LexiQuest s animací (fade + scale, 600ms)
- [ ] Tagline: `@L["Hero.Tagline"]` - velký font (Poppins, 48px)
- [ ] Subtitle: `@L["Hero.Subtitle"]` - menší text
- [ ] CTA tlačítka:
  - `<TmButton Variant="Primary" Size="Lg" OnClick="NavigateToRegister">@L["Hero.CTA.Register"]</TmButton>`
  - `<TmButton Variant="Outline" Size="Lg" OnClick="NavigateToGuest">@L["Hero.CTA.TryFree"]</TmButton>`
- [ ] Animated demo: scramble animation (každých 5s přeháže písmena slova → ukáže správnou odpověď)
- [ ] Social proof: `@L["Hero.SocialProof"]` ("10 000+ hráčů")
- [ ] Gradient pozadí: white → orange (subtle)
- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Animace load sequence (fade/slide stagger)

### T-300.3: How It Works Section
- [ ] **TEST (bUnit):** `HowItWorksSection_Renders_3Steps` → RED
- [ ] Vytvořit `HowItWorksSection.razor`
- [ ] 3× `TmCard` s ikonami a popisem:
  1. `TmIcon` (puzzle) + `@L["HowItWorks.Step1.Title"]` + description
  2. `TmIcon` (shuffle) + `@L["HowItWorks.Step2.Title"]` + description
  3. `TmIcon` (star) + `@L["HowItWorks.Step3.Title"]` + description
- [ ] Čísla kroků (1, 2, 3) v kruzích
- [ ] Hover efekt: scale 1.02, shadow
- [ ] Stagger animations (100ms delays)
- [ ] **GREEN:** Test prochází

### T-300.4: Features/Gamification Preview Section
- [ ] **TEST (bUnit):** `FeaturesSection_Renders_FeatureTabs` → RED
- [ ] Vytvořit `FeaturesPreview.razor`
- [ ] `TmTabs` + `TmTabPanel` pro feature kategorie:
  - Streak (🔥): vizuální streak counter, fire animace
  - Leagues (🏆): mini leaderboard preview
  - Achievements (⭐): grid odznaků
  - Paths (🗺️): mini path vizualizace
- [ ] Každý tab s ilustrací/screenshotem
- [ ] **GREEN:** Test prochází

### T-300.5: Learning Paths Preview
- [ ] Vytvořit `PathsPreview.razor`
- [ ] 4× `TmCard` pro cesty s gradienty:
  - 🌱 Beginner: zelený gradient, "3-5 písmen"
  - 🌿 Intermediate: modro-zelený, "5-7 písmen"
  - 🌳 Advanced: hnědý, "7-10 písmen"
  - 🔥 Expert: červeno-oranžový, "10+ písmen"
- [ ] Letter count badges: `TmBadge`
- [ ] Features pro každou cestu
- [ ] Hover animations

### T-300.6: Testimonials Section
- [ ] **TEST (bUnit):** `TestimonialsSection_Renders_3Reviews` → RED
- [ ] Vytvořit `TestimonialsSection.razor`
- [ ] 3× `TmCard` s recenzemi:
  - `TmAvatar` autora
  - Quote text z .resx
  - Jméno a role
  - 5 hvěziček (⭐⭐⭐⭐⭐)
- [ ] Carousel na mobilu (swipe gesture)
- [ ] **GREEN:** Test prochází

### T-300.7: Final CTA Section
- [ ] Vytvořit `FinalCTA.razor`
- [ ] `TmCard` (Elevated) s:
  - Titulek: `@L["CTA.Title"]`
  - Benefits list (5 bodů s checkmark ikonami)
  - `TmButton Variant="Primary" Size="Lg" Block="true">@L["CTA.Button"]</TmButton>`
- [ ] Gradient pozadí (oranžový)

### T-300.8: Footer
- [ ] Vytvořit `AppFooter.razor` komponentu
- [ ] `@inject IStringLocalizer<Footer> L`
- [ ] Logo, navigační linky (About, Terms, Privacy, Contact)
- [ ] Sociální ikony: `TmIcon` (github, twitter, discord)
- [ ] Copyright text z .resx

### T-300.9: Landing Page Assembly
- [ ] Vytvořit `Index.razor` (`@page "/"`) s layout bez sidebar (full-width)
- [ ] Sestavit sekce: Hero → HowItWorks → Features → Paths → Testimonials → CTA → Footer
- [ ] Scroll fade-in animace per sekce (Intersection Observer)
- [ ] Smooth scroll pro anchor linky

### T-300.10: SEO a Performance
- [ ] Meta tags: title, description, keywords v `<head>`
- [ ] Open Graph tags (og:title, og:description, og:image)
- [ ] Twitter Card meta tags
- [ ] JSON-LD schema (WebApplication)
- [ ] Lazy loading obrázků
- [ ] Minified CSS
- [ ] Lighthouse audit: Performance > 90, Accessibility > 90
- [ ] FCP < 1.5s, TTI < 3.5s

### T-300.11: Responsive Design
- [ ] Mobile (< 640px): single column, hamburger menu, stacked CTA buttons
- [ ] Tablet (640-1024px): 2 column grid, medium sized elements
- [ ] Desktop (> 1024px): full layout, side-by-side hero
- [ ] Testovat na různých zařízeních

---

## T-301: UC-027 Guest Mode - Backend

### T-301.1: GuestSessionService (TDD)
- [ ] **TEST:** `GuestSessionService_StartGame_CreatesAnonymousSession` → RED
- [ ] **TEST:** `GuestSessionService_StartGame_UsesBeginnerWords` → RED
- [ ] **TEST:** `GuestSessionService_StartGame_5WordsPerGame` → RED
- [ ] **TEST:** `GuestSessionService_SubmitAnswer_Correct_CalculatesXP` → RED
- [ ] **TEST:** `GuestSessionService_SubmitAnswer_Wrong_ShowsCorrectAnswer` → RED
- [ ] Vytvořit `IGuestSessionService` interface
- [ ] Implementovat `GuestSessionService` - in-memory session (bez DB persistence)
- [ ] Použít pouze Beginner slova (Path 1, level 1-5)
- [ ] XP se zobrazuje ale neukládá (motivace k registraci)
- [ ] **GREEN:** Všechny testy prochází

### T-301.2: Guest Rate Limiter (TDD)
- [ ] **TEST:** `GuestLimiter_FirstGame_Allows` → RED
- [ ] **TEST:** `GuestLimiter_5thGame_Allows` → RED
- [ ] **TEST:** `GuestLimiter_6thGame_Denies` → RED
- [ ] **TEST:** `GuestLimiter_After24h_ResetsCounter` → RED
- [ ] Vytvořit `IGuestLimiter` interface
- [ ] Implementovat `GuestLimiter` - cookie-based tracking (guestGamesToday, guestLastDate)
- [ ] Max 5 her za 24h
- [ ] Fallback: IP-based tracking přes `IMemoryCache`
- [ ] **GREEN:** Testy prochází

### T-301.3: Guest Endpoints
- [ ] **TEST:** `GuestStartEndpoint_Returns200_WithScrambledWord` → RED
- [ ] **TEST:** `GuestStartEndpoint_LimitReached_Returns429` → RED
- [ ] **TEST:** `GuestAnswerEndpoint_Returns200_WithResult` → RED
- [ ] **TEST:** `GuestStatusEndpoint_Returns200_WithRemainingGames` → RED
- [ ] Vytvořit `POST /api/v1/game/guest/start` endpoint (bez [Authorize])
- [ ] Vytvořit `POST /api/v1/game/guest/answer` endpoint (bez [Authorize])
- [ ] Vytvořit `GET /api/v1/guest/status` endpoint (zbývající hry)
- [ ] Vytvořit `POST /api/v1/guest/convert` endpoint (převod guest → registered)
- [ ] **GREEN:** Testy prochází

### T-301.4: Guest to Registered Conversion (TDD)
- [ ] **TEST:** `GuestConversion_WithProgress_TransfersXPToNewAccount` → RED
- [ ] **TEST:** `GuestConversion_WithoutProgress_CreatesCleanAccount` → RED
- [ ] Implementovat konverzi: přenést guest XP a statistiky do nového registrovaného účtu
- [ ] **GREEN:** Testy prochází

---

## T-302: UC-027 Guest Mode - Frontend

### T-302.1: Guest Game Flow (Tempo.Blazor)
- [ ] **TEST (bUnit):** `GuestGame_Renders_GameArena` → RED
- [ ] **TEST (bUnit):** `GuestGame_ShowsRemainingGames` → RED
- [ ] **TEST (bUnit):** `GuestGame_LimitReached_ShowsRegisterCTA` → RED
- [ ] Vytvořit `GuestGame.razor` (`@page "/play"`)
- [ ] `@inject IStringLocalizer<Game> L`
- [ ] Reuse `GameArena` komponentu (bez streak, bez lives persistence)
- [ ] Header: `TmBadge` "Host" label, zbývající hry counter
- [ ] Bez navigace (simplified layout)
- [ ] Po každé hře: CTA modal

### T-302.2: Guest CTA Modal (Tempo.Blazor)
- [ ] **TEST (bUnit):** `GuestCTAModal_Renders_AfterGame` → RED
- [ ] **TEST (bUnit):** `GuestCTAModal_ShowsBenefits` → RED
- [ ] Vytvořit `GuestCTAModal.razor`
- [ ] `TmModal` (Size: Medium) po dokončení každé guest hry
- [ ] Výsledky hry (slova, XP - "ale nejsou uloženy!")
- [ ] Benefits registrace:
  - `TmIcon` (check) + "Neomezený počet her"
  - `TmIcon` (check) + "Ukládání XP a progressu"
  - `TmIcon` (check) + "Ligy a žebříčky"
  - `TmIcon` (check) + "Streak a achievementy"
  - `TmIcon` (check) + "Multiplayer souboje"
- [ ] `TmButton Variant="Primary" Size="Lg">Zaregistrovat se</TmButton>`
- [ ] `TmButton Variant="Ghost">Hrát další hru</TmButton>` (pokud zbývají)
- [ ] **GREEN:** Testy prochází

### T-302.3: Guest Limit Screen
- [ ] Vytvořit `GuestLimitReached.razor` komponentu
- [ ] `TmCard` (Elevated) s:
  - `TmIcon` (lock) velký
  - Titulek: "Denní limit her dosažen"
  - Popis: "Zaregistruj se pro neomezený přístup"
  - Benefits list
  - `TmButton` registrace
  - Countdown do resetu (za kolik hodin se limit resetuje)
- [ ] **GREEN:** Testy prochází

### T-302.4: Guest Progress Conversion UI
- [ ] Vytvořit `GuestConvertModal.razor`
- [ ] `TmModal` při registraci guest uživatele
- [ ] Zobrazí: "Převést tvůj guest progress?"
- [ ] XP earned, words solved, best time
- [ ] `TmButton` "Převést a registrovat" / "Registrovat bez převodu"
- [ ] LocalStorage: uložit guest progress (guestGamesToday, guestProgress)
- [ ] Při registraci: odeslat guest data na `/api/v1/guest/convert`

### T-302.5: Landing Page Integration
- [ ] Přidat "Hrát jako host" tlačítko na landing page Hero section
- [ ] Click → navigace na `/play`
- [ ] Kontrola limitu před startem hry
- [ ] Po hře → CTA modal s registrací

---

## Tempo.Blazor komponenty použité v této fázi

| Komponenta | Použití |
|------------|---------|
| `TmButton` | CTA buttons, game controls, navigation |
| `TmCard` | Feature cards, testimonials, CTA, limit screen |
| `TmIcon` | Steps icons, feature icons, benefit checkmarks |
| `TmBadge` | Path difficulty, guest label, modifiers |
| `TmTabs` + `TmTabPanel` | Feature preview tabs |
| `TmAvatar` | Testimonial avatars |
| `TmModal` | Guest CTA, conversion, limit reached |
| `TmAlert` | Warnings, info messages |
| `TmProgressBar` | Guest remaining games |
| `TmTooltip` | Feature descriptions |
| `ToastService` | Success/error notifications |

---

## Ověření dokončení fáze

- [ ] Landing page kompletní se všemi sekcemi
- [ ] SEO: meta tags, Open Graph, JSON-LD
- [ ] Lighthouse > 90 (Performance + Accessibility)
- [ ] Responsive: mobile, tablet, desktop
- [ ] Guest mode: hrát bez registrace (max 5 her/24h)
- [ ] Guest CTA: modal po každé hře s benefits
- [ ] Guest limit: blokace po 5 hrách, countdown
- [ ] Guest conversion: převod progressu při registraci
- [ ] Všechny texty z .resx
- [ ] `dotnet test` → všechny testy zelené
