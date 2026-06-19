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
- [x] **TEST:** `Notification_Create_SetsDefaultUnread` → RED
- [x] **TEST:** `Notification_MarkRead_SetsReadAt` → RED
- [x] **TEST:** `NotificationPreference_Default_AllEnabled` → RED
- [x] Vytvořit `Notification` entitu (Id, UserId, Type, Title, Message, Severity, IsRead, ReadAt, CreatedAt, ActionUrl)
- [x] Vytvořit `NotificationType` enum (StreakWarning, StreakLost, DailyChallenge, LeagueUpdate, AchievementUnlocked, Milestone, SystemMessage)
- [x] Vytvořit `NotificationPreference` entitu (UserId, PushEnabled, EmailEnabled, StreakReminder, StreakReminderTime, LeagueUpdates, AchievementNotifications, DailyChallengeReminder)
- [x] EF Core konfigurace + migrace
- [x] **GREEN:** Testy prochází

### T-600.2: NotificationService (TDD)
- [x] **TEST:** `NotificationService_Send_StreakWarning_CreatesNotification` → RED
- [x] **TEST:** `NotificationService_Send_RespectsPreferences_PushDisabled_SkipsPush` → RED
- [x] **TEST:** `NotificationService_Send_RespectsPreferences_EmailDisabled_SkipsEmail` → RED
- [x] **TEST:** `NotificationService_GetUnread_ReturnsOnlyUnread` → RED
- [x] **TEST:** `NotificationService_GetUnreadCount_ReturnsCorrectCount` → RED
- [x] **TEST:** `NotificationService_MarkAllRead_MarksAll` → RED
- [x] **TEST:** `NotificationService_FrequencyLimit_DoesNotSpam` → RED
- [x] Vytvořit `INotificationService` interface
- [x] Vytvořit DTOs: `NotificationDto`, `NotificationPreferenceDto`, `UpdatePreferencesRequest`
- [x] Implementovat `NotificationService`:
  - In-app notification creation (DB persist)
  - Push notification dispatch (browser Push API)
  - Email notification dispatch (via IEmailService)
  - Frequency limiting (max 5 per hodinu)
- [x] **GREEN:** Všechny testy prochází

### T-600.3: Push Notification Setup
- [x] Nastavit Web Push API v Blazor (VAPID keys)
- [x] Vytvořit `PushSubscription` entitu (UserId, Endpoint, P256dh, Auth)
- [x] Implementovat `IPushService` interface
- [x] Implementovat `WebPushService` (web-push library)
- [x] Service worker pro push notifications
- [x] EF Core migrace pro PushSubscription

### T-600.4: Notification Background Jobs (Hangfire)
- [x] **TEST:** `StreakReminderJob_Execute_SendsNotificationAt21h` → RED
- [x] **TEST:** `DailyChallengeReminderJob_Execute_SendsAt8h` → RED
- [x] **TEST:** `InactiveReminderJob_Execute_Sends7DaysInactive` → RED
- [x] Vytvořit `StreakReminderJob` - denně v 21:00 pokud uživatel nesplnil denní cíl
- [x] Vytvořit `DailyChallengeReminderJob` - denně v 8:00
- [x] Vytvořit `InactiveReminderJob` - po 7 dnech neaktivity → email "Chybíš nám!"
- [x] Zaregistrovat Hangfire recurring jobs
- [x] **GREEN:** Testy prochází

### T-600.5: Notification Endpoints
- [x] Vytvořit `GET /api/v1/notifications` (seznam notifikací, paginovaný)
- [x] Vytvořit `GET /api/v1/notifications/unread-count` (počet nepřečtených)
- [x] Vytvořit `POST /api/v1/notifications/{id}/read` (označí jako přečtené)
- [x] Vytvořit `POST /api/v1/notifications/read-all` (označí všechny)
- [x] Vytvořit `GET /api/v1/notifications/preferences` (preference)
- [x] Vytvořit `PUT /api/v1/notifications/preferences` (aktualizuje preference)
- [x] Vytvořit `POST /api/v1/notifications/push-subscription` (uloží push subscription)

