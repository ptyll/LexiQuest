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
- [x] Vytvořit `LexiQuest.Blazor/Resources/Pages/Index.resx` s klíči:
  - Hero.Tagline, Hero.Subtitle, Hero.CTA.Register, Hero.CTA.TryFree, Hero.SocialProof
  - HowItWorks.Title, HowItWorks.Step1.Title/Description, Step2.Title/Description, Step3.Title/Description
  - Features.Title, Features.Streak.Title/Description, Features.Leagues.Title/Description, Features.Achievements.Title/Description, Features.Paths.Title/Description
  - Paths.Title, Path1.Name/Description, Path2.Name/Description, Path3.Name/Description, Path4.Name/Description
  - Testimonials.Title, Testimonial1.Quote/Author/Role, Testimonial2..., Testimonial3...
  - CTA.Title, CTA.Subtitle, CTA.Benefit1-5, CTA.Button
  - Footer.About, Footer.Terms, Footer.Privacy, Footer.Contact, Footer.Copyright

### T-300.2: Hero Section (Tempo.Blazor)
- [x] **TEST (bUnit):** `HeroSection_Renders_Tagline` → RED
- [x] **TEST (bUnit):** `HeroSection_Renders_CTAButtons` → RED
- [x] **TEST (bUnit):** `HeroSection_Renders_AnimatedDemo` → RED
- [x] Vytvořit `HeroSection.razor` komponentu
- [x] `@inject IStringLocalizer<HeroSection> L`
- [x] Logo LexiQuest s animací (fade + scale, 600ms)
- [x] Tagline: `@L["Hero.Tagline"]` - velký font (Poppins, 48px)
- [x] Subtitle: `@L["Hero.Subtitle"]` - menší text
- [x] CTA tlačítka:
  - `<TmButton Variant="Primary" Size="Lg" OnClick="NavigateToRegister">@L["Hero.CTA.Register"]</TmButton>`
  - `<TmButton Variant="Outline" Size="Lg" OnClick="NavigateToGuest">@L["Hero.CTA.TryFree"]</TmButton>`
- [x] Animated demo: scramble animation (každých 5s přeháže písmena slova → ukáže správnou odpověď)
- [x] Social proof: `@L["Hero.SocialProof"]` ("10 000+ hráčů")
- [x] Gradient pozadí: white → orange (subtle)
- [x] **GREEN:** Testy prochází (7/7 ✅)
- [x] **REFACTOR:** Animace load sequence (fade/slide stagger)

### T-300.3: How It Works Section
- [x] **TEST (bUnit):** `HowItWorksSection_Renders_3Steps` → RED
- [x] Vytvořit `HowItWorksSection.razor`
- [x] 3× `TmCard` s ikonami a popisem:
  1. `TmIcon` (puzzle) + `@L["HowItWorks.Step1.Title"]` + description
  2. `TmIcon` (shuffle) + `@L["HowItWorks.Step2.Title"]` + description
  3. `TmIcon` (star) + `@L["HowItWorks.Step3.Title"]` + description
- [x] Čísla kroků (1, 2, 3) v kruzích
- [x] Hover efekt: scale 1.02, shadow
- [x] Stagger animations (100ms delays)
- [x] **GREEN:** Test prochází (6/6 ✅)

### T-300.4: Features/Gamification Preview Section
- [x] **TEST (bUnit):** `FeaturesSection_Renders_FeatureTabs` → RED
- [x] Vytvořit `FeaturesSection.razor`
- [x] `TmTabs` + `TmTabPanel` pro feature kategorie:
  - Streak (🔥): vizuální streak counter, fire animace
  - Leagues (🏆): mini leaderboard preview
  - Achievements (⭐): grid odznaků
  - Paths (🗺️): mini path vizualizace
- [x] Každý tab s ilustrací/screenshotem
- [x] **GREEN:** Test prochází (6/6 ✅)

### T-300.5: Learning Paths Preview
- [x] Vytvořit `PathsPreviewSection.razor`
- [x] 4× `TmCard` pro cesty s gradienty:
  - 🌱 Beginner: zelený gradient, "3-5 písmen"
  - 🌿 Intermediate: modro-zelený, "5-7 písmen"
  - 🌳 Advanced: hnědý, "7-10 písmen"
  - 🔥 Expert: červeno-oranžový, "10+ písmen"
- [x] Letter count badges: `TmBadge`
- [x] Features pro každou cestu
- [x] Hover animations
- [x] **GREEN:** Testy prochází (7/7 ✅)

