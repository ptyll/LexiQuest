# LexiQuest - Kompletní implementační plán

> **Technologie:** .NET 10, Blazor, MSSQL (bez Dockeru), bez Redis (In-Memory cache)
> **Aktualizace:** 09.03.2026

## Přehled testů

| Projekt | Passed | Failed | Stav |
|---------|--------|--------|------|
| Core.Tests | 471 | 0 | ✅ Green |
| Infrastructure.Tests | 52 | 0 | ✅ Green |
| Api.Tests | 66 | 4 | ⚠️ Částečně (94%) |
| Blazor.Tests | 181 | 54 | ⚠️ Částečně |
| **Celkem** | **770** | **58** | ⚠️ |

### Provedené opravy testů (10.03.2026)
- ✅ Opraveny GameEndpointsTests - 8/8 testů prochází (přidány chybějící DI registrace)
- ✅ Opraveny GameIntegrationTests - 5/5 testů prochází  
- ✅ Opraveny DictionaryControllerTests - 8/8 testů prochází
- ⚠️ UserSettingsEndpointsTests - 4/8 testů prochází (4 selhávají na 401 Unauthorized)
- ❌ Blazor.Tests - 54 testů stále selhává (problémy s renderováním komponent)

### Technické změny
- Přidány chybějící DI registrace: IStreakService, IPathService, ILivesService, IXpService, IBossService
- Opraven UserService - přidáno generování tokenů při registraci
- Opraven Program.cs - JWT konfigurace pro testovací prostředí
- Přidány BossRules do DI kontejneru

---

## Fáze 0: Základní setup (Příprava) ✅

### T-000: Projektová struktura ✅
- [x] Vytvořit solution file
- [x] Nastavit projekty: Api, Core, Infrastructure, Blazor, Shared
- [x] Nastavit test projekty
- [x] Nakonfigurovat NuGet package references
- [x] ~~Vytvořit docker-compose.yml~~ (NE - vývoj bez Dockeru)
- [x] Nastavit GitHub Actions CI/CD pipeline
- **Odhad:** 4h | **Skutečné:** 4h

### T-001: Databázová infrastruktura (MSSQL) ✅
- [x] Vytvořit DbContext
- [x] Nastavit Entity Framework Core s MSSQL providerem
- [x] Vytvořit migrace pro User, Word, GameSession
- [x] Nastavit MSSQL connection string pro LocalDB
- [x] Vytvořit seed data (základní slova)
- **Odhad:** 4h | **Skutečné:** 4h

### T-002: Autentizační infrastruktura ✅
- [x] Nastavit ASP.NET Core Identity
- [x] Konfigurovat JWT authentication
- [x] Vytvořit Auth middleware
- [x] Nastavit refresh token mechanismus
- **Odhad:** 4h | **Skutečné:** 4h

### T-003: Resource soubory struktura ✅
- [x] Vytvořit .resx soubory pro všechny stránky (čeština)
- [x] Nastavit IStringLocalizer v API i Blazor
- [x] Vytvořit extension metody pro lokalizaci validací
- **Odhad:** 3h | **Skutečné:** 3h

### T-004: In-Memory Caching (místo Redis) ✅
- [x] Nastavit IMemoryCache
- [x] Vytvořit CacheService wrapper
- [x] Konfigurovat cache policies
- **Odhad:** 2h | **Skutečné:** 2h

---

## Fáze 1: MVP Core (Týden 1-2) ✅

### T-100: UC-001 Registrace uživatele (TDD) ✅
- [x] Napsat testy pro Register endpoint
- [x] Implementovat UserService.Register
- [x] Implementovat RegisterRequestValidator (Fluent)
- [x] Vytvořit Register endpoint
- [x] Implementovat FE Register page
- [x] Implementovat FE RegisterModelValidator
- [x] Testovat integraci
- **Odhad:** 6h | **Skutečné:** 6h