### T-600.6: Frontend - Notification Bell (Tempo.Blazor)
- [x] **TEST (bUnit):** `NotificationBell_ShowsUnreadCount` → RED
- [x] **TEST (bUnit):** `NotificationBell_Click_ShowsDropdown` → RED
- [x] **TEST (bUnit):** `NotificationBell_MarkAllRead_ClearsCount` → RED
- [x] Přidat `TmNotificationBell` do `TmTopBar` v MainLayout
- [x] Konfigurovat s custom notification provider:
  - Unread count badge
  - Dropdown s notifikacemi (Today, Yesterday, This Week grouping)
  - Per-notification: `TmIcon` (dle severity), title, subtitle, time, `TmButton` action
  - "Označit vše jako přečtené" `TmButton`
- [x] Real-time update unread count (polling každých 30s nebo SignalR)
- [x] **GREEN:** Testy prochází

### T-600.7: Frontend - Notification Preferences (Tempo.Blazor)
- [x] Přidat do Settings page sekci "Notifikace":
  - `TmToggle` Push notifications
  - `TmToggle` Email notifications
  - `TmToggle` Streak reminder + `TmTimePicker` čas
  - `TmToggle` League updates
  - `TmToggle` Achievement notifications
  - `TmToggle` Daily challenge reminder
- [x] Push permission request dialog při prvním zapnutí

### T-600.8: Integrace notifikací do existujících features
- [x] Streak warning → push at 21:00 pokud nesplněno
- [x] Streak lost → push + in-app
- [x] Daily challenge available → push at 8:00
- [x] League position change → in-app real-time
- [x] Achievement unlocked → in-app + toast
- [x] Level up → in-app
- [x] Premium expiry → email 3 dny předem

---

## T-601: UC-024 Admin Panel

### T-601.1: Admin Role Setup (TDD)
- [x] **TEST:** `AdminAuthorizationService_IsAdmin_ReturnsTrueForAdminRole` → RED
- [x] **TEST:** `AdminAuthorizationService_IsModerator_ReturnsTrueForModRole` → RED
- [x] **TEST:** `AdminAuthorizationService_RegularUser_ReturnsFalseForAdmin` → RED
- [x] Vytvořit admin role: Admin, Moderator, ContentManager
- [x] Seed admin uživatele
- [x] Vytvořit `[Authorize(Roles = "Admin")]` atribut pro admin endpointy
- [x] Vytvořit `[Authorize(Policy = "ContentManagement")]` policy
- [x] **GREEN:** Testy prochází

### T-601.2: Admin - Word Management Backend (TDD)
- [x] **TEST:** `AdminWordService_GetWords_ReturnsPaginatedList` → RED
- [x] **TEST:** `AdminWordService_CreateWord_AddsToDb` → RED
- [x] **TEST:** `AdminWordService_UpdateWord_ModifiesExisting` → RED
- [x] **TEST:** `AdminWordService_DeleteWord_RemovesFromDb` → RED
- [x] **TEST:** `AdminWordService_BulkImport_CSV_AddsMultiple` → RED
- [x] **TEST:** `AdminWordService_BulkImport_DuplicatesSkipped` → RED
- [x] **TEST:** `AdminWordService_Export_ReturnsCSV` → RED
- [x] **TEST:** `AdminWordService_GetStats_ReturnsDifficultyDistribution` → RED
- [x] Vytvořit `IAdminWordService` interface
- [x] Vytvořit DTOs: `AdminWordDto`, `AdminWordListRequest` (search, filter, pagination), `BulkImportResult`, `WordStatsDto`
- [x] Vytvořit validátory: `CreateWordValidator`, `UpdateWordValidator`
- [x] Implementovat `AdminWordService` s IDataTableDataProvider<AdminWordDto>
- [x] **GREEN:** Testy prochází

