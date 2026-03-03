# Fáze 6: Advanced Features (Týden 10-11)

> **Cíl:** Notifikace (push + email), admin panel, AI výzvy, performance optimalizace
> **Závislost:** Fáze 2 (streak, achievements, ligy), Fáze 4 (premium)
> **Tempo.Blazor komponenty:** TmNotificationBell, TmDataTable, TmCard, TmStatCard, TmButton, TmBadge, TmModal, TmIcon, TmProgressBar, TmAlert, TmToggle, TmTimePicker, TmSelect, TmFormField, TmTextInput, TmSearchInput, TmTabs, TmTabPanel, TmChart, TmEmptyState, TmDrawer, TmFileDropZone, TmBulkActionBar, TmColumnPicker, TmFilterBuilder, TmPagination, ToastService, FluentValidationValidator

---

## ⚠️ KRITICKÁ PRAVIDLA

- **TDD:** Test FIRST → RED → GREEN → REFACTOR
- **Žádné hardcoded texty** → vše z `.resx`
- **FluentValidation** na FE i BE s lokalizací
- **DTOs** v `LexiQuest.Shared`
- **HTTP status kódy** místo wrapper tříd
- **Admin panel:** role-based access control (RBAC)
- **Produkční kód** od prvního řádku

---

## T-600: UC-023 Notifikace

### T-600.1: Notification Domain (TDD)
- [ ] **TEST:** `Notification_Create_SetsDefaultUnread` → RED
- [ ] **TEST:** `Notification_MarkRead_SetsReadAt` → RED
- [ ] **TEST:** `NotificationPreference_Default_AllEnabled` → RED
- [ ] Vytvořit `Notification` entitu (Id, UserId, Type, Title, Message, Severity, IsRead, ReadAt, CreatedAt, ActionUrl)
- [ ] Vytvořit `NotificationType` enum (StreakWarning, StreakLost, DailyChallenge, LeagueUpdate, AchievementUnlocked, Milestone, SystemMessage)
- [ ] Vytvořit `NotificationPreference` entitu (UserId, PushEnabled, EmailEnabled, StreakReminder, StreakReminderTime, LeagueUpdates, AchievementNotifications, DailyChallengeReminder)
- [ ] EF Core konfigurace + migrace
- [ ] **GREEN:** Testy prochází

### T-600.2: NotificationService (TDD)
- [ ] **TEST:** `NotificationService_Send_StreakWarning_CreatesNotification` → RED
- [ ] **TEST:** `NotificationService_Send_RespectsPreferences_PushDisabled_SkipsPush` → RED
- [ ] **TEST:** `NotificationService_Send_RespectsPreferences_EmailDisabled_SkipsEmail` → RED
- [ ] **TEST:** `NotificationService_GetUnread_ReturnsOnlyUnread` → RED
- [ ] **TEST:** `NotificationService_GetUnreadCount_ReturnsCorrectCount` → RED
- [ ] **TEST:** `NotificationService_MarkAllRead_MarksAll` → RED
- [ ] **TEST:** `NotificationService_FrequencyLimit_DoesNotSpam` → RED
- [ ] Vytvořit `INotificationService` interface
- [ ] Vytvořit DTOs: `NotificationDto`, `NotificationPreferenceDto`, `UpdatePreferencesRequest`
- [ ] Implementovat `NotificationService`:
  - In-app notification creation (DB persist)
  - Push notification dispatch (browser Push API)
  - Email notification dispatch (via IEmailService)
  - Frequency limiting (max 5 per hodinu)
- [ ] **GREEN:** Všechny testy prochází

### T-600.3: Push Notification Setup
- [ ] Nastavit Web Push API v Blazor (VAPID keys)
- [ ] Vytvořit `PushSubscription` entitu (UserId, Endpoint, P256dh, Auth)
- [ ] Implementovat `IPushService` interface
- [ ] Implementovat `WebPushService` (web-push library)
- [ ] Service worker pro push notifications
- [ ] EF Core migrace pro PushSubscription