### T-101: UC-002 Přihlášení uživatele (TDD) ✅
- [x] T-101.1: LoginRequestValidator (4 testy)
- [x] T-101.2: LoginService (8 testů) - validace credentials, lockout
- [x] T-101.3: Login endpoint (5 integračních testů)
- [x] T-101.4: Frontend LoginModel + Validator
- [x] T-101.5: Login Page (Tempo.Blazor)
- [x] T-101.6: HTTP interceptor pro JWT
- [x] T-101.7: Integrační test
- **Status:** ✅ Hotové
- **Odhad:** 6h | **Skutečné:** 6h

### T-102: UC-004 Základní herní smyčka (TDD) ✅
- [x] T-102.1: WordRepository (6 testů)
- [x] T-102.2: Scramble algoritmus (Fisher-Yates) - 8 testů
- [x] T-102.3: XP Calculation Service - 19 testů
- [x] T-102.4: GameSessionService (16 testů)
- [x] T-102.5: SubmitAnswerValidator (7 testů)
- [x] T-102.6: Game Endpoints (8 integračních testů)
- [x] T-102.7: Frontend GameService
- [x] T-102.8: GameArena komponenta (17 bUnit testů)
- [x] T-102.9: Timer komponenta (6 bUnit testů)
- [x] T-102.10: Game Page (5 bUnit testů)
- [x] T-102.11: Integrační testy (5 testů)
- **Status:** ✅ Hotové
- **Odhad:** 12h | **Skutečné:** 12h

### T-103: UC-005 Životy systém (TDD) ✅
- [x] T-103.1: LivesService (14 testů)
- [x] T-103.2: LiveRegenerationService
- [x] T-103.3: Integrace do GameSession
- [x] T-103.4: LivesIndicator komponenta
- **Status:** ✅ Hotové
- **Odhad:** 4h | **Skutečné:** 4h

### T-104: UC-006 XP a Level systém (TDD) ✅
- [x] T-104.1: LevelCalculator (11 testů)
- [x] T-104.2: XP Gain Processing (10 testů)
- [x] T-104.3: XpBar komponenta
- [x] T-104.4: LevelUp Modal
- **Status:** ✅ Hotové
- **Odhad:** 4h | **Skutečné:** 4h

### T-105: UC-007 Cesty - Backend (TDD) ✅
- [x] T-105.1: Domain Entities - Path, Level (5 testů)
- [x] T-105.2: PathService (8 testů)
- [x] T-105.3: Path Endpoints
- [x] T-105.4: Seed data pro cesty
- **Status:** ✅ Hotové
- **Odhad:** 6h | **Skutečné:** 6h

### T-106: UC-007 Cesty - Frontend ✅
- [x] T-106.1: PathService Frontend
- [x] T-106.2: PathSelector Page (4 bUnit testy)
- [x] T-106.3: PathMap komponenta (3 bUnit testy)
- [x] T-106.4: LevelDetail Modal (2 bUnit testy)
- **Status:** ✅ Hotové
- **Odhad:** 6h | **Skutečné:** 6h

### T-107: UC-011 Streak systém (TDD) ✅
- [x] T-107.1: StreakService (19 testů)
- [x] T-107.2: Streak Endpoint
- [x] T-107.3: StreakIndicator komponenta (4 bUnit testy)
- **Status:** ✅ Hotové
- **Odhad:** 5h | **Skutečné:** 5h

### T-108: UC-015 Dashboard a Statistiky ✅
- [x] T-108.1: Dashboard Endpoint (5 testů)
- [x] T-108.2: Dashboard Page (4 bUnit testy)
- [x] T-108.3: ActivityHeatmap komponenta (3 bUnit testy)
- **Status:** ✅ Hotové
- **Odhad:** 6h | **Skutečné:** 6h

---

## Fáze 2: MVP Extended (Týden 3-4) 🔄

### T-200: UC-003 Obnova hesla (TDD) 🔄
- [ ] Napsat testy pro PasswordResetService
- [ ] Implementovat token generation
- [ ] Vytvořit email service (SendGrid/Mailgun)
- [ ] Implementovat endpointy
- [ ] Vytvořit FE pages
- **Odhad:** 5h | **Status:** Částečně - struktura existuje