### T-601.3: Admin - Word Management Endpoints
- [x] Vytvořit `GET /api/v1/admin/words` (paginovaný, filtrovatelný)
- [x] Vytvořit `POST /api/v1/admin/words` (vytvoří slovo)
- [x] Vytvořit `PUT /api/v1/admin/words/{id}` (upraví slovo)
- [x] Vytvořit `DELETE /api/v1/admin/words/{id}` (smaže slovo)
- [x] Vytvořit `POST /api/v1/admin/words/import` (bulk CSV import)
- [x] Vytvořit `GET /api/v1/admin/words/export` (CSV export)
- [x] Vytvořit `GET /api/v1/admin/words/stats` (statistiky)
- [x] Všechny s `[Authorize(Roles = "Admin,ContentManager")]`

### T-601.4: Admin - User Management Backend (TDD)
- [x] **TEST:** `AdminUserService_GetUsers_ReturnsPaginatedList` → RED
- [x] **TEST:** `AdminUserService_SuspendUser_SetsLockedOut` → RED
- [x] **TEST:** `AdminUserService_UnsuspendUser_ClearsLockout` → RED
- [x] **TEST:** `AdminUserService_ResetPassword_SendsResetEmail` → RED
- [x] Vytvořit `IAdminUserService` interface
- [x] Vytvořit DTOs: `AdminUserDto`, `AdminUserListRequest`
- [x] Implementovat `AdminUserService`
- [x] **GREEN:** Testy prochází

### T-601.5: Admin - User Management Endpoints
- [x] Vytvořit `GET /api/v1/admin/users` (paginovaný, searchable)
- [x] Vytvořit `GET /api/v1/admin/users/{id}` (detail)
- [x] Vytvořit `POST /api/v1/admin/users/{id}/suspend` (suspension)
- [x] Vytvořit `POST /api/v1/admin/users/{id}/unsuspend` (unsuspend)
- [x] Vytvořit `POST /api/v1/admin/users/{id}/reset-password` (password reset)
- [x] Všechny s `[Authorize(Roles = "Admin")]`

### T-601.6: Frontend - Admin Dashboard (Tempo.Blazor)
- [x] **TEST (bUnit):** `AdminDashboard_Renders_StatsCards` → RED
- [x] **TEST (bUnit):** `AdminDashboard_NonAdmin_RedirectsToHome` → RED
- [x] Vytvořit `AdminDashboard.razor` (`@page "/admin"`)
- [x] `@inject IStringLocalizer<Admin> L`
- [x] **Stats Cards**: `TmStatCard` × 4 (Total Users, Active Today, Total Words, Daily Challenges)
- [x] **Charts**: `TmChart` (Line) - registrace za posledních 30 dní
- [x] **Quick Links**: `TmCard` s `TmButton` ke správě slov, uživatelů
- [x] Route guard: redirect na `/` pokud nemá admin roli

### T-601.7: Frontend - Word Management (Tempo.Blazor)
- [x] **TEST (bUnit):** `WordManagement_Renders_DataTable` → RED
- [x] **TEST (bUnit):** `WordManagement_Search_FiltersResults` → RED
- [x] **TEST (bUnit):** `WordManagement_Import_ProcessesCSV` → RED
- [x] Vytvořit `AdminWords.razor` (`@page "/admin/words"`)
- [x] `TmDataTable<AdminWordDto>` s:
  - Columns: Word, Difficulty `TmBadge`, Category, Length, Success Rate, Actions
  - `TmSearchInput` pro vyhledávání (debounced)
  - `TmFilterBuilder` s filtry: Difficulty (Select), Category (Select), Length range (Number)
  - `TmColumnPicker` pro viditelnost sloupců
  - `TmPagination` (page sizes: 25, 50, 100)
  - `TmBulkActionBar` pro hromadné akce (Delete selected)
  - Server-side data provider (IDataTableDataProvider)
