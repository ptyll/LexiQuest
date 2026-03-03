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
- [ ] Zkontrolovat coverage pro `LexiQuest.Core` → cíl > 80%
- [ ] Zkontrolovat coverage pro `LexiQuest.Infrastructure` → cíl > 80%
- [ ] Identifikovat netestované cesty (branches, edge cases)
- [ ] Doplnit chybějící testy pro:

### T-700.2: Core Domain Tests (doplnění)
- [ ] User entity: všechny metody a invarianty
- [ ] Word entity: Scramble edge cases (2-char words, all same chars)
- [ ] GameSession: všechny status transitions
- [ ] GameRound: XP calculation edge cases
- [ ] League: promotion/demotion boundary cases
- [ ] Streak: timezone edge cases, grace period
- [ ] Achievement: progress tracking accuracy
- [ ] Subscription: expiry boundary, renewal

### T-700.3: Service Layer Tests (doplnění)
- [ ] UserService: registration concurrent access
- [ ] GameSessionService: concurrent answer submission
- [ ] LeagueService: concurrent XP updates
- [ ] StreakService: midnight boundary
- [ ] XpCalculator: overflow prevention
- [ ] CoinService: concurrent spend prevention
- [ ] InventoryService: concurrent purchase prevention
- [ ] DictionaryService: import error handling (malformed files)
- [ ] MultiplayerGameService: disconnect/reconnect scenarios

### T-700.4: Validator Tests (doplnění)
- [ ] Všechny validátory: boundary values (exact min, exact max, min-1, max+1)
- [ ] Unicode/diacritics handling ve všech text inputs
- [ ] SQL injection attempt strings
- [ ] XSS attempt strings (script tags, event handlers)
- [ ] Extrémně dlouhé stringy (> 10000 chars)

### T-700.5: Integration Tests - API
- [ ] **Auth flow**: Register → Login → Refresh → Logout → Protected endpoint (401)
- [ ] **Game flow**: Start → Answer (correct) → Answer (wrong) → Next round → Complete → XP check
- [ ] **Boss flow**: Start Boss → Marathon complete → Condition boss → Twist boss
- [ ] **League flow**: Join → Add XP → Check rank → Weekly reset → Check promotion
- [ ] **Daily Challenge flow**: Get daily → Start → Complete → Check leaderboard → Try again (403)
- [ ] **Streak flow**: Complete level → Check streak → Next day complete → Check increment → Skip day → Check reset
- [ ] **Achievement flow**: Complete word → Check achievement unlock → Progress tracking
- [ ] **Premium flow**: Create checkout → Webhook (payment) → Check premium features → Cancel
- [ ] **Shop flow**: Earn coins → Purchase item → Equip → Check inventory
- [ ] **Dictionary flow**: Create → Add words → Import CSV → Start game with custom → Delete
- [ ] **Team flow**: Create → Invite → Accept → Check members → Kick → Leave → Disband
- [ ] **Guest flow**: Start guest → Play 5 games → 6th game (429) → Register → Convert progress
- [ ] **Admin flow**: Login as admin → CRUD words → Import → Export → Manage users
- [ ] **Notification flow**: Trigger event → Check notification created → Mark read → Check preferences

### T-700.6: Integration Tests - SignalR
- [ ] Matchmaking: join queue → match found notification
- [ ] Match gameplay: submit answer → opponent receives update
- [ ] Match completion: both finish → results broadcast
- [ ] Disconnect handling: disconnect → 30s grace → forfeit
- [ ] Reconnect: disconnect → reconnect within 30s → resume

### T-700.7: E2E Tests (Playwright)
- [ ] Setup Playwright v test projektu
- [ ] Nastavit test fixtures (browser, page, API mocking kde nutné)
- [ ] **Critical User Flows**:
  - [ ] E2E: Register → Login → Dashboard visible
  - [ ] E2E: Login → Start game → Answer 5 words → Level complete
  - [ ] E2E: Dashboard → Check stats → Navigate to paths → Start level
  - [ ] E2E: Guest → Play game → See CTA → Register
  - [ ] E2E: Landing page → CTA click → Register page
  - [ ] E2E: Settings → Change theme → Verify dark mode
  - [ ] E2E: Achievements page → Filter by category → Check cards
- [ ] **Responsive Tests**:
  - [ ] E2E: Mobile viewport (375px) → Navigation works → Game playable
  - [ ] E2E: Tablet viewport (768px) → Layout correct
  - [ ] E2E: Desktop viewport (1280px) → Full layout

### T-700.8: Load Testing
- [ ] Setup k6 nebo Artillery pro load testing
- [ ] Scénáře:
  - [ ] 100 concurrent users: login + play game
  - [ ] 50 concurrent multiplayer matches
  - [ ] 1000 users: dashboard load
  - [ ] Leaderboard query under load (1000+ users per league)