### T-600.4: Notification Background Jobs (Hangfire)
- [ ] **TEST:** `StreakReminderJob_Execute_SendsNotificationAt21h` → RED
- [ ] **TEST:** `DailyChallengeReminderJob_Execute_SendsAt8h` → RED
- [ ] **TEST:** `InactiveReminderJob_Execute_Sends7DaysInactive` → RED
- [ ] Vytvořit `StreakReminderJob` - denně v 21:00 pokud uživatel nesplnil denní cíl
- [ ] Vytvořit `DailyChallengeReminderJob` - denně v 8:00
- [ ] Vytvořit `InactiveReminderJob` - po 7 dnech neaktivity → email "Chybíš nám!"
- [ ] Zaregistrovat Hangfire recurring jobs
- [ ] **GREEN:** Testy prochází

### T-600.5: Notification Endpoints
- [ ] Vytvořit `GET /api/v1/notifications` (seznam notifikací, paginovaný)
- [ ] Vytvořit `GET /api/v1/notifications/unread-count` (počet nepřečtených)
- [ ] Vytvořit `POST /api/v1/notifications/{id}/read` (označí jako přečtené)
- [ ] Vytvořit `POST /api/v1/notifications/read-all` (označí všechny)
- [ ] Vytvořit `GET /api/v1/notifications/preferences` (preference)
- [ ] Vytvořit `PUT /api/v1/notifications/preferences` (aktualizuje preference)
- [ ] Vytvořit `POST /api/v1/notifications/push-subscription` (uloží push subscription)

### T-600.6: Frontend - Notification Bell (Tempo.Blazor)
- [ ] **TEST (bUnit):** `NotificationBell_ShowsUnreadCount` → RED
- [ ] **TEST (bUnit):** `NotificationBell_Click_ShowsDropdown` → RED
- [ ] **TEST (bUnit):** `NotificationBell_MarkAllRead_ClearsCount` → RED
- [ ] Přidat `TmNotificationBell` do `TmTopBar` v MainLayout
- [ ] Konfigurovat s custom notification provider:
  - Unread count badge
  - Dropdown s notifikacemi (Today, Yesterday, This Week grouping)
  - Per-notification: `TmIcon` (dle severity), title, subtitle, time, `TmButton` action
  - "Označit vše jako přečtené" `TmButton`
- [ ] Real-time update unread count (polling každých 30s nebo SignalR)
- [ ] **GREEN:** Testy prochází

### T-600.7: Frontend - Notification Preferences (Tempo.Blazor)
- [ ] Přidat do Settings page sekci "Notifikace":
  - `TmToggle` Push notifications
  - `TmToggle` Email notifications
  - `TmToggle` Streak reminder + `TmTimePicker` čas
  - `TmToggle` League updates
  - `TmToggle` Achievement notifications
  - `TmToggle` Daily challenge reminder
- [ ] Push permission request dialog při prvním zapnutí

### T-600.8: Integrace notifikací do existujících features
- [ ] Streak warning → push at 21:00 pokud nesplněno
- [ ] Streak lost → push + in-app
- [ ] Daily challenge available → push at 8:00
- [ ] League position change → in-app real-time
- [ ] Achievement unlocked → in-app + toast
- [ ] Level up → in-app
- [ ] Premium expiry → email 3 dny předem

---

## T-601: UC-024 Admin Panel

### T-601.1: Admin Role Setup (TDD)
- [ ] **TEST:** `AdminAuthorizationService_IsAdmin_ReturnsTrueForAdminRole` → RED
- [ ] **TEST:** `AdminAuthorizationService_IsModerator_ReturnsTrueForModRole` → RED
- [ ] **TEST:** `AdminAuthorizationService_RegularUser_ReturnsFalseForAdmin` → RED
- [ ] Vytvořit admin role: Admin, Moderator, ContentManager
- [ ] Seed admin uživatele
- [ ] Vytvořit `[Authorize(Roles = "Admin")]` atribut pro admin endpointy
- [ ] Vytvořit `[Authorize(Policy = "ContentManagement")]` policy
- [ ] **GREEN:** Testy prochází

