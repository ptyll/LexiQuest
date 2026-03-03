# LexiQuest - Kompletní implementační plán

> **Technologie:** .NET 10, Blazor, MSSQL (bez Dockeru), bez Redis (In-Memory cache)

## Fáze 0: Základní setup (Příprava)

### T-000: Projektová struktura
- [ ] Vytvořit solution file
- [ ] Nastavit projekty: Api, Core, Infrastructure, Blazor, Shared
- [ ] Nastavit test projekty
- [ ] Nakonfigurovat NuGet package references
- [ ] ~~Vytvořit docker-compose.yml~~ (NE - vývoj bez Dockeru)
- [ ] Nastavit GitHub Actions CI/CD pipeline
- **Odhad:** 4h

### T-001: Databázová infrastruktura (MSSQL)
- [ ] Vytvořit DbContext
- [ ] Nastavit Entity Framework Core s MSSQL providerem
- [ ] Vytvořit migrace pro User, Word, GameSession
- [ ] Nastavit MSSQL connection string pro LocalDB
- [ ] Vytvořit seed data (základní slova)
- **Odhad:** 4h
- **Závislost:** T-000

### T-002: Autentizační infrastruktura
- [ ] Nastavit ASP.NET Core Identity
- [ ] Konfigurovat JWT authentication
- [ ] Vytvořit Auth middleware
- [ ] Nastavit refresh token mechanismus
- **Odhad:** 4h
- **Závislost:** T-001

### T-003: Resource soubory struktura
- [ ] Vytvořit .resx soubory pro všechny stránky (čeština)
- [ ] Nastavit IStringLocalizer v API i Blazor
- [ ] Vytvořit extension metody pro lokalizaci validací
- **Odhad:** 3h

### T-004: In-Memory Caching (místo Redis)
- [ ] Nastavit IMemoryCache
- [ ] Vytvořit CacheService wrapper
- [ ] Konfigurovat cache policies
- **Odhad:** 2h

---

## Fáze 1: MVP Core (Týden 1-2)

### T-100: UC-001 Registrace uživatele (TDD)
- [ ] Napsat testy pro Register endpoint
- [ ] Implementovat UserService.Register
- [ ] Implementovat RegisterRequestValidator (Fluent)
- [ ] Vytvořit Register endpoint
- [ ] Implementovat FE Register page
- [ ] Implementovat FE RegisterModelValidator
- [ ] Testovat integraci
- **Odhad:** 6h
- **Závislost:** T-002

### T-101: UC-002 Přihlášení uživatele (TDD)
- [ ] Napsat testy pro Login endpoint
- [ ] Implementovat LoginService
- [ ] Implementovat LoginRequestValidator
- [ ] Vytvořit Login endpoint
- [ ] Implementovat FE Login page
- [ ] Testovat integraci
- **Odhad:** 4h
- **Závislost:** T-100

### T-102: UC-004 Základní herní smyčka (TDD)
- [ ] Napsat testy pro GameSessionService
- [ ] Implementovat WordRepository
- [ ] Implementovat scramble algoritmus (Fisher-Yates)
- [ ] Implementovat StartGame endpoint
- [ ] Implementovat SubmitAnswer endpoint
- [ ] Implementovat XP calculation service
- [ ] Vytvořit FE GameArena komponentu
- [ ] Implementovat Timer komponentu
- [ ] Testovat integraci
- **Odhad:** 12h
- **Závislost:** T-101

### T-103: UC-005 Životy systém (TDD)
- [ ] Napsat testy pro LivesService
- [ ] Implementovat LiveRegenerationService (BackgroundService)
- [ ] Přidat lives logiku do GameSession
- [ ] Vytvořit LivesIndicator komponentu
- **Odhad:** 4h
- **Závislost:** T-102

### T-104: UC-006 XP a Level systém (TDD)
- [ ] Napsat testy pro LevelCalculator
- [ ] Implementovat XP calculation s bonusy
- [ ] Implementovat LevelUp detection
- [ ] Vytvořit XpBar komponentu
- [ ] Vytvořit LevelUp modal
- **Odhad:** 4h
- **Závislost:** T-102

### T-105: UC-007 Cesty - Backend (TDD)
- [ ] Napsat testy pro PathService
- [ ] Vytvořit Path, Level entities
- [ ] Implementovat PathRepository
- [ ] Vytvořit endpointy pro cesty
- **Odhad:** 6h
- **Závislost:** T-104

### T-106: UC-007 Cesty - Frontend
- [ ] Vytvořit PathSelector page
- [ ] Vytvořit PathMap komponentu
- [ ] Implementovat LevelNode komponentu (všechny stavy)
- [ ] Vytvořit LevelDetail modal
- **Odhad:** 6h
- **Závislost:** T-105

### T-107: UC-011 Streak systém (TDD)
- [ ] Napsat testy pro StreakService
- [ ] Implementovat Streak calculation
- [ ] Vytvořit StreakIndicator komponentu
- [ ] Implementovat Fire animation
- **Odhad:** 5h
- **Závislost:** T-101

