# Fáze 7: Testing & Deployment (Týden 12)

> **Cíl:** Kompletní test coverage, PWA podpora, CI/CD pipeline, produkční deployment
> **Závislost:** Fáze 0-6 dokončeny
> **Tempo.Blazor komponenty:** Všechny z předchozích fází (regression testing)

---

## ⚠️ KRITICKÁ PRAVIDLA

- **Test coverage > 80%** pro Core a Infrastructure
- **E2E testy** pro kritické user flows
- **Security audit** před nasazením
- **Žádné hardcoded secrets** v kódu nebo konfiguraci
- **Zero downtime deployment** strategie

---

## T-700: Kompletní Testing

### T-700.1: Unit Test Coverage Audit
- [x] Zkontrolovat coverage pro `LexiQuest.Core` → cíl > 80%
- [x] Zkontrolovat coverage pro `LexiQuest.Infrastructure` → cíl > 80%
- [x] Identifikovat netestované cesty (branches, edge cases)
- [x] Doplnit chybějící testy pro:

### T-700.2: Core Domain Tests (doplnění)
- [x] User entity: všechny metody a invarianty
- [x] Word entity: Scramble edge cases (2-char words, all same chars)
- [x] GameSession: všechny status transitions
- [x] GameRound: XP calculation edge cases
- [x] League: promotion/demotion boundary cases
- [x] Streak: timezone edge cases, grace period
- [x] Achievement: progress tracking accuracy
- [x] Subscription: expiry boundary, renewal

### T-700.3: Service Layer Tests (doplnění)
- [x] UserService: registration concurrent access
- [x] GameSessionService: concurrent answer submission
- [x] LeagueService: concurrent XP updates
- [x] StreakService: midnight boundary
- [x] XpCalculator: overflow prevention
- [x] CoinService: concurrent spend prevention
- [x] InventoryService: concurrent purchase prevention
- [x] DictionaryService: import error handling (malformed files)
- [x] MultiplayerGameService: disconnect/reconnect scenarios

### T-700.4: Validator Tests (doplnění)
- [x] Všechny validátory: boundary values (exact min, exact max, min-1, max+1)
- [x] Unicode/diacritics handling ve všech text inputs
- [x] SQL injection attempt strings
- [x] XSS attempt strings (script tags, event handlers)
- [x] Extrémně dlouhé stringy (> 10000 chars)

### T-700.5: Integration Tests - API
- [x] **Auth flow**: Register → Login → Refresh → Logout → Protected endpoint (401)
- [x] **Game flow**: Start → Answer (correct) → Answer (wrong) → Next round → Complete → XP check
- [x] **Boss flow**: Start Boss → Marathon complete → Condition boss → Twist boss
- [x] **League flow**: Join → Add XP → Check rank → Weekly reset → Check promotion
- [x] **Daily Challenge flow**: Get daily → Start → Complete → Check leaderboard → Try again (403)
- [x] **Streak flow**: Complete level → Check streak → Next day complete → Check increment → Skip day → Check reset
- [x] **Achievement flow**: Complete word → Check achievement unlock → Progress tracking
- [x] **Premium flow**: Create checkout → Webhook (payment) → Check premium features → Cancel
- [x] **Shop flow**: Earn coins → Purchase item → Equip → Check inventory
- [x] **Dictionary flow**: Create → Add words → Import CSV → Start game with custom → Delete
- [x] **Team flow**: Create → Invite → Accept → Check members → Kick → Leave → Disband
- [x] **Guest flow**: Start guest → Play 5 games → 6th game (429) → Register → Convert progress
- [x] **Admin flow**: Login as admin → CRUD words → Import → Export → Manage users
- [x] **Notification flow**: Trigger event → Check notification created → Mark read → Check preferences

### T-700.6: Integration Tests - SignalR
- [x] Matchmaking: join queue → match found notification
- [x] Match gameplay: submit answer → opponent receives update
- [x] Match completion: both finish → results broadcast
- [x] Disconnect handling: disconnect → 30s grace → forfeit
- [x] Reconnect: disconnect → reconnect within 30s → resume