### T-601.2: Admin - Word Management Backend (TDD)
- [ ] **TEST:** `AdminWordService_GetWords_ReturnsPaginatedList` → RED
- [ ] **TEST:** `AdminWordService_CreateWord_AddsToDb` → RED
- [ ] **TEST:** `AdminWordService_UpdateWord_ModifiesExisting` → RED
- [ ] **TEST:** `AdminWordService_DeleteWord_RemovesFromDb` → RED
- [ ] **TEST:** `AdminWordService_BulkImport_CSV_AddsMultiple` → RED
- [ ] **TEST:** `AdminWordService_BulkImport_DuplicatesSkipped` → RED
- [ ] **TEST:** `AdminWordService_Export_ReturnsCSV` → RED
- [ ] **TEST:** `AdminWordService_GetStats_ReturnsDifficultyDistribution` → RED
- [ ] Vytvořit `IAdminWordService` interface
- [ ] Vytvořit DTOs: `AdminWordDto`, `AdminWordListRequest` (search, filter, pagination), `BulkImportResult`, `WordStatsDto`
- [ ] Vytvořit validátory: `CreateWordValidator`, `UpdateWordValidator`
- [ ] Implementovat `AdminWordService` s IDataTableDataProvider<AdminWordDto>
- [ ] **GREEN:** Testy prochází

### T-601.3: Admin - Word Management Endpoints
- [ ] Vytvořit `GET /api/v1/admin/words` (paginovaný, filtrovatelný)
- [ ] Vytvořit `POST /api/v1/admin/words` (vytvoří slovo)
- [ ] Vytvořit `PUT /api/v1/admin/words/{id}` (upraví slovo)
- [ ] Vytvořit `DELETE /api/v1/admin/words/{id}` (smaže slovo)
- [ ] Vytvořit `POST /api/v1/admin/words/import` (bulk CSV import)
- [ ] Vytvořit `GET /api/v1/admin/words/export` (CSV export)
- [ ] Vytvořit `GET /api/v1/admin/words/stats` (statistiky)
- [ ] Všechny s `[Authorize(Roles = "Admin,ContentManager")]`

### T-601.4: Admin - User Management Backend (TDD)
- [ ] **TEST:** `AdminUserService_GetUsers_ReturnsPaginatedList` → RED
- [ ] **TEST:** `AdminUserService_SuspendUser_SetsLockedOut` → RED
- [ ] **TEST:** `AdminUserService_UnsuspendUser_ClearsLockout` → RED
- [ ] **TEST:** `AdminUserService_ResetPassword_SendsResetEmail` → RED
- [ ] Vytvořit `IAdminUserService` interface
- [ ] Vytvořit DTOs: `AdminUserDto`, `AdminUserListRequest`
- [ ] Implementovat `AdminUserService`
- [ ] **GREEN:** Testy prochází

### T-601.5: Admin - User Management Endpoints
- [ ] Vytvořit `GET /api/v1/admin/users` (paginovaný, searchable)
- [ ] Vytvořit `GET /api/v1/admin/users/{id}` (detail)
- [ ] Vytvořit `POST /api/v1/admin/users/{id}/suspend` (suspension)
- [ ] Vytvořit `POST /api/v1/admin/users/{id}/unsuspend` (unsuspend)
- [ ] Vytvořit `POST /api/v1/admin/users/{id}/reset-password` (password reset)
- [ ] Všechny s `[Authorize(Roles = "Admin")]`

### T-601.6: Frontend - Admin Dashboard (Tempo.Blazor)
- [ ] **TEST (bUnit):** `AdminDashboard_Renders_StatsCards` → RED
- [ ] **TEST (bUnit):** `AdminDashboard_NonAdmin_RedirectsToHome` → RED
- [ ] Vytvořit `AdminDashboard.razor` (`@page "/admin"`)
- [ ] `@inject IStringLocalizer<Admin> L`
- [ ] **Stats Cards**: `TmStatCard` × 4 (Total Users, Active Today, Total Words, Daily Challenges)
- [ ] **Charts**: `TmChart` (Line) - registrace za posledních 30 dní
- [ ] **Quick Links**: `TmCard` s `TmButton` ke správě slov, uživatelů
- [ ] Route guard: redirect na `/` pokud nemá admin roli

### T-601.7: Frontend - Word Management (Tempo.Blazor)
- [ ] **TEST (bUnit):** `WordManagement_Renders_DataTable` → RED
- [ ] **TEST (bUnit):** `WordManagement_Search_FiltersResults` → RED
- [ ] **TEST (bUnit):** `WordManagement_Import_ProcessesCSV` → RED
- [ ] Vytvořit `AdminWords.razor` (`@page "/admin/words"`)
- [ ] `TmDataTable<AdminWordDto>` s:
  - Columns: Word, Difficulty `TmBadge`, Category, Length, Success Rate, Actions
  - `TmSearchInput` pro vyhledávání (debounced)
  - `TmFilterBuilder` s filtry: Difficulty (Select), Category (Select), Length range (Number)
  - `TmColumnPicker` pro viditelnost sloupců
  - `TmPagination` (page sizes: 25, 50, 100)
  - `TmBulkActionBar` pro hromadné akce (Delete selected)
  - Server-side data provider (IDataTableDataProvider)