- [ ] Měřit: response time p50/p95/p99, error rate, throughput
- [ ] Identifikovat bottlenecky a opravit

### T-700.9: Security Audit
- [ ] **Authentication**:
  - [ ] JWT token expiry respected
  - [ ] Refresh token rotation works
  - [ ] Password hashing (bcrypt/argon2)
  - [ ] Account lockout after failed attempts
  - [ ] CORS correctly configured
- [ ] **Authorization**:
  - [ ] All protected endpoints require auth
  - [ ] Admin endpoints require admin role
  - [ ] Users can only access own data
  - [ ] Team operations respect role permissions
- [ ] **Input Validation**:
  - [ ] SQL injection prevention (parameterized queries via EF Core)
  - [ ] XSS prevention (Blazor auto-encoding + CSP headers)
  - [ ] CSRF protection
  - [ ] File upload validation (size, type)
  - [ ] Rate limiting on public endpoints
- [ ] **Data Protection**:
  - [ ] Sensitive data encrypted at rest (connection strings, API keys)
  - [ ] HTTPS enforced
  - [ ] Security headers (HSTS, X-Content-Type-Options, X-Frame-Options)
  - [ ] No sensitive data in logs
  - [ ] No secrets in source code or config files
- [ ] **Payment Security**:
  - [ ] Stripe webhook signature verification
  - [ ] No card data stored on server
  - [ ] PCI compliance checklist

---

## T-701: PWA (Progressive Web App)

### T-701.1: Service Worker
- [ ] Vytvořit `service-worker.js` pro Blazor WASM
- [ ] Konfigurovat cache strategies:
  - Cache First: static assets (CSS, JS, images, fonts)
  - Network First: API calls
  - Stale While Revalidate: non-critical data
- [ ] Offline fallback page
- [ ] Service worker update notification (new version available → reload prompt)

### T-701.2: Web App Manifest
- [ ] Vytvořit `manifest.json`:
  - name: "LexiQuest"
  - short_name: "LexiQuest"
  - start_url: "/dashboard"
  - display: "standalone"
  - background_color: "#ffffff"
  - theme_color: "#FF9800"
  - icons: 192px, 512px (PNG + maskable)
- [ ] App icons v multiple sizes (48, 72, 96, 128, 144, 192, 256, 384, 512)
- [ ] Splash screens pro iOS
- [ ] Link manifest v index.html

### T-701.3: Offline Support
- [ ] Offline detection v Blazor (`navigator.onLine`)
- [ ] Offline banner: `TmAlert Severity="Warning"` "Jsi offline - některé funkce nejsou dostupné"
- [ ] Cache posledních seed dat pro offline gameplay (training mode only)
- [ ] Queue API calls when offline → replay when online
- [ ] LocalStorage fallback pro kritická data

### T-701.4: Install Prompt
- [ ] Implementovat "Add to Home Screen" prompt
- [ ] `TmAlert` nebo `TmModal` s install instrukcemi
- [ ] Detekovat standalone mode → přizpůsobit UI (skrýt install prompt)

---

## T-702: Production Deployment

### T-702.1: CI/CD Pipeline (GitHub Actions)
- [ ] Vytvořit `.github/workflows/ci.yml`:
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

- [ ] Vytvořit `.github/workflows/deploy.yml`:
  - Trigger: release tag
  - Steps:
    1. Build + Test (same as CI)
    2. Publish API to Azure App Service / IIS
    3. Publish Blazor WASM to Azure Static Web Apps / IIS
    4. Run EF Core migrations (production DB)
    5. Health check after deployment
    6. Notify (Slack/Discord) on success/failure

### T-702.2: Azure Infrastructure (nebo IIS)
- [ ] **API hosting**:
  - Azure App Service (Linux, .NET 10) NEBO IIS s Windows Server
  - Environment variables pro secrets (connection string, JWT key, Stripe key)
  - Application Insights pro monitoring
  - Auto-scaling rules (CPU > 70% → scale out)

- [ ] **Frontend hosting**:
  - Azure Static Web Apps NEBO IIS static files
  - CDN pro static assets
  - Custom domain + SSL certificate

- [ ] **Database**:
  - Azure SQL Database NEBO MSSQL Server on-premise
  - Automated backups (daily, 7-day retention)
  - Geo-replication (optional for HA)

- [ ] **Networking**:
  - HTTPS only (force redirect)
  - CORS: only Blazor frontend origin
  - Rate limiting: 100 req/min per IP
  - DDoS protection (Azure default)

### T-702.3: Monitoring & Logging (Serilog)
- [ ] Konfigurovat Serilog sinks:
  - Console (development)
  - File (rolling, 7-day retention)
  - Application Insights (production)