### T-108: UC-015 Dashboard a Statistiky
- [ ] Vytvořit Dashboard endpoint
- [ ] Vytvořit Dashboard page
- [ ] Implementovat StatCards
- [ ] Vytvořit ActivityHeatmap komponentu
- **Odhad:** 6h
- **Závislost:** T-104, T-107

---

## Fáze 2: MVP Extended (Týden 3-4)

### T-200: UC-003 Obnova hesla (TDD)
- [ ] Napsat testy pro PasswordResetService
- [ ] Implementovat token generation
- [ ] Vytvořit email service (SendGrid/Mailgun)
- [ ] Implementovat endpointy
- [ ] Vytvořit FE pages
- **Odhad:** 5h

### T-201: UC-013 Ligy - Backend (TDD)
- [ ] Napsat testy pro LeagueService
- [ ] Vytvořit League, LeagueParticipant entities
- [ ] Implementovat LeagueAssignment algorithm
- [ ] Vytvořit WeeklyLeagueResetJob (Hangfire s MSSQL)
- [ ] Implementovat XP tracking pro ligy
- **Odhad:** 8h

### T-202: UC-013 Ligy - Frontend
- [ ] Vytvořit Leagues page
- [ ] Implementovat Leaderboard komponentu
- [ ] Vytvořit UserPosition card
- [ ] Implementovat Promotion/Demotion zones
- **Odhad:** 6h
- **Závislost:** T-201

### T-203: UC-014 Denní výzva (TDD)
- [ ] Vytvořit DailyChallengeService
- [ ] Implementovat DailyChallenge selection
- [ ] Vytvořit endpointy
- [ ] Vytvořit DailyChallenge page
- [ ] Implementovat Leaderboard pro denní výzvu
- **Odhad:** 5h

### T-204: UC-016 Achievementy - Backend (TDD)
- [ ] Vytvořit Achievement entity
- [ ] Implementovat AchievementService
- [ ] Vytvořit AchievementChecker (event-driven)
- [ ] Napojit na herní události
- **Odhad:** 6h

### T-205: UC-016 Achievementy - Frontend
- [ ] Vytvořit Achievements page
- [ ] Implementovat AchievementCard komponentu
- [ ] Vytvořit AchievementUnlock modal
- [ ] Implementovat Category tabs
- **Odhad:** 5h
- **Závislost:** T-204

### T-206: UC-008,009,010 Boss Levely (TDD)
- [ ] Rozšířit GameSession o Boss typy
- [ ] Implementovat MarathonBossRules
- [ ] Implementovat ConditionBossRules (zakázané písmeno)
- [ ] Implementovat TwistBossRules (odkrývání)
- [ ] Vytvořit BossLevel komponenty
- **Odhad:** 10h
- **Závislost:** T-106

### T-207: UC-017 Nastavení profilu
- [ ] Vytvořit UserSettings endpointy
- [ ] Vytvořit Settings page
- [ ] Implementovat Preference toggles
- [ ] Avatar upload (lokální filesystem)
- **Odhad:** 5h

### T-208: UI/UX Polishing
- [ ] Přidat Loading stavy (Skeletons)
- [ ] Implementovat Error boundaries
- [ ] Přidat Toast notifikace
- [ ] Animace přechodů mezi stránkami
- [ ] Mobile responsive úpravy
- **Odhad:** 6h

---

## Fáze 3: Landing Page & Guest Mode (Týden 5)

### T-300: UC-026 Landing Page
- [ ] Vytvořit Landing page komponentu
- [ ] Implementovat Hero section s animací
- [ ] Přidat Features section
- [ ] Přidat Testimonials section
- [ ] Přidat CTA sections
- [ ] Implementovat responsive design
- [ ] SEO optimalizace
- **Odhad:** 8h

### T-301: UC-027 Testovací hra bez registrace (Guest Mode) - Backend
- [ ] Vytvořit GuestSessionService (bez DB, jen in-memory/anonymous)
- [ ] Implementovat GuestLimiter (IP-based nebo cookie-based limit 3-5 her)
- [ ] Vytvořit GuestGameController
- [ ] Implementovat GuestProgress tracking (dočasný)
- [ ] Vytvořit endpoint pro převod Guest → Registered
- **Odhad:** 6h

### T-302: UC-027 Testovací hra bez registrace (Guest Mode) - Frontend
- [ ] Přidat "Hrát jako host" tlačítko na Landing page
- [ ] Vytvořit GuestGame flow (samoobslužný, bez loginu)
- [ ] Implementovat Guest limitations (max 3-5 her)
- [ ] Přidat CTA pro registraci po každé hře
- [ ] Implementovat "Převést progress" modal při registraci
- [ ] LocalStorage pro dočasný guest progress
- **Odhad:** 6h
- **Závislost:** T-301

---

## Fáze 4: Premium Features (Týden 6-7)