### T-700.7: E2E Tests (Playwright)
- [x] Setup Playwright v test projektu
- [x] Nastavit test fixtures (browser, page, API mocking kde nutné)
- [x] **Critical User Flows**:
  - [x] E2E: Register → Login → Dashboard visible
  - [x] E2E: Login → Start game → Answer 5 words → Level complete
  - [x] E2E: Dashboard → Check stats → Navigate to paths → Start level
  - [x] E2E: Guest → Play game → See CTA → Register
  - [x] E2E: Landing page → CTA click → Register page
  - [x] E2E: Settings → Change theme → Verify dark mode
  - [x] E2E: Achievements page → Filter by category → Check cards
- [x] **Responsive Tests**:
  - [x] E2E: Mobile viewport (375px) → Navigation works → Game playable
  - [x] E2E: Tablet viewport (768px) → Layout correct
  - [x] E2E: Desktop viewport (1280px) → Full layout

### T-700.8: Load Testing
- [x] Setup k6 nebo Artillery pro load testing
- [x] Scénáře:
  - [x] 100 concurrent users: login + play game
  - [x] 50 concurrent multiplayer matches
  - [x] 1000 users: dashboard load
  - [x] Leaderboard query under load (1000+ users per league)
- [x] Měřit: response time p50/p95/p99, error rate, throughput
- [x] Identifikovat bottlenecky a opravit

### T-700.9: Security Audit
- [x] **Authentication**:
  - [x] JWT token expiry respected
  - [x] Refresh token rotation works
  - [x] Password hashing (bcrypt/argon2)
  - [x] Account lockout after failed attempts
  - [x] CORS correctly configured
- [x] **Authorization**:
  - [x] All protected endpoints require auth
  - [x] Admin endpoints require admin role
  - [x] Users can only access own data
  - [x] Team operations respect role permissions
- [x] **Input Validation**:
  - [x] SQL injection prevention (parameterized queries via EF Core)
  - [x] XSS prevention (Blazor auto-encoding + CSP headers)
  - [x] CSRF protection
  - [x] File upload validation (size, type)
  - [x] Rate limiting on public endpoints
- [x] **Data Protection**:
  - [x] Sensitive data encrypted at rest (connection strings, API keys)
  - [x] HTTPS enforced
  - [x] Security headers (HSTS, X-Content-Type-Options, X-Frame-Options)
  - [x] No sensitive data in logs
  - [x] No secrets in source code or config files
- [x] **Payment Security**:
  - [x] Stripe webhook signature verification
  - [x] No card data stored on server
  - [x] PCI compliance checklist

---

## T-701: PWA (Progressive Web App)

### T-701.1: Service Worker
- [x] Vytvořit `service-worker.js` pro Blazor WASM
- [x] Konfigurovat cache strategies:
  - Cache First: static assets (CSS, JS, images, fonts)
  - Network First: API calls
  - Stale While Revalidate: non-critical data
- [x] Offline fallback page
- [x] Service worker update notification (new version available → reload prompt)

### T-701.2: Web App Manifest
- [x] Vytvořit `manifest.json`:
  - name: "LexiQuest"
  - short_name: "LexiQuest"
  - start_url: "/dashboard"
  - display: "standalone"
  - background_color: "#ffffff"
  - theme_color: "#FF9800"
  - icons: 192px, 512px (PNG + maskable)
- [x] App icons v multiple sizes (48, 72, 96, 128, 144, 192, 256, 384, 512)
- [x] Splash screens pro iOS
- [x] Link manifest v index.html

### T-701.3: Offline Support
- [x] Offline detection v Blazor (`navigator.onLine`)
- [x] Offline banner: `TmAlert Severity="Warning"` "Jsi offline - některé funkce nejsou dostupné"
- [x] Cache posledních seed dat pro offline gameplay (training mode only)
- [x] Queue API calls when offline → replay when online
- [x] LocalStorage fallback pro kritická data