- [ ] **CRUD modaly**:
  - Create: `TmModal` s `TmFormField` + `TmTextInput` (Word), `TmSelect` (Difficulty, Category)
  - Edit: `TmModal` s pre-filled fields
  - Delete: `TmModal` confirm dialog
  - `<FluentValidationValidator />` ve všech formulářích
- [ ] **Import**: `TmFileDropZone` pro CSV, result `TmAlert` s počtem importovaných/přeskočených
- [ ] **Export**: `TmButton` "Export CSV" → stáhne soubor
- [ ] **Stats panel**: `TmDrawer` s `TmChart` (Pie) difficulty distribution, `TmChart` (Bar) success rates
- [ ] **GREEN:** Testy prochází

### T-601.8: Frontend - User Management (Tempo.Blazor)
- [ ] **TEST (bUnit):** `UserManagement_Renders_DataTable` → RED
- [ ] Vytvořit `AdminUsers.razor` (`@page "/admin/users"`)
- [ ] `TmDataTable<AdminUserDto>` s:
  - Columns: `TmAvatar`, Username, Email, Level, XP, Streak, Status `TmBadge`, Registered, Actions
  - `TmSearchInput` pro vyhledávání
  - `TmFilterBuilder`: Status (Active/Suspended), Premium (Yes/No), Level range
  - Actions: `TmButton` Suspend/Unsuspend, Reset Password, View Detail
- [ ] **User Detail**: `TmDrawer` s kompletním profilem, stats, game history
- [ ] **GREEN:** Test prochází

---

## T-602: UC-025 AI Generované výzvy

### T-602.1: AIChallengeService (TDD)
- [ ] **TEST:** `AIChallengeService_Analyze_IdentifiesWeakLetters` → RED
- [ ] **TEST:** `AIChallengeService_Analyze_IdentifiesSlowCategories` → RED
- [ ] **TEST:** `AIChallengeService_Generate_WeaknessFocus_SelectsProblematicLetters` → RED
- [ ] **TEST:** `AIChallengeService_Generate_SpeedTraining_SelectsShortWords` → RED
- [ ] **TEST:** `AIChallengeService_Generate_MemoryGame_SelectsRepeatedWords` → RED
- [ ] **TEST:** `AIChallengeService_Generate_PatternRecognition_SelectsSimilarWords` → RED
- [ ] **TEST:** `AIChallengeService_PredictDifficulty_ReturnsScore0to1` → RED
- [ ] Vytvořit `IAIChallengeService` interface
- [ ] Vytvořit `AIChallengeType` enum (WeaknessFocus, SpeedTraining, MemoryGame, PatternRecognition)
- [ ] Vytvořit DTOs: `AIChallengeRequest`, `AIChallengeDto`, `AIChallengeWordDto` (word, predictedDifficulty, reason)
- [ ] Implementovat `AIChallengeService`:
  - Analýza uživatelských dat (error rate per letter, avg time per category)
  - Weakness Focus: slova s písmeny kde error rate > 30%
  - Speed Training: krátká slova pro rychlost
  - Memory Game: slova z předchozích chyb
  - Pattern Recognition: slova s podobnou strukturou
  - Difficulty prediction: 0-1 based on user history
- [ ] **GREEN:** Všechny testy prochází

### T-602.2: AI Challenge Endpoints
- [ ] Vytvořit `GET /api/v1/challenges/ai` (vrací personalizovanou výzvu)
- [ ] Vytvořit `POST /api/v1/challenges/ai/start` (spustí AI challenge session)
- [ ] Vytvořit `GET /api/v1/challenges/ai/analysis` (vrací analýzu hráčových slabostí)