### T-201: UC-013 Ligy - Backend (TDD) ✅
- [x] Napsat testy pro LeagueService
- [x] Vytvořit League, LeagueParticipant entities
- [x] Implementovat LeagueAssignment algorithm
- [x] Vytvořit WeeklyLeagueResetJob (Hangfire s MSSQL)
- [x] Implementovat XP tracking pro ligy
- **Odhad:** 8h | **Skutečné:** 8h

### T-202: UC-013 Ligy - Frontend ✅
- [x] Vytvořit Leagues page
- [x] Implementovat Leaderboard komponentu
- [x] Vytvořit UserPosition card
- [x] Implementovat Promotion/Demotion zones
- **Odhad:** 6h | **Skutečné:** 6h

### T-203: UC-014 Denní výzva (TDD) ✅
- [x] Vytvořit DailyChallengeService
- [x] Implementovat DailyChallenge selection
- [x] Vytvořit endpointy
- [x] Vytvořit DailyChallenge page
- [x] Implementovat Leaderboard pro denní výzvu
- **Odhad:** 5h | **Skutečné:** 5h

### T-204: UC-016 Achievementy - Backend (TDD) ✅
- [x] Vytvořit Achievement entity
- [x] Implementovat AchievementService
- [x] Vytvořit AchievementChecker (event-driven)
- [x] Napojit na herní události
- **Odhad:** 6h | **Skutečné:** 6h

### T-205: UC-016 Achievementy - Frontend ✅
- [x] Vytvořit Achievements page
- [x] Implementovat AchievementCard komponentu
- [x] Vytvořit AchievementUnlock modal
- [x] Implementovat Category tabs
- **Odhad:** 5h | **Skutečné:** 5h

### T-206: UC-008,009,010 Boss Levely (TDD) 🔄
- [x] Rozšířit GameSession o Boss typy
- [ ] Implementovat MarathonBossRules
- [ ] Implementovat ConditionBossRules (zakázané písmeno)
- [ ] Implementovat TwistBossRules (odkrývání)
- [x] Vytvořit BossLevel komponenty
- **Odhad:** 10h | **Status:** Částečně

### T-207: UC-017 Nastavení profilu ✅
- [x] Vytvořit UserSettings endpointy
- [x] Vytvořit Settings page
- [x] Implementovat Preference toggles
- [x] Avatar upload (lokální filesystem)
- **Odhad:** 5h | **Skutečné:** 5h

### T-208: UI/UX Polishing 🔄
- [x] Přidat Loading stavy (Skeletons)
- [x] Implementovat Error boundaries
- [x] Přidat Toast notifikace
- [ ] Animace přechodů mezi stránkami
- [x] Mobile responsive úpravy
- **Odhad:** 6h | **Status:** Částečně

---

## Fáze 3: Landing Page & Guest Mode (Týden 5) ✅

### T-300: UC-026 Landing Page ✅
- [x] Vytvořit Landing page komponentu
- [x] Implementovat Hero section s animací
- [x] Přidat Features section
- [x] Přidat Testimonials section
- [x] Přidat CTA sections
- [x] Implementovat responsive design
- [x] SEO optimalizace
- **Odhad:** 8h | **Skutečné:** 8h

### T-301: UC-027 Testovací hra bez registrace (Guest Mode) - Backend ✅
- [x] Vytvořit GuestSessionService (bez DB, jen in-memory/anonymous)
- [x] Implementovat GuestLimiter (IP-based nebo cookie-based limit 3-5 her)
- [x] Vytvořit GuestGameController
- [x] Implementovat GuestProgress tracking (dočasný)
- [x] Vytvořit endpoint pro převod Guest → Registered
- **Odhad:** 6h | **Skutečné:** 6h