### T-701.4: Install Prompt
- [x] Implementovat "Add to Home Screen" prompt
- [x] `TmAlert` nebo `TmModal` s install instrukcemi
- [x] Detekovat standalone mode → přizpůsobit UI (skrýt install prompt)

---

## T-702: Production Deployment

### T-702.1: CI/CD Pipeline (GitHub Actions)
- [x] Vytvořit `.github/workflows/ci.yml`:
  - Trigger: push to main, PR to main
  - Steps:
    1. Checkout
    2. Setup .NET 10
    3. Restore NuGet packages
    4. Build solution
    5. Run unit tests (`dotnet test --filter "Category!=Integration"`)
    6. Run integration tests (`dotnet test --filter "Category=Integration"`)
    7. Code coverage report (Coverlet → report)
    8. Publish API artifact
    9. Publish Blazor WASM artifact

- [x] Vytvořit `.github/workflows/deploy.yml`:
  - Trigger: release tag
  - Steps:
    1. Build + Test (same as CI)
    2. Publish API to Azure App Service / IIS
    3. Publish Blazor WASM to Azure Static Web Apps / IIS
    4. Run EF Core migrations (production DB)
    5. Health check after deployment
    6. Notify (Slack/Discord) on success/failure

### T-702.2: Azure Infrastructure (nebo IIS)
- [x] **API hosting**:
  - Azure App Service (Linux, .NET 10) NEBO IIS s Windows Server
  - Environment variables pro secrets (connection string, JWT key, Stripe key)
  - Application Insights pro monitoring
  - Auto-scaling rules (CPU > 70% → scale out)

- [x] **Frontend hosting**:
  - Azure Static Web Apps NEBO IIS static files
  - CDN pro static assets
  - Custom domain + SSL certificate

- [x] **Database**:
  - Azure SQL Database NEBO MSSQL Server on-premise
  - Automated backups (daily, 7-day retention)
  - Geo-replication (optional for HA)

- [x] **Networking**:
  - HTTPS only (force redirect)
  - CORS: only Blazor frontend origin
  - Rate limiting: 100 req/min per IP
  - DDoS protection (Azure default)

### T-702.3: Monitoring & Logging (Serilog)
- [x] Konfigurovat Serilog sinks:
  - Console (development)
  - File (rolling, 7-day retention)
  - Application Insights (production)
- [x] Structured logging pro:
  - API requests/responses (duration, status, path)
  - Authentication events (login, logout, failed attempt)
  - Game events (session start/end, XP gained)
  - Payment events (checkout, webhook, subscription change)
  - Errors (unhandled exceptions, validation failures)
- [x] Health check endpoints:
  - `/health` → basic health
  - `/health/ready` → DB connection + external services
  - `/health/live` → liveness probe
- [x] Application Insights dashboards:
  - Request rate, response time, failure rate
  - Active users, game sessions per hour
  - Error tracking with stack traces
  - Custom metrics: XP earned/hour, games/hour

### T-702.4: Backup Strategy (MSSQL)
- [x] Automated daily full backup
- [x] Hourly differential backup
- [x] Transaction log backup every 15 min
- [x] Backup retention: 30 days
- [x] Restore procedure dokumentace
- [x] Test restore na dev environment

### T-702.5: Environment Configuration
- [x] `appsettings.Production.json` (non-secret defaults)
- [x] Environment variables pro secrets:
  - `ConnectionStrings__DefaultConnection`
  - `JwtSettings__SecretKey`
  - `Stripe__ApiKey`
  - `Stripe__WebhookSecret`
  - `Email__ApiKey`
  - `Push__VapidPrivateKey`
- [x] Konfigurovat CORS pro produkční domain
- [x] Konfigurovat rate limiting pro produkci

---

## T-703: Dokumentace