- [x] **CRUD modaly**:
  - Create: `TmModal` s `TmFormField` + `TmTextInput` (Word), `TmSelect` (Difficulty, Category)
  - Edit: `TmModal` s pre-filled fields
  - Delete: `TmModal` confirm dialog
  - `<FluentValidationValidator />` ve všech formulářích
- [x] **Import**: `TmFileDropZone` pro CSV, result `TmAlert` s počtem importovaných/přeskočených
- [x] **Export**: `TmButton` "Export CSV" → stáhne soubor
- [x] **Stats panel**: `TmDrawer` s `TmChart` (Pie) difficulty distribution, `TmChart` (Bar) success rates
- [x] **GREEN:** Testy prochází

### T-601.8: Frontend - User Management (Tempo.Blazor)
- [x] **TEST (bUnit):** `UserManagement_Renders_DataTable` → RED
- [x] Vytvořit `AdminUsers.razor` (`@page "/admin/users"`)
- [x] `TmDataTable<AdminUserDto>` s:
  - Columns: `TmAvatar`, Username, Email, Level, XP, Streak, Status `TmBadge`, Registered, Actions
  - `TmSearchInput` pro vyhledávání
  - `TmFilterBuilder`: Status (Active/Suspended), Premium (Yes/No), Level range
  - Actions: `TmButton` Suspend/Unsuspend, Reset Password, View Detail
- [x] **User Detail**: `TmDrawer` s kompletním profilem, stats, game history
- [x] **GREEN:** Test prochází

---

## T-602: UC-025 AI Generované výzvy

### T-602.1: AIChallengeService (TDD)
- [x] **TEST:** `AIChallengeService_Analyze_IdentifiesWeakLetters` → RED
- [x] **TEST:** `AIChallengeService_Analyze_IdentifiesSlowCategories` → RED
- [x] **TEST:** `AIChallengeService_Generate_WeaknessFocus_SelectsProblematicLetters` → RED
- [x] **TEST:** `AIChallengeService_Generate_SpeedTraining_SelectsShortWords` → RED
- [x] **TEST:** `AIChallengeService_Generate_MemoryGame_SelectsRepeatedWords` → RED
- [x] **TEST:** `AIChallengeService_Generate_PatternRecognition_SelectsSimilarWords` → RED
- [x] **TEST:** `AIChallengeService_PredictDifficulty_ReturnsScore0to1` → RED
- [x] Vytvořit `IAIChallengeService` interface
- [x] Vytvořit `AIChallengeType` enum (WeaknessFocus, SpeedTraining, MemoryGame, PatternRecognition)
- [x] Vytvořit DTOs: `AIChallengeRequest`, `AIChallengeDto`, `AIChallengeWordDto` (word, predictedDifficulty, reason)
- [x] Implementovat `AIChallengeService`:
  - Analýza uživatelských dat (error rate per letter, avg time per category)
  - Weakness Focus: slova s písmeny kde error rate > 30%
  - Speed Training: krátká slova pro rychlost
  - Memory Game: slova z předchozích chyb
  - Pattern Recognition: slova s podobnou strukturou
  - Difficulty prediction: 0-1 based on user history
- [x] **GREEN:** Všechny testy prochází

### T-602.2: AI Challenge Endpoints
- [x] Vytvořit `GET /api/v1/challenges/ai` (vrací personalizovanou výzvu)
- [x] Vytvořit `POST /api/v1/challenges/ai/start` (spustí AI challenge session)
- [x] Vytvořit `GET /api/v1/challenges/ai/analysis` (vrací analýzu hráčových slabostí)

### T-602.3: Frontend - AI Challenge Page (Tempo.Blazor)
- [x] **TEST (bUnit):** `AIChallengePage_Renders_ChallengeTypes` → RED
- [x] **TEST (bUnit):** `AIChallengePage_ShowsAnalysis` → RED
- [x] Vytvořit `AIChallenge.razor` (`@page "/ai-challenge"`)
- [x] `@inject IStringLocalizer<AIChallenge> L`
- [x] **Analysis Section**: `TmCard` s:
  - Weakness letters: `TmChip` pro každé problematické písmeno
  - Slow categories: `TmProgressBar` s success rate per category
  - Tips: `TmAlert Severity="Info"` s doporučeními