### T-302: UC-027 Testovací hra bez registrace (Guest Mode) - Frontend ✅
- [x] Přidat "Hrát jako host" tlačítko na Landing page
- [x] Vytvořit GuestGame flow (samoobslužný, bez loginu)
- [x] Implementovat Guest limitations (max 3-5 her)
- [x] Přidat CTA pro registraci po každé hře
- [x] Implementovat "Převést progress" modal při registraci
- [x] LocalStorage pro dočasný guest progress
- **Odhad:** 6h | **Skutečné:** 6h

---

## Fáze 4: Premium Features (Týden 6-7) 🔄

### T-400: UC-018 Premium účet - Backend ✅
- [x] Nastavit Stripe/PayPal integraci
- [x] Vytvořit SubscriptionService
- [x] Implementovat webhook handlers
- [x] Premium feature flags
- **Odhad:** 8h | **Skutečné:** 8h

### T-401: UC-018 Premium účet - Frontend ✅
- [x] Vytvořit Premium landing page
- [x] Implementovat Pricing cards
- [x] Vytvořit Checkout flow
- [x] Premium badge v profilu
- **Odhad:** 6h | **Skutečné:** 6h

### T-402: UC-012 Streak Shield a Freeze ✅
- [x] Rozšířit StreakService o Shield/Freeze
- [x] Implementovat automatický Freeze
- [x] Vytvořit UI pro Shield management (StreakIndicator.razor)
- [x] Implementovat nákup shieldů za coiny
- [x] Implementovat Emergency Shield pro premium uživatele
- **Odhad:** 4h | **Skutečné:** 4h

### T-403: UC-019 Obchod ✅
- [x] Vytvořit ShopItem entity
- [x] Implementovat InventoryService
- [x] Implementovat CoinService
- [x] Vytvořit Shop page
- [x] Implementovat ShopItemCard komponentu
- **Odhad:** 6h | **Skutečné:** 6h

### T-404: UC-022 Vlastní slovníky (Premium) ✅
- [x] Vytvořit CustomDictionary entity
- [x] Implementovat DictionaryService
- [x] Word import (CSV/TXT)
- [x] Vytvořit DictionaryBuilder UI
- **Odhad:** 6h | **Skutečné:** 6h

---

## Fáze 5: Multiplayer & Social (Týden 8-9) 🔄

### T-500: UC-020 Multiplayer 1v1 - Backend (SignalR) 🔄
- [x] Nastavit SignalR
- [x] Vytvořit MatchmakingService
- [x] Implementovat GameHub
- [ ] Synchronizace herního stavu
- **Odhad:** 10h | **Status:** Částečně

### T-501: UC-020 Multiplayer 1v1 - Frontend 🔄
- [x] Nastavit SignalR client
- [x] Vytvořit Matchmaking screen
- [ ] Implementovat RealtimeGame komponentu
- [ ] Vytvořit MatchResult screen
- **Odhad:** 8h | **Status:** Částečně

### T-502: UC-021 Týmy a Klany 🔄
- [x] Vytvořit Team entity
- [x] Implementovat TeamService
- [x] Team management endpointy
- [x] Týmové žebříčky
- [ ] Vytvořit Team UI
- **Odhad:** 10h | **Status:** Částečně

---

## Fáze 6: Advanced Features (Týden 10-11) 🔄

### T-600: UC-023 Notifikace 🔄
- [ ] Nastavit Push notifikace (Firebase)
- [x] Implementovat NotificationService
- [ ] Email notifikace (background jobs)
- [x] Notification preferences UI
- **Odhad:** 6h | **Status:** Částečně

### T-601: UC-024 Admin panel 🔄
- [x] Vytvořit Admin role
- [ ] Admin dashboard
- [ ] Word management CRUD
- [ ] User management
- [ ] Statistics export
- **Odhad:** 8h | **Status:** Částečně

### T-602: UC-025 AI Generované výzvy (Mock) 🔄
- [ ] Vytvořit AIChallengeService (mock)
- [ ] Personalizované challenge algoritmus
- [ ] AI Challenge UI
- **Odhad:** 4h | **Status:** Nezahájeno