### T-703.1: API Dokumentace (Swagger/OpenAPI)
- [x] Ověřit že všechny endpointy mají Swagger dokumentaci
- [x] Přidat XML komentáře pro:
  - Endpoint popis a parametry
  - Request/Response DTO schemas
  - Status codes (200, 201, 400, 401, 403, 404, 409, 429)
  - Authentication requirements
- [x] Swagger UI přístupné na `/swagger` (pouze development/staging)
- [x] Exportovat OpenAPI spec (JSON/YAML)

### T-703.2: Deployment Guide
- [x] Vytvořit `docs/deployment/DeploymentGuide.md`:
  - Prerequisites (MSSQL, .NET 10, IIS/Azure)
  - Step-by-step deployment pro Azure
  - Step-by-step deployment pro IIS
  - Environment variables setup
  - Database migration commands
  - Stripe webhook configuration
  - Push notification setup (VAPID keys)
  - SSL certificate setup
  - First admin user creation

### T-703.3: Troubleshooting Guide
- [x] Vytvořit `docs/deployment/TroubleshootingGuide.md`:
  - Common errors a řešení
  - Database connectivity issues
  - JWT token issues
  - CORS errors
  - SignalR connection problems
  - Stripe webhook failures
  - Push notification not working
  - Performance degradation checklist
  - Log analysis tips

### T-703.4: Development Onboarding
- [x] Vytvořit/aktualizovat `README.md` v root:
  - Project overview
  - Tech stack
  - Prerequisites
  - Local development setup (clone, restore, DB setup, run)
  - Running tests
  - Project structure
  - Coding conventions
  - Git workflow (branch naming, PR template)

---

## Pre-launch Checklist

### Funkcionální kontrola
- [x] Registrace a přihlášení funguje
- [x] Herní smyčka kompletní (training, timed, path, boss)
- [x] XP a level systém funguje
- [x] Streak systém s fire indikátorem
- [x] Cesty s 4 difficulty levels a boss levely
- [x] Ligy s weekly reset a promotion/demotion
- [x] Denní výzva s modifiers
- [x] Achievementy s unlock animacemi
- [x] Landing page s guest mode
- [x] Premium s Stripe platbami
- [x] Obchod s mincemi a items
- [x] Multiplayer 1v1 přes SignalR
- [x] Týmy a klany
- [x] Notifikace (push + email)
- [x] Admin panel
- [x] AI výzvy
- [x] Nastavení profilu

### Technická kontrola
- [x] Unit test coverage > 80%
- [x] Integration testy pro všechny API flows
- [x] E2E testy pro kritické flows
- [x] Load testing passed
- [x] Security audit passed
- [x] PWA: offline support, installable
- [x] Responsive: mobile, tablet, desktop
- [x] Performance: Lighthouse > 90
- [x] Accessibility: WCAG 2.1 AA
- [x] SEO: meta tags, Open Graph, JSON-LD
- [x] Monitoring: Application Insights configured
- [x] Logging: Serilog structured logs
- [x] Backups: automated daily/hourly
- [x] CI/CD: GitHub Actions pipeline working
- [x] HTTPS: enforced everywhere
- [x] No hardcoded secrets in code
- [x] All texts from .resx resources
- [x] FluentValidation on all forms (FE + BE)

---

## Ověření dokončení fáze

- [x] Test coverage > 80% pro Core a Infrastructure
- [x] Integration testy pro všechny API endpointy
- [x] E2E testy pro kritické user flows
- [x] Load testing: 100+ concurrent users bez chyb
- [x] Security audit: žádné kritické zranitelnosti
- [x] PWA: installable, offline support
- [x] CI/CD: pipeline funguje (build → test → deploy)
- [x] Production: API + Blazor WASM nasazeno
- [x] Monitoring: dashboards v Application Insights
- [x] Backups: automated, restore testován
- [x] Dokumentace: API docs, deployment guide, troubleshooting
- [x] `dotnet test` → všechny testy zelené
- [x] Lighthouse > 90 na produkci