### T-400: UC-018 Premium účet - Backend
- [ ] Nastavit Stripe/PayPal integraci
- [ ] Vytvořit SubscriptionService
- [ ] Implementovat webhook handlers
- [ ] Premium feature flags
- **Odhad:** 8h

### T-401: UC-018 Premium účet - Frontend
- [ ] Vytvořit Premium landing page
- [ ] Implementovat Pricing cards
- [ ] Vytvořit Checkout flow
- [ ] Premium badge v profilu
- **Odhad:** 6h
- **Závislost:** T-400

### T-402: UC-012 Streak Shield a Freeze
- [ ] Rozšířit StreakService o Shield/Freeze
- [ ] Implementovat automatický Freeze
- [ ] Vytvořit UI pro Shield management
- **Odhad:** 4h
- **Závislost:** T-401

### T-403: UC-019 Obchod
- [ ] Vytvořit ShopItem entity
- [ ] Implementovat InventoryService
- [ ] Vytvořit Shop page
- [ ] Implementovat ItemCard komponentu
- **Odhad:** 6h

### T-404: UC-022 Vlastní slovníky (Premium)
- [ ] Vytvořit CustomDictionary entity
- [ ] Implementovat DictionaryService
- [ ] Word import (CSV/TXT)
- [ ] Vytvořit DictionaryBuilder UI
- **Odhad:** 6h
- **Závislost:** T-401

---

## Fáze 5: Multiplayer & Social (Týden 8-9)

### T-500: UC-020 Multiplayer 1v1 - Backend (SignalR)
- [ ] Nastavit SignalR
- [ ] Vytvořit MatchmakingService
- [ ] Implementovat GameHub
- [ ] Synchronizace herního stavu
- **Odhad:** 10h

### T-501: UC-020 Multiplayer 1v1 - Frontend
- [ ] Nastavit SignalR client
- [ ] Vytvořit Matchmaking screen
- [ ] Implementovat RealtimeGame komponentu
- [ ] Vytvořit MatchResult screen
- **Odhad:** 8h
- **Závislost:** T-500

### T-502: UC-021 Týmy a Klany
- [ ] Vytvořit Team entity
- [ ] Implementovat TeamService
- [ ] Team management endpointy
- [ ] Týmové žebříčky
- [ ] Vytvořit Team UI
- **Odhad:** 10h

---

## Fáze 6: Advanced Features (Týden 10-11)

### T-600: UC-023 Notifikace
- [ ] Nastavit Push notifikace (Firebase)
- [ ] Implementovat NotificationService
- [ ] Email notifikace (background jobs)
- [ ] Notification preferences UI
- **Odhad:** 6h

### T-601: UC-024 Admin panel
- [ ] Vytvořit Admin role
- [ ] Admin dashboard
- [ ] Word management CRUD
- [ ] User management
- [ ] Statistics export
- **Odhad:** 8h

### T-602: UC-025 AI Generované výzvy (Mock)
- [ ] Vytvořit AIChallengeService (mock)
- [ ] Personalizované challenge algoritmus
- [ ] AI Challenge UI
- **Odhad:** 4h

### T-603: Performance optimalizace
- [ ] Optimalizovat In-Memory caching strategie
- [ ] Database query optimalizace (MSSQL indexing)
- [ ] Image optimization
- [ ] Bundle size optimalizace
- **Odhad:** 6h

---

## Fáze 7: Testing & Deployment (Týden 12)

### T-700: Testing
- [ ] Unit test coverage > 80%
- [ ] Integration testy pro API
- [ ] E2E testy (Playwright)
- [ ] Load testing
- [ ] Security audit
- **Odhad:** 10h

### T-701: PWA
- [ ] Vytvořit service worker
- [ ] Manifest.json
- [ ] Offline support
- [ ] Push notifikace setup
- **Odhad:** 4h

### T-702: Production Deployment
- [ ] Azure/AWS infrastruktura
- [ ] CI/CD pipeline pro deployment
- [ ] Monitoring (Application Insights)
- [ ] Logging (Serilog)
- [ ] Backup strategie (MSSQL)
- **Odhad:** 6h

### T-703: Dokumentace
- [ ] API dokumentace (Swagger)
- [ ] Uživatelská dokumentace
- [ ] Deployment guide
- [ ] Troubleshooting guide
- **Odhad:** 4h

---

## Celkový odhad

| Fáze | Hodiny |
|------|--------|
| Fáze 0: Setup | 17h |
| Fáze 1: MVP Core | 53h |
| Fáze 2: MVP Extended | 56h |
| Fáze 3: Landing & Guest | 20h |
| Fáze 4: Premium | 30h |
| Fáze 5: Multiplayer | 28h |
| Fáze 6: Advanced | 24h |
| Fáze 7: Testing & Deployment | 24h |
| **Celkem** | **252h** |
| **Buffer (20%)** | **50h** |
| **Celkem s bufferem** | **302h** |

Při 6h práce denně: **50 pracovních dnů = ~10-11 týdnů**

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