- [ ] Structured logging pro:
  - API requests/responses (duration, status, path)
  - Authentication events (login, logout, failed attempt)
  - Game events (session start/end, XP gained)
  - Payment events (checkout, webhook, subscription change)
  - Errors (unhandled exceptions, validation failures)
- [ ] Health check endpoints:
  - `/health` → basic health
  - `/health/ready` → DB connection + external services
  - `/health/live` → liveness probe
- [ ] Application Insights dashboards:
  - Request rate, response time, failure rate
  - Active users, game sessions per hour
  - Error tracking with stack traces
  - Custom metrics: XP earned/hour, games/hour

### T-702.4: Backup Strategy (MSSQL)
- [ ] Automated daily full backup
- [ ] Hourly differential backup
- [ ] Transaction log backup every 15 min
- [ ] Backup retention: 30 days
- [ ] Restore procedure dokumentace
- [ ] Test restore na dev environment

### T-702.5: Environment Configuration
- [ ] `appsettings.Production.json` (non-secret defaults)
- [ ] Environment variables pro secrets:
  - `ConnectionStrings__DefaultConnection`
  - `JwtSettings__SecretKey`
  - `Stripe__ApiKey`
  - `Stripe__WebhookSecret`
  - `Email__ApiKey`
  - `Push__VapidPrivateKey`
- [ ] Konfigurovat CORS pro produkční domain
- [ ] Konfigurovat rate limiting pro produkci

---

## T-703: Dokumentace

### T-703.1: API Dokumentace (Swagger/OpenAPI)
- [ ] Ověřit že všechny endpointy mají Swagger dokumentaci
- [ ] Přidat XML komentáře pro:
  - Endpoint popis a parametry
  - Request/Response DTO schemas
  - Status codes (200, 201, 400, 401, 403, 404, 409, 429)
  - Authentication requirements
- [ ] Swagger UI přístupné na `/swagger` (pouze development/staging)
- [ ] Exportovat OpenAPI spec (JSON/YAML)

### T-703.2: Deployment Guide
- [ ] Vytvořit `docs/deployment/DeploymentGuide.md`:
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
- [ ] Vytvořit `docs/deployment/TroubleshootingGuide.md`:
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
- [ ] Vytvořit/aktualizovat `README.md` v root:
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
- [ ] Registrace a přihlášení funguje
- [ ] Herní smyčka kompletní (training, timed, path, boss)
- [ ] XP a level systém funguje
- [ ] Streak systém s fire indikátorem
- [ ] Cesty s 4 difficulty levels a boss levely
- [ ] Ligy s weekly reset a promotion/demotion
- [ ] Denní výzva s modifiers
- [ ] Achievementy s unlock animacemi
- [ ] Landing page s guest mode
- [ ] Premium s Stripe platbami
- [ ] Obchod s mincemi a items
- [ ] Multiplayer 1v1 přes SignalR
- [ ] Týmy a klany
- [ ] Notifikace (push + email)
- [ ] Admin panel
- [ ] AI výzvy
- [ ] Nastavení profilu

### Technická kontrola
- [ ] Unit test coverage > 80%
- [ ] Integration testy pro všechny API flows
- [ ] E2E testy pro kritické flows
- [ ] Load testing passed
- [ ] Security audit passed
- [ ] PWA: offline support, installable
- [ ] Responsive: mobile, tablet, desktop
- [ ] Performance: Lighthouse > 90
- [ ] Accessibility: WCAG 2.1 AA
- [ ] SEO: meta tags, Open Graph, JSON-LD
- [ ] Monitoring: Application Insights configured
- [ ] Logging: Serilog structured logs
- [ ] Backups: automated daily/hourly
- [ ] CI/CD: GitHub Actions pipeline working
- [ ] HTTPS: enforced everywhere
- [ ] No hardcoded secrets in code
- [ ] All texts from .resx resources
- [ ] FluentValidation on all forms (FE + BE)

---

## Ověření dokončení fáze

- [ ] Test coverage > 80% pro Core a Infrastructure
- [ ] Integration testy pro všechny API endpointy
- [ ] E2E testy pro kritické user flows
- [ ] Load testing: 100+ concurrent users bez chyb
- [ ] Security audit: žádné kritické zranitelnosti
- [ ] PWA: installable, offline support
- [ ] CI/CD: pipeline funguje (build → test → deploy)
- [ ] Production: API + Blazor WASM nasazeno
- [ ] Monitoring: dashboards v Application Insights
- [ ] Backups: automated, restore testován
- [ ] Dokumentace: API docs, deployment guide, troubleshooting
- [ ] `dotnet test` → všechny testy zelené
- [ ] Lighthouse > 90 na produkci