### T-602.3: Frontend - AI Challenge Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `AIChallengePage_Renders_ChallengeTypes` → RED
- [ ] **TEST (bUnit):** `AIChallengePage_ShowsAnalysis` → RED
- [ ] Vytvořit `AIChallenge.razor` (`@page "/ai-challenge"`)
- [ ] `@inject IStringLocalizer<AIChallenge> L`
- [ ] **Analysis Section**: `TmCard` s:
  - Weakness letters: `TmChip` pro každé problematické písmeno
  - Slow categories: `TmProgressBar` s success rate per category
  - Tips: `TmAlert Severity="Info"` s doporučeními
- [ ] **Challenge Types**: 4× `TmCard`:
  - Weakness Focus 🎯: "Zaměř se na slabá písmena"
  - Speed Training ⚡: "Zrychli své reakce"
  - Memory Game 🧠: "Opakuj problematická slova"
  - Pattern Recognition 🔍: "Rozpoznej vzory"
  - Každý s `TmButton` "Začít"
- [ ] **Challenge Session**: Reuse GameArena s AI-specifickým feedbackem
  - Po každém slově: `TmTooltip` s "Proč toto slovo?" (AI explanation)
- [ ] **GREEN:** Testy prochází

---

## T-603: Performance Optimalizace

### T-603.1: In-Memory Caching Optimalizace
- [ ] **TEST:** `CachedWordRepository_GetRandom_UsesCacheFirst` → RED
- [ ] **TEST:** `CachedWordRepository_CacheExpired_RefreshesFromDb` → RED
- [ ] Cache strategie:
  - Words by difficulty: MediumLived (30 min)
  - User stats: ShortLived (5 min)
  - Leaderboard: ShortLived (5 min)
  - Daily challenge: Daily (24h)
  - Path structure: LongLived (2h)
- [ ] Implementovat cache invalidation při CRUD operacích
- [ ] Cache warming na startup (Beginner words, paths)
- [ ] **GREEN:** Testy prochází

### T-603.2: Database Query Optimalizace (MSSQL)
- [ ] Analyzovat slow queries přes SQL Server Profiler / EF Core logging
- [ ] Přidat indexy:
  - `IX_User_Email` (unique)
  - `IX_User_Username` (unique)
  - `IX_Word_Difficulty_Category` (composite)
  - `IX_GameSession_UserId_StartedAt` (composite)
  - `IX_LeagueParticipant_WeeklyXP` (pro leaderboard sorting)
  - `IX_Notification_UserId_IsRead` (pro unread count)
  - `IX_Achievement_Category` (pro filtering)
- [ ] Optimalizovat N+1 queries → Include/ThenInclude kde potřeba
- [ ] Přidat `AsNoTracking()` pro read-only queries
- [ ] EF Core migrace s indexy

### T-603.3: Frontend Bundle Optimalizace
- [ ] Analyzovat Blazor WASM bundle size
- [ ] Lazy loading pro non-critical pages (Admin, AI Challenge, Team)
- [ ] Trim unused assemblies v .csproj (`<BlazorWebAssemblyEnableLinking>true</BlazorWebAssemblyEnableLinking>`)
- [ ] Compress static assets (Brotli/gzip)
- [ ] Cache static assets s versioning (hash v URL)

### T-603.4: Image Optimalizace
- [ ] WebP format pro všechny obrázky
- [ ] Responsive images (`srcset` pro různé velikosti)
- [ ] Lazy loading pro obrázky mimo viewport
- [ ] Avatar thumbnails: generovat menší varianty (32px, 64px, 128px)
- [ ] CDN pro statické assety (připravit konfiguraci)

---

## Ověření dokončení fáze

- [ ] Push notifikace fungují v prohlížeči
- [ ] Email notifikace se odesílají (streak warning, daily challenge, inactive)
- [ ] Notification bell v top bar se správným unread count
- [ ] Notification preferences fungují (push/email toggle per type)
- [ ] Admin panel: word CRUD, bulk import/export, statistics
- [ ] Admin panel: user management (suspend, reset password)
- [ ] Admin panel: role-based access (Admin, Moderator, ContentManager)
- [ ] AI výzvy: analýza slabostí, 4 typy challengí, personalizace
- [ ] Caching: optimalizované cache strategie pro všechny entity
- [ ] DB indexy: optimalizované queries
- [ ] Bundle size: optimalizovaný WASM bundle
- [ ] Všechny texty z .resx
- [ ] `dotnet test` → všechny testy zelené
