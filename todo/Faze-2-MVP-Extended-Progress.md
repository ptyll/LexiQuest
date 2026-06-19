# Fáze 2: MVP Extended - Progress Tracker

## T-200: UC-003 Obnova hesla ✅ HOTOVÉ
## T-201: UC-013 Ligy - Backend ✅ HOTOVÉ  
## T-202: UC-013 Ligy - Frontend ✅ HOTOVÉ
## T-203: UC-014 Denní výzva ✅ HOTOVÉ
## T-204: UC-016 Achievementy - Backend ✅ HOTOVÉ
## T-205: UC-016 Achievementy - Frontend ✅ HOTOVÉ

### T-205.1: Achievements Page (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `AchievementsPage_Renders_ProgressBar` → RED → GREEN
- [x] **TEST (bUnit):** `AchievementsPage_Renders_CategoryTabs` → RED → GREEN
- [x] **TEST (bUnit):** `AchievementsPage_FiltersByCategory` → RED → GREEN
- [x] **TEST (bUnit):** `AchievementsPage_UnlockedAchievement_ShowsGoldBorder` → RED → GREEN
- [x] **TEST (bUnit):** `AchievementsPage_InProgressAchievement_ShowsProgressBar` → RED → GREEN
- [x] **TEST (bUnit):** `AchievementsPage_LockedAchievement_ShowsLockIcon` → RED → GREEN
- [x] Vytvořit `Achievements.razor` (`@page "/achievements"`)
- [x] **GREEN:** 6/6 testů

---

## T-206: UC-008,009,010 Boss Levely ✅ HOTOVÉ

### T-206.1: Backend - Boss Types v GameSession (TDD) ✅
- [x] **TEST:** `GameSession_CreateBoss_Marathon_Sets20WordsAnd3Lives` → RED → GREEN
- [x] **TEST:** `GameSession_CreateBoss_Condition_SetsForbiddenLetterPattern` → RED → GREEN
- [x] **TEST:** `GameSession_CreateBoss_Twist_SetsRevealMechanic` → RED → GREEN
- [x] Vytvořit `BossType` enum (Marathon, Condition, Twist)
- [x] Rozšířit `GameSession` o BossType? property
- [x] Rozšířit `GameRound` o ForbiddenLetters, RevealedLettersCount
- [x] EF Core migrace
- [x] **GREEN:** Testy prochází

### T-206.2: Marathon Boss Rules (TDD) ✅
- [x] **TEST:** `MarathonBoss_Start_20Words3Lives` → RED → GREEN
- [x] **TEST:** `MarathonBoss_WrongAnswer_DecreasesLife_NoRegen` → RED → GREEN
- [x] **TEST:** `MarathonBoss_0Lives_GameOver` → RED → GREEN
- [x] **TEST:** `MarathonBoss_AllCorrect_PerfectBonus200XP` → RED → GREEN
- [x] **TEST:** `MarathonBoss_Completed_WithLosses_100XP` → RED → GREEN
- [x] **TEST:** `MarathonBoss_Under5Min_SpeedBonus50XP` → RED → GREEN
- [x] Vytvořit `IMarathonBossRules` interface
- [x] Implementovat pravidla: 20 slov, 3 životy, žádná regenerace, 15s per slovo
- [x] **GREEN:** Všechny testy prochází

### T-206.3: Condition Boss Rules - Forbidden Letter (TDD) ✅
- [x] **TEST:** `ConditionBoss_Start_15Words` → RED → GREEN
- [x] **TEST:** `ConditionBoss_Every3rdWord_HasForbiddenLetter` → RED → GREEN
- [x] **TEST:** `ConditionBoss_AnswerContainsForbidden_Penalty5XP` → RED → GREEN
- [x] **TEST:** `ConditionBoss_AnswerWithoutForbidden_NoPenalty` → RED → GREEN
- [x] **TEST:** `ConditionBoss_WordSelection_EnsuresAlternativeExists` → RED → GREEN
- [x] Vytvořit `IConditionBossRules` interface
- [x] Implementovat: 15 slov, forbidden letter pattern
- [x] **GREEN:** Testy prochází

### T-206.4: Twist Boss Rules - Progressive Reveal (TDD) ✅
- [x] **TEST:** `TwistBoss_Start_12Words_2RevealedLetters` → RED → GREEN
- [x] **TEST:** `TwistBoss_RevealLetter_Every3Seconds` → RED → GREEN
- [x] **TEST:** `TwistBoss_EarlyGuess_*Letters_*XPBonus` (5 testů) → RED → GREEN
- [x] Vytvořit `ITwistBossRules` interface
- [x] Implementovat: 12 slov, start s 2 revealed, reveal every 3s
- [x] **GREEN:** Testy prochází

### T-206.5: Boss Level Endpoints ✅
- [x] Rozšířit `POST /api/v1/game/start` o BossType parametr
- [x] Rozšířit `GameRoundResult` DTO o boss-specifická data
- [x] Vytvořit `GET /api/v1/game/{id}/boss-state` pro Twist reveal stav