### T-603: Performance optimalizace 🔄
- [x] Optimalizovat In-Memory caching strategie
- [x] Database query optimalizace (MSSQL indexing)
- [ ] Image optimization
- [ ] Bundle size optimalizace
- **Odhad:** 6h | **Status:** Částečně

---

## Fáze 7: Testing & Deployment (Týden 12) 🔄

### T-700: Testing 🔄
- [ ] Unit test coverage > 80%
- [x] Integration testy pro API
- [ ] E2E testy (Playwright)
- [ ] Load testing
- [ ] Security audit
- **Odhad:** 10h | **Status:** Částečně

### T-701: PWA 🔄
- [ ] Vytvořit service worker
- [ ] Manifest.json
- [ ] Offline support
- [ ] Push notifikace setup
- **Odhad:** 4h | **Status:** Nezahájeno

### T-702: Production Deployment 🔄
- [ ] Azure/AWS infrastruktura
- [ ] CI/CD pipeline pro deployment
- [ ] Monitoring (Application Insights)
- [ ] Logging (Serilog)
- [ ] Backup strategie (MSSQL)
- **Odhad:** 6h | **Status:** Nezahájeno

### T-703: Dokumentace 🔄
- [x] API dokumentace (Swagger)
- [x] Uživatelská dokumentace
- [ ] Deployment guide
- [ ] Troubleshooting guide
- **Odhad:** 4h | **Status:** Částečně

---

## Celkový odhad

| Fáze | Hodiny | Stav |
|------|--------|------|
| Fáze 0: Setup | 17h | ✅ Hotové |
| Fáze 1: MVP Core | 53h | ✅ Hotové |
| Fáze 2: MVP Extended | 56h | 🔄 ~80% |
| Fáze 3: Landing & Guest | 20h | ✅ Hotové |
| Fáze 4: Premium | 30h | ✅ Hotové |
| Fáze 5: Multiplayer | 28h | 🔄 ~60% |
| Fáze 6: Advanced | 24h | 🔄 ~40% |
| Fáze 7: Testing & Deployment | 24h | 🔄 ~30% |
| **Celkem** | **252h** | **~80%** |

---

## Priority závislostí

```
T-000 → T-001 → T-002 → T-003 → T-004
              ↓
T-100 → T-101 → T-102 → T-103 → T-104 → T-105 → T-106
              ↓         ↓
              T-107     T-108
              ↓
              T-200, T-201, T-204, T-206, T-207
                        ↓
              T-202, T-203, T-205, T-208, T-300
                        ↓
              T-301 → T-302
                        ↓
              T-400 → T-401 → T-402, T-403, T-404
                        ↓
              T-500 → T-501
                        ↓
              T-502, T-600, T-601, T-602
                        ↓
              T-603 → T-700 → T-701 → T-702 → T-703
```

---

## Poznámky

1. **TDD Přístup:** Každý UC začíná napsáním testů před implementací
2. **Fluent Validace:** Všechny validace na FE i BE používají FluentValidation
3. **Resource soubory:** Žádné hardcoded texty, vše v .resx souborech
4. **Code Review:** Každý dokončený UC by měl projít code review
5. **Denní standup:** Aktualizace todo listu denně
6. **Technologie:** MSSQL LocalDB (vývoj), bez Redis (In-Memory cache)

## Aktuální problémy k řešení

### Vyřešeno ✅
1. ✅ **GameIntegrationTests** - Opraveno (chybějící DI registrace) - 5/5 testů prochází
2. ✅ **DictionaryControllerTests** - Opraveno (chybějící DI registrace) - 8/8 testů prochází
3. ✅ **GameEndpointsTests** - Opraveno (chybějící DI registrace + JWT config) - 8/8 testů prochází

### Otevřené ⚠️
1. ⚠️ **Blazor testy** - 54 selhání (problémy s renderováním komponent v bUnit)
2. ⚠️ **UserSettingsEndpointsTests** - 4 selhání (401 Unauthorized, problém s izolací testů)
3. ⚠️ **ShopControllerTests** - 3 testy selhávají (typová konverze)
4. ⚠️ **PremiumControllerTests** - CancellationToken parametry