### T-300.6: Testimonials Section
- [x] **TEST (bUnit):** `TestimonialsSection_Renders_3Reviews` → RED
- [x] Vytvořit `TestimonialsSection.razor`
- [x] 3× `TmCard` s recenzemi:
  - `TmAvatar` autora
  - Quote text z .resx
  - Jméno a role
  - 5 hvěziček (⭐⭐⭐⭐⭐)
- [x] Responsive grid (3→2→1)
- [x] **GREEN:** Test prochází (7/7 ✅)

### T-300.7: Final CTA Section
- [x] Vytvořit `FinalCTASection.razor`
- [x] TmCard styl s:
  - Titulek: `@L["CTA.Title"]`
  - Benefits list (5 bodů s checkmark ikonami)
  - `TmButton Variant="Primary" Size="Lg">@L["CTA.Button"]</TmButton>`
- [x] Gradient pozadí (oranžový)
- [x] **GREEN:** Testy prochází (6/6 ✅)

### T-300.8: Footer
- [x] Vytvořit `Footer.razor` komponentu
- [x] `@inject IStringLocalizer<Footer> L`
- [x] Logo, navigační linky (About, Terms, Privacy, Contact)
- [x] Sociální ikony: `TmIcon` (github, twitter, discord)
- [x] Copyright text z .resx
- [x] **GREEN:** Testy prochází (5/5 ✅)

### T-300.9: Landing Page Assembly
- [x] Vytvořit `Home.razor` (`@page "/"`) s `LandingLayout`
- [x] Sestavit sekce: Hero → HowItWorks → Features → Paths → Testimonials → CTA → Footer
- [x] LandingLayout pro full-width layout
- [x] **GREEN:** Build OK ✅

### T-300.10: SEO a Performance
- [x] Meta tags: title, description, keywords v `<head>`
- [x] Open Graph tags (og:title, og:description, og:image)
- [x] Twitter Card meta tags
- [x] JSON-LD schema (WebApplication)
- [x] Lazy loading ready
- [x] Minified CSS (scoped)
- [ ] Lighthouse audit: Performance > 90 (runtime test)
- [ ] FCP < 1.5s (runtime test)

### T-300.11: Responsive Design
- [x] Mobile (< 640px): single column, stacked CTA buttons
- [x] Tablet (640-1024px): 2 column grid
- [x] Desktop (> 1024px): full layout
- [x] Responsive všechny komponenty

---

## T-301: UC-027 Guest Mode - Backend

### T-301.1: GuestSessionService (TDD)
- [x] **TEST:** `GuestSessionService_StartGame_CreatesAnonymousSession` → RED
- [x] **TEST:** `GuestSessionService_StartGame_UsesBeginnerWords` → RED
- [x] **TEST:** `GuestSessionService_StartGame_5WordsPerGame` → RED
- [x] **TEST:** `GuestSessionService_SubmitAnswer_Correct_CalculatesXP` → RED
- [x] **TEST:** `GuestSessionService_SubmitAnswer_Wrong_ShowsCorrectAnswer` → RED
- [x] Vytvořit `IGuestSessionService` interface
- [x] Implementovat `GuestSessionService` - in-memory session (bez DB persistence)
- [x] Použít pouze Beginner slova (DifficultyLevel.Beginner)
- [x] XP se vypočítá ale neukládá do DB (motivace k registraci)
- [x] **GREEN:** Všechny testy prochází (6/6 ✅)

### T-301.2: Guest Rate Limiter (TDD)
- [x] **TEST:** `GuestLimiter_FirstGame_Allows` → RED
- [x] **TEST:** `GuestLimiter_5thGame_Allows` → RED
- [x] **TEST:** `GuestLimiter_6thGame_Denies` → RED
- [x] **TEST:** `GuestLimiter_After24h_ResetsCounter` → RED
- [x] Vytvořit `IGuestLimiter` interface
- [x] Implementovat `GuestLimiter` - IP-based tracking přes `IMemoryCache`
- [x] Max 5 her za 24h
- [x] **GREEN:** Testy prochází (6/6 ✅)