- [x] **Challenge Types**: 4× `TmCard`:
  - Weakness Focus 🎯: "Zaměř se na slabá písmena"
  - Speed Training ⚡: "Zrychli své reakce"
  - Memory Game 🧠: "Opakuj problematická slova"
  - Pattern Recognition 🔍: "Rozpoznej vzory"
  - Každý s `TmButton` "Začít"
- [x] **Challenge Session**: Reuse GameArena s AI-specifickým feedbackem
  - Po každém slově: `TmTooltip` s "Proč toto slovo?" (AI explanation)
- [x] **GREEN:** Testy prochází

---

## T-603: Performance Optimalizace

### T-603.1: In-Memory Caching Optimalizace
- [x] **TEST:** `CachedWordRepository_GetRandom_UsesCacheFirst` → RED
- [x] **TEST:** `CachedWordRepository_CacheExpired_RefreshesFromDb` → RED
- [x] Cache strategie:
  - Words by difficulty: MediumLived (30 min)
  - User stats: ShortLived (5 min)
  - Leaderboard: ShortLived (5 min)
  - Daily challenge: Daily (24h)
  - Path structure: LongLived (2h)
- [x] Implementovat cache invalidation při CRUD operacích
- [x] Cache warming na startup (Beginner words, paths)
- [x] **GREEN:** Testy prochází

### T-603.2: Database Query Optimalizace (MSSQL)
- [x] Analyzovat slow queries přes SQL Server Profiler / EF Core logging
- [x] Přidat indexy:
  - `IX_User_Email` (unique)
  - `IX_User_Username` (unique)
  - `IX_Word_Difficulty_Category` (composite)
  - `IX_GameSession_UserId_StartedAt` (composite)
  - `IX_LeagueParticipant_WeeklyXP` (pro leaderboard sorting)
  - `IX_Notification_UserId_IsRead` (pro unread count)
  - `IX_Achievement_Category` (pro filtering)
- [x] Optimalizovat N+1 queries → Include/ThenInclude kde potřeba
- [x] Přidat `AsNoTracking()` pro read-only queries
- [x] EF Core migrace s indexy

### T-603.3: Frontend Bundle Optimalizace
- [x] Analyzovat Blazor WASM bundle size
- [x] Lazy loading pro non-critical pages (Admin, AI Challenge, Team)
- [x] Trim unused assemblies v .csproj (`<BlazorWebAssemblyEnableLinking>true</BlazorWebAssemblyEnableLinking>`)
- [x] Compress static assets (Brotli/gzip)
- [x] Cache static assets s versioning (hash v URL)

### T-603.4: Image Optimalizace
- [x] WebP format pro všechny obrázky
- [x] Responsive images (`srcset` pro různé velikosti)
- [x] Lazy loading pro obrázky mimo viewport
- [x] Avatar thumbnails: generovat menší varianty (32px, 64px, 128px)
- [x] CDN pro statické assety (připravit konfiguraci)

---

## Ověření dokončení fáze

- [x] Push notifikace fungují v prohlížeči
- [x] Email notifikace se odesílají (streak warning, daily challenge, inactive)
- [x] Notification bell v top bar se správným unread count
- [x] Notification preferences fungují (push/email toggle per type)
- [x] Admin panel: word CRUD, bulk import/export, statistics
- [x] Admin panel: user management (suspend, reset password)
- [x] Admin panel: role-based access (Admin, Moderator, ContentManager)
- [x] AI výzvy: analýza slabostí, 4 typy challengí, personalizace
- [x] Caching: optimalizované cache strategie pro všechny entity
- [x] DB indexy: optimalizované queries
- [x] Bundle size: optimalizovaný WASM bundle
- [x] Všechny texty z .resx
- [x] `dotnet test` → všechny testy zelené