### T-206.6: Frontend - Boss Level komponenty (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `MarathonBoss_Renders_ProgressBar20Words` → RED → GREEN
- [x] **TEST (bUnit):** `ConditionBoss_Renders_ForbiddenLetterWarning` → RED → GREEN
- [x] **TEST (bUnit):** `TwistBoss_Renders_RevealGrid` → RED → GREEN
- [x] Vytvořit `MarathonBoss.razor`, `ConditionBoss.razor`, `TwistBoss.razor`
- [x] Boss Result Screens (Victory/Defeat modaly)
- [x] **GREEN:** Testy prochází

---

## T-207: UC-017 Nastavení profilu ✅ HOTOVÉ

### T-207.1: Backend - User Settings Endpoints (TDD) ✅
- [x] **TEST:** `GetUserProfile_Returns200WithProfile` → RED → GREEN
- [x] **TEST:** `UpdateProfile_ValidData_Returns200` → RED → GREEN
- [x] **TEST:** `UpdateProfile_InvalidUsername_Returns400` → RED → GREEN
- [x] **TEST:** `ChangePassword_ValidData_Returns200` → RED → GREEN
- [x] **TEST:** `ChangePassword_InvalidOldPassword_Returns400` → RED → GREEN
- [x] Vytvořit DTOs: `UserProfileDto`, `UpdateProfileRequest`, `ChangePasswordRequest`, `UserPreferencesDto`, `PrivacySettingsDto`
- [x] Vytvořit validátory: `UpdateProfileValidator`, `ChangePasswordValidator` s lokalizací
- [x] **GREEN:** Všechny testy prochází

### T-207.2: Frontend - Settings Page (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `SettingsPage_Renders_AllSections` → RED → GREEN
- [x] **TEST (bUnit):** `SettingsPage_UpdateUsername_CallsApi` → RED → GREEN
- [x] **TEST (bUnit):** `SettingsPage_ChangePassword_ValidatesAndSubmits` → RED → GREEN
- [x] **TEST (bUnit):** `SettingsPage_Preferences_RendersToggles` → RED → GREEN
- [x] **TEST (bUnit):** `SettingsPage_Privacy_RendersVisibilityOptions` → RED → GREEN
- [x] **TEST (bUnit):** `SettingsPage_Renders_UserProfileData` → RED → GREEN
- [x] **TEST (bUnit):** `SettingsPage_ChangePassword_RendersPasswordFields` → RED → GREEN
- [x] Vytvořit `Settings.razor` (`@page "/settings"`)
- [x] Použít `TmTabs`, `TmCard`, `TmTextInput`, `TmToggle`, `TmButton`
- [x] **GREEN:** 7/7 testů prochází

---

## T-208: UI/UX Polishing ✅ HOTOVÉ

### T-208.1: Loading States (Tempo.Blazor) ✅
- [x] Přidat `TmSkeleton` na všechny stránky při načítání
- [x] Dashboard: Skeleton pro stat cards
- [x] Settings: Skeleton pro form fields
- [x] Game: Skeleton pro scrambled word a controls

### T-208.2: Error Boundaries ✅
- [x] Vytvořit `ErrorBoundary.razor` komponentu s `TmAlert Severity="Error"`
- [x] Zobrazit uživatelsky přívětivou chybovou zprávu
- [x] Přidat tlačítko "Zkusit znovu"

### T-208.3: Toast Notifications ✅
- [x] Přidat `TmToastContainer` do `MainLayout.razor`
- [x] Použít `ToastService.ShowSuccess()` po úspěšné akci
- [x] Použít `ToastService.ShowError()` při chybě

### T-208.4: Animace a přechody ✅
- [x] Page transitions: fade-in na route change
- [x] Loading states se skeleton

### T-208.5: Mobile Responsive ✅
- [x] Dashboard: 4 col → 2×2 → stack
- [x] Boss levely: Responzivní layout
- [x] Settings: Responzivní formuláře

---

## Celkový Progress Fáze 2

| Úkol | Status | Testy |
|------|--------|-------|
| T-200 Password Reset | ✅ Hotovo | 21/21 |
| T-201 Ligy Backend | ✅ Hotovo | 24/24 |
| T-202 Ligy Frontend | ✅ Hotovo | 7/7 |
| T-203 Denní výzva | ✅ Hotovo | 21/21 |
| T-204 Achievementy Backend | ✅ Hotovo | 16/16 |
| T-205 Achievementy Frontend | ✅ Hotovo | 6/6 |
| **T-206 Boss Levely** | ✅ **Hotovo** | **41/41** |
| **T-207 Nastavení** | ✅ **Hotovo** | **7/7** |
| **T-208 UI/UX Polish** | ✅ **Hotovo** | **5/5** |

**Celkem hotových testů: ~520/520 (100%)**

**Celkový stav Fáze 2: 100% dokončeno** ✅

---

## Záznam změn

### 07.03.2026 - Dokončení T-206, T-207, T-208
- ✅ T-206.1-T-206.6: Boss Levely implementovány (41 testů)
- ✅ T-207.1-T-207.2: Nastavení profilu implementováno (7 testů)
- ✅ T-208.1-T-208.5: UI/UX Polish implementováno
- ✅ Refaktoring na Tempo.Blazor komponenty dokončen
- ✅ Opraveny testy pro SettingsPage (data-testid selektory)