### T-301.3: Guest Endpoints
- [x] **TEST:** `GuestStartEndpoint_Returns200_WithScrambledWord` → RED
- [x] **TEST:** `GuestStartEndpoint_LimitReached_Returns429` → RED
- [x] **TEST:** `GuestAnswerEndpoint_Returns200_WithResult` → RED
- [x] **TEST:** `GuestStatusEndpoint_Returns200_WithRemainingGames` → RED
- [x] Vytvořit `POST /api/v1/game/guest/start` endpoint (bez [Authorize])
- [x] Vytvořit `POST /api/v1/game/guest/answer` endpoint (bez [Authorize])
- [x] Vytvořit `GET /api/v1/game/guest/status` endpoint (zbývající hry)
- [x] Vytvořit `POST /api/v1/game/guest/convert` endpoint (převod guest → registered)
- [x] **GREEN:** Testy prochází (4/4 ✅)

### T-301.4: Guest to Registered Conversion (TDD)
- [x] **TEST:** `GuestConversion_WithProgress_TransfersXPToNewAccount` → RED
- [x] **TEST:** `GuestConversion_WithoutProgress_CreatesCleanAccount` → RED
- [x] Implementovat konverzi: přenést guest XP a statistiky do nového registrovaného účtu
- [x] **GREEN:** Testy prochází (2/2 ✅)

---

## T-302: UC-027 Guest Mode - Frontend

### T-302.1: Guest Game Flow (Tempo.Blazor)
- [x] **TEST (bUnit):** `GuestGame_Renders_GameArena` → RED
- [x] **TEST (bUnit):** `GuestGame_ShowsRemainingGames` → RED
- [x] **TEST (bUnit):** `GuestGame_LimitReached_ShowsRegisterCTA` → RED
- [x] Vytvořit `GuestGame.razor` (`@page "/play"`)
- [x] `@inject IStringLocalizer<GuestGame> L`
- [x] GameArena komponenta (bez streak, bez lives persistence)
- [x] Header: `TmBadge` zbývající hry counter
- [x] Po každé hře: CTA modal
- [x] **GREEN:** Build OK ✅

### T-302.2: Guest CTA Modal (Tempo.Blazor)
- [x] **TEST (bUnit):** `GuestCTAModal_Renders_AfterGame` → RED
- [x] **TEST (bUnit):** `GuestCTAModal_ShowsBenefits` → RED
- [x] Vytvořit `GuestCTAModal.razor`
- [x] Modal (Size: Medium) po dokončení každé guest hry
- [x] Výsledky hry (slova, XP)
- [x] Benefits registrace:
  - `TmIcon` (save) + "Ukládání pokroku"
  - `TmIcon` (trophy) + "Achievementy a odznaky"
  - `TmIcon` (users) + "Soutěž v ligách"
  - `TmIcon` (activity) + "Detailní statistiky"
- [x] `TmButton Variant="Primary">Zaregistrovat se</TmButton>`
- [x] `TmButton Variant="Ghost">Možná později</TmButton>`
- [x] **GREEN:** Build OK ✅

### T-302.3: Guest Limit Screen
- [x] Vytvořit `GuestLimitReached.razor` komponentu
- [x] `TmCard` (Elevated) s:
  - `TmIcon` (clock) velký
  - Titulek: "Denní limit dosažen"
  - Popis: "Zaregistruj se pro neomezený přístup"
  - Benefits list
  - `TmButton` registrace
- [x] **GREEN:** Build OK ✅

### T-302.4: Guest Progress Conversion UI
- [x] Vytvořit `GuestConvertModal.razor`
- [x] Modal při dokončení hry
- [x] Zobrazí: "Hra dokončena!"
- [x] XP earned, words solved
- [x] `TmButton` "Uložit pokrok" / "Hrát znovu"
- [x] **GREEN:** Build OK ✅

### T-302.5: Landing Page Integration
- [x] Přidat "Hrát jako host" tlačítko na landing page Hero section
- [x] Click → navigace na `/play`
- [x] Kontrola limitu před startem hry
- [x] Po hře → CTA modal s registrací

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

- [x] Landing page kompletní se všemi sekcemi
- [x] SEO: meta tags, Open Graph, JSON-LD
- [ ] Lighthouse > 90 (Performance + Accessibility) - runtime test
- [x] Responsive: mobile, tablet, desktop
- [x] Guest mode: hrát bez registrace (max 5 her/24h)
- [x] Guest CTA: modal po každé hře s benefits
- [x] Guest limit: blokace po 5 hrách s CTA na registraci
- [x] Guest conversion: převod progressu při registraci
- [x] Všechny texty z .resx
- [x] `dotnet test` → Backend testy zelené (18/18 ✅)
- [ ] Blazor testy: vyžadují Tempo.Blazor setup v bUnit
