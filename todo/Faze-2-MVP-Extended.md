# Fáze 2: MVP Extended (Týden 3-4)

> **Cíl:** Obnova hesla, ligy, denní výzvy, achievementy, boss levely, nastavení profilu, UI polish
> **Závislost:** Fáze 1 kompletně dokončena
> **Tempo.Blazor komponenty:** TmDataTable, TmTabs, TmTabPanel, TmAccordion, TmModal, TmCard, TmBadge, TmToggle, TmRadioGroup, TmSelect, TmTimePicker, TmAvatar, TmAvatarGroup, TmChip, TmEmptyState, TmSkeleton, TmTooltip, TmProgressBar, TmAlert, TmButton, TmTextInput, TmFormField, TmFormSection, TmIcon, TmDrawer, FluentValidationValidator, ToastService

---

## ⚠️ KRITICKÁ PRAVIDLA

- **TDD:** Test FIRST → RED → GREEN → REFACTOR
- **Žádné hardcoded texty** → vše z `.resx`
- **FluentValidation** přes `FluentValidationValidator` na FE, `AbstractValidator<T>` s `IStringLocalizer` na BE
- **DTOs** v `LexiQuest.Shared`
- **HTTP status kódy** místo wrapper tříd
- **Produkční kód** od prvního řádku

---

## T-200: UC-003 Obnova hesla ✅

### T-200.1: Backend - PasswordResetService (TDD) ✅
- [x] **TEST:** `PasswordResetService_RequestReset_ValidEmail_GeneratesToken` → RED
- [x] **TEST:** `PasswordResetService_RequestReset_InvalidEmail_Returns200OK` (neodhaluje existenci) → RED
- [x] **TEST:** `PasswordResetService_RequestReset_TokenExpires_In1Hour` → RED
- [x] **TEST:** `PasswordResetService_ResetPassword_ValidToken_ChangesPassword` → RED
- [x] **TEST:** `PasswordResetService_ResetPassword_ExpiredToken_Returns400` → RED
- [x] **TEST:** `PasswordResetService_ResetPassword_UsedToken_Returns400` → RED
- [x] **TEST:** `PasswordResetService_ResetPassword_InvalidToken_Returns400` → RED
- [x] **TEST:** `PasswordResetService_ResetPassword_SameAsOld_Returns400` → RED
- [x] Vytvořit `IPasswordResetService` interface
- [x] Vytvořit `PasswordResetToken` entitu (Token, UserId, ExpiresAt, UsedAt)
- [x] EF Core konfigurace + migrace
- [x] Vytvořit DTOs v Shared: `RequestPasswordResetDto` (Email), `ResetPasswordDto` (Token, NewPassword, ConfirmPassword)
- [x] Implementovat `PasswordResetService` s token generací (secure random)
- [x] **GREEN:** Všechny testy prochází

### T-200.2: Backend - Email Service (TDD) ✅
- [x] **TEST:** `EmailService_SendPasswordReset_SendsEmail` → RED
- [x] **TEST:** `EmailService_SendWelcome_SendsEmail` → RED
- [x] Vytvořit `IEmailService` interface (SendPasswordResetEmail, SendWelcomeEmail)
- [x] Implementovat `EmailService` (SendGrid nebo SMTP)
- [x] Email templates z .resx souborů (`PasswordResetEmail.resx`, `WelcomeEmail.resx`)
- [x] **GREEN:** Testy prochází

### T-200.3: Backend - Password Reset Validators (TDD) ✅
- [x] **TEST:** `RequestPasswordResetValidator_EmptyEmail_ReturnsError` → RED
- [x] **TEST:** `ResetPasswordValidator_WeakPassword_ReturnsError` → RED
- [x] **TEST:** `ResetPasswordValidator_Mismatch_ReturnsError` → RED
- [x] Vytvořit validátory s lokalizovanými zprávami
- [x] **GREEN:** Testy prochází

### T-200.4: Backend - Endpoints ✅
- [x] Vytvořit `POST /api/v1/users/password-reset/request` (vždy vrací 200)
- [x] Vytvořit `POST /api/v1/users/password-reset/confirm` (validuje token + mění heslo)
- [x] **GREEN:** Testy prochází

### T-200.5: Frontend - Password Reset Pages (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `PasswordResetRequest_Renders_EmailField` → RED
- [x] **TEST (bUnit):** `PasswordResetConfirm_Renders_PasswordFields` → RED
- [x] Vytvořit `PasswordResetRequest.razor` (`@page "/password-reset"`)
  - `TmCard` (Elevated, centrovaný), `TmFormField` + `TmTextInput` pro email
  - `<FluentValidationValidator />`
  - `TmButton` pro odeslání, `TmAlert` pro potvrzení odeslání
- [x] Vytvořit `PasswordResetConfirm.razor` (`@page "/password-reset/{Token}"`)
  - `TmFormField` + `TmTextInput` pro nové heslo + `TmPasswordStrengthIndicator`
  - `TmFormField` + `TmTextInput` pro potvrzení hesla
  - `<FluentValidationValidator />`
  - `TmButton` pro odeslání, redirect na `/login` po úspěchu
- [x] Všechny texty z `PasswordReset.resx`
- [x] **GREEN:** Testy prochází

---

## T-201: UC-013 Ligy - Backend ✅

### T-201.1: Domain Entities (TDD) ✅
- [x] **TEST:** `League_Create_SetsCorrectDefaults` → RED
- [x] **TEST:** `LeagueParticipant_AddXP_UpdatesWeeklyXP` → RED
- [x] **TEST:** `LeagueParticipant_Rank_CalculatesCorrectly` → RED
- [x] Vytvořit `League` entitu (Id, Tier, WeekStart, WeekEnd, IsActive, Participants)
- [x] Vytvořit `LeagueParticipant` entitu (UserId, LeagueId, WeeklyXP, Rank, IsPromoted, IsDemoted)
- [x] Vytvořit `LeagueTier` enum (Bronze, Silver, Gold, Diamond, Legend)
- [x] Vytvořit `LeagueChangeStatus` enum (Promoted, Demoted, Stayed)
- [x] EF Core konfigurace + migrace
- [x] **GREEN:** Testy prochází

### T-201.2: LeagueService (TDD) ✅
- [x] **TEST:** `LeagueService_AssignNewUser_PlacesInBronze` → RED
- [x] **TEST:** `LeagueService_AssignUser_Bronze_Max30Participants` → RED
- [x] **TEST:** `LeagueService_GetCurrentLeague_ReturnsActiveLeague` → RED
- [x] **TEST:** `LeagueService_AddXP_UpdatesParticipantXP` → RED
- [x] **TEST:** `LeagueService_GetLeaderboard_ReturnsSortedByXP` → RED
- [x] **TEST:** `LeagueService_CalculatePromotions_Top5Promoted` → RED
- [x] **TEST:** `LeagueService_CalculateDemotions_Bottom5Demoted` → RED
- [x] **TEST:** `LeagueService_LegendTier_Top3Promoted` → RED
- [x] **TEST:** `LeagueService_LegendTier_Bottom10Demoted` → RED
- [x] **TEST:** `LeagueService_GetRewards_BronzeTier_Returns50XP` → RED
- [x] **TEST:** `LeagueService_GetRewards_DiamondTier_Returns500XP` → RED
- [x] Vytvořit `ILeagueService` interface
- [x] Vytvořit DTOs: `LeagueInfoDto`, `LeagueParticipantDto`, `LeagueLeaderboardDto`
- [x] Implementovat `LeagueService`
- [x] **GREEN:** Všechny testy prochází

### T-201.3: Weekly League Reset Job (TDD) ✅
- [x] **TEST:** `LeagueResetJob_Execute_CreatesNewWeekLeagues` → RED
- [x] **TEST:** `LeagueResetJob_Execute_MovesPromotedUsersUp` → RED
- [x] **TEST:** `LeagueResetJob_Execute_MovesDemotedUsersDown` → RED
- [x] **TEST:** `LeagueResetJob_Execute_ResetsWeeklyXP` → RED
- [x] Vytvořit `LeagueResetJob` (Hangfire RecurringJob, běží v pondělí 00:00 UTC)
- [x] Implementovat promotion/demotion logic
- [x] Implementovat nové přiřazení do skupin (30 hráčů)
- [x] **GREEN:** Testy prochází
- [x] Zaregistrovat Hangfire job v Program.cs

### T-201.4: League Endpoints ✅
- [x] **TEST:** `GetCurrentLeagueEndpoint_Returns200` → RED
- [x] **TEST:** `GetLeagueHistoryEndpoint_ReturnsPastLeagues` → RED
- [x] Vytvořit `GET /api/v1/leagues/current` endpoint
- [x] Vytvořit `GET /api/v1/leagues/history` endpoint
- [x] **GREEN:** Testy prochází

---

## T-202: UC-013 Ligy - Frontend ✅

### T-202.1: LeagueService Frontend ✅
- [x] **TEST:** `LeagueService_GetCurrentLeague_ReturnsLeagueInfo` → RED
- [x] Implementovat `LeagueService` v Blazor/Services/
- [x] **GREEN:** Test prochází

### T-202.2: Leagues Page (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `LeaguesPage_Renders_LeagueHeaderWithTier` → RED
- [x] **TEST (bUnit):** `LeaguesPage_Renders_UserPositionCard` → RED
- [x] **TEST (bUnit):** `LeaguesPage_Renders_Leaderboard` → RED
- [x] Vytvořit `Leagues.razor` stránku (`@page "/leagues"`)
- [x] `@inject IStringLocalizer<Leagues> L`
- [x] **League Header**: `TmCard` s `TmIcon` (medal/trophy), tier name, week info, countdown timer
- [x] **User Position Card**: `TmCard` (Elevated) s rank číslem, `TmAvatar`, XP, `TmProgressBar` k promo/demo
- [x] **Leaderboard**: Použít vlastní list (ne TmDataTable - leaderboard je vizuálně specifický)
  - Řádky s pozicí, `TmAvatar`, username, XP, trend `TmIcon` (▲/→/▼)
  - Medaile 🥇🥈🥉 pro top 3
  - Zvýraznění aktuálního uživatele (primary-50 bg, 2px border)
  - Promo zóna: zelený pozadí (top 5)
  - Demo zóna: červené pozadí (bottom 5)
- [x] **League History**: `TmAccordion` s předchozími týdny
- [x] **Rewards sekce**: `TmCard` s odměnami per tier
- [x] Countdown timer: zelená >24h, oranžová <24h (pulse), červená <6h (pulse+shake)
- [x] **GREEN:** Testy prochází
- [x] **REFACTOR:** Styling dle UI-UX-008

---

## T-203: UC-014 Denní výzva ✅

### T-203.1: Backend - DailyChallengeService (TDD) ✅
- [x] **TEST:** `DailyChallengeService_GetToday_ReturnsChallengeForCurrentDate` → RED
- [x] **TEST:** `DailyChallengeService_GetToday_SameWordForAllUsers` → RED
- [x] **TEST:** `DailyChallengeService_GetModifier_Monday_ReturnsCategory` → RED
- [x] **TEST:** `DailyChallengeService_GetModifier_Tuesday_ReturnsSpeed` → RED
- [x] **TEST:** `DailyChallengeService_GetModifier_Wednesday_ReturnsNoHints` → RED
- [x] **TEST:** `DailyChallengeService_GetModifier_Thursday_ReturnsDoubleLetters` → RED
- [x] **TEST:** `DailyChallengeService_GetModifier_Friday_ReturnsTeam` → RED
- [x] **TEST:** `DailyChallengeService_GetModifier_Saturday_ReturnsHard` → RED
- [x] **TEST:** `DailyChallengeService_GetModifier_Sunday_ReturnsEasy` → RED
- [x] **TEST:** `DailyChallengeService_SubmitChallenge_CalculatesXPWithModifier` → RED
- [x] **TEST:** `DailyChallengeService_AlreadyCompleted_ReturnsForbidden` → RED
- [x] Vytvořit `IDailyChallengeService` interface
- [x] Vytvořit `DailyChallenge` entitu (Date, WordId, Modifier, CreatedAt)
- [x] Vytvořit `DailyChallengeCompletion` entitu (UserId, ChallengeDate, TimeMs, XPEarned)
- [x] Vytvořit `DailyModifier` enum (Category, Speed, NoHints, DoubleLetters, Team, Hard, Easy)
- [x] Vytvořit DTOs: `DailyChallengeDto`, `DailyLeaderboardEntryDto`
- [x] EF Core konfigurace + migrace
- [x] Implementovat `DailyChallengeService` s daily selection (deterministický seed z data)
- [x] **GREEN:** Všechny testy prochází

### T-203.2: Backend - Daily Challenge Endpoints ✅
- [x] Vytvořit `GET /api/v1/game/daily` (vrací dnešní challenge info)
- [x] Vytvořit `POST /api/v1/game/daily/start` (startuje challenge session)
- [x] Vytvořit `GET /api/v1/game/daily/leaderboard` (denní žebříček)

### T-203.3: Frontend - DailyChallenge Page (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `DailyChallengePage_Renders_TodaysChallenge` → RED
- [x] **TEST (bUnit):** `DailyChallengePage_ShowsModifier_Badge` → RED
- [x] **TEST (bUnit):** `DailyChallengePage_Completed_ShowsResults` → RED
- [x] Vytvořit `DailyChallenge.razor` (`@page "/daily-challenge"`)
- [x] `@inject IStringLocalizer<DailyChallenge> L`
- [x] Header: `TmBadge` s modifier (barva dle typu), countdown do resetu
- [x] Challenge info: `TmCard` s modifier pravidly, XP multiplikátor
- [x] Play button: `TmButton` (Primary, Block)
- [x] Completed state: `TmCard` s výsledky (čas, XP, rank v leaderboardu)
- [x] Leaderboard: List s top 10 hráči (čas, XP), `TmAvatar` + username
- [x] **GREEN:** Testy prochází

---

## T-204: UC-016 Achievementy - Backend ✅

### T-204.1: Domain Entities (TDD) ✅
- [x] **TEST:** `Achievement_Create_SetsProperties` → RED
- [x] **TEST:** `UserAchievement_Unlock_SetsUnlockedAt` → RED
- [x] Vytvořit `Achievement` entitu (Id, Key, Category, XPReward, IconName, RequiredValue)
- [x] Vytvořit `UserAchievement` entitu (UserId, AchievementId, UnlockedAt, Progress)
- [x] Vytvořit `AchievementCategory` enum (Performance, Streak, Difficulty, Special)
- [x] EF Core konfigurace + migrace
- [x] Seed data pro všechny achievementy (First Word, 100 Words, 1K Words, Streak 3/7/14/30/365, Path completions, atd.)
- [x] **GREEN:** Testy prochází

### T-204.2: AchievementService (TDD) ✅
- [x] **TEST:** `AchievementService_CheckWordSolved_FirstWord_UnlocksAchievement` → RED
- [x] **TEST:** `AchievementService_CheckWordSolved_100Words_UnlocksAchievement` → RED
- [x] **TEST:** `AchievementService_CheckStreak_3Days_UnlocksAchievement` → RED
- [x] **TEST:** `AchievementService_CheckStreak_7Days_UnlocksAchievementAnd50XP` → RED
- [x] **TEST:** `AchievementService_CheckPathComplete_UnlocksAchievement` → RED
- [x] **TEST:** `AchievementService_CheckPerfectBoss_UnlocksAchievement` → RED
- [x] **TEST:** `AchievementService_GetProgress_ReturnsCorrectPercentage` → RED
- [x] **TEST:** `AchievementService_AlreadyUnlocked_DoesNotDuplicate` → RED
- [x] Vytvořit `IAchievementService` interface
- [x] Vytvořit DTOs: `AchievementDto`, `UserAchievementDto`, `AchievementProgressDto`
- [x] Implementovat `AchievementService` s event-driven checking
- [x] Zavolat po: WordSolved, LevelCompleted, StreakUpdated, PathCompleted, BossDefeated
- [x] **GREEN:** Všechny testy prochází

### T-204.3: Achievement Endpoints ✅
- [x] Vytvořit `GET /api/v1/achievements` (všechny achievementy s progress)
- [x] Vytvořit `GET /api/v1/achievements/{id}` (detail)
- [x] Vytvořit `GET /api/v1/users/me/achievements` (uživatelovy achievementy)

---

## T-205: UC-016 Achievementy - Frontend ✅

### T-205.1: Achievements Page (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `AchievementsPage_Renders_ProgressBar` → RED
- [x] **TEST (bUnit):** `AchievementsPage_Renders_CategoryTabs` → RED
- [x] **TEST (bUnit):** `AchievementsPage_FiltersByCategory` → RED
- [x] Vytvořit `Achievements.razor` (`@page "/achievements"`)
- [x] `@inject IStringLocalizer<Achievements> L`
- [x] Progress header: `TmProgressBar` s "12/50 unlocked" textem
- [x] Category filter: `TmTabs` + `TmTabPanel` (All, Performance 🎯, Streak 🔥, Difficulty 🧠, Special 🏆)
- [x] Grid karet s achievementy
- [x] **GREEN:** Test prochází

### T-205.2: AchievementCard komponenta (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `AchievementCard_Unlocked_ShowsGoldBorder` → RED
- [x] **TEST (bUnit):** `AchievementCard_InProgress_ShowsProgressBar` → RED
- [x] **TEST (bUnit):** `AchievementCard_Locked_ShowsLockIcon` → RED
- [x] Vytvořit `AchievementCard.razor`
- [x] Unlocked state: `TmCard` se zlatým pozadím (gold-50), `TmBadge` (Success), `TmIcon` barevná, datum odemčení, XP reward
- [x] In Progress state: `TmCard` bílá, `TmIcon` šedá (30% opacity), `TmProgressBar` s %, zbývající požadavek
- [x] Locked state: `TmCard` šedá (gray-100), `TmIcon` (lock), dashed border, opacity 0.5, requirements text
- [x] **GREEN:** Testy prochází

### T-205.3: AchievementUnlock Modal (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `AchievementUnlockModal_Renders_AchievementInfo` → RED
- [x] Vytvořit `AchievementUnlockModal.razor`
- [x] `TmModal` (Size: Medium) s backdrop blur
- [x] Badge icon s glow efektem (CSS box-shadow animation)
- [x] Confetti burst animace
- [x] Achievement name, description, XP reward
- [x] `TmButton` "Skvělé!" → zavře modal
- [x] **GREEN:** Test prochází

---

## T-206: UC-008,009,010 Boss Levely 🔄

### T-206.1: Backend - Boss Types v GameSession (TDD) ✅
- [x] **TEST:** `GameSession_CreateBoss_Marathon_Sets20WordsAnd3Lives` → RED
- [x] **TEST:** `GameSession_CreateBoss_Condition_SetsForbiddenLetterPattern` → RED
- [x] **TEST:** `GameSession_CreateBoss_Twist_SetsRevealMechanic` → RED
- [x] Vytvořit `BossType` enum (Marathon, Condition, Twist)
- [x] Rozšířit `GameSession` o BossType? property
- [x] Rozšířit `GameRound` o ForbiddenLetters, RevealedLettersCount
- [x] EF Core migrace
- [x] **GREEN:** Testy prochází

### T-206.2: Marathon Boss Rules (TDD) ✅
- [x] **TEST:** `MarathonBoss_Start_20Words3Lives` → RED
- [x] **TEST:** `MarathonBoss_WrongAnswer_DecreasesLife_NoRegen` → RED
- [x] **TEST:** `MarathonBoss_0Lives_GameOver` → RED
- [x] **TEST:** `MarathonBoss_AllCorrect_PerfectBonus200XP` → RED
- [x] **TEST:** `MarathonBoss_Completed_WithLosses_100XP` → RED
- [x] **TEST:** `MarathonBoss_Under5Min_SpeedBonus50XP` → RED
- [x] Vytvořit `IMarathonBossRules` interface
- [x] Implementovat pravidla: 20 slov, 3 životy, žádná regenerace, 15s per slovo
- [x] **GREEN:** Všechny testy prochází

### T-206.3: Condition Boss Rules - Forbidden Letter (TDD) ✅
- [x] **TEST:** `ConditionBoss_Start_15Words` → RED
- [x] **TEST:** `ConditionBoss_Every3rdWord_HasForbiddenLetter` → RED
- [x] **TEST:** `ConditionBoss_AnswerContainsForbidden_Penalty5XP` → RED
- [x] **TEST:** `ConditionBoss_AnswerWithoutForbidden_NoPenalty` → RED
- [x] **TEST:** `ConditionBoss_WordSelection_EnsuresAlternativeExists` → RED
- [x] Vytvořit `IConditionBossRules` interface
- [x] Implementovat: 15 slov, forbidden letter pattern (1-2 normal, 3 forbidden, 4-5 normal, 6 forbidden...)
- [x] Word selection: ověřit že existuje validní odpověď bez forbidden letter
- [x] **GREEN:** Testy prochází

### T-206.4: Twist Boss Rules - Progressive Reveal (TDD) ✅
- [x] **TEST:** `TwistBoss_Start_12Words_2RevealedLetters` → RED
- [x] **TEST:** `TwistBoss_RevealLetter_Every3Seconds` → RED
- [x] **TEST:** `TwistBoss_EarlyGuess_2Letters_10XPBonus` → RED
- [x] **TEST:** `TwistBoss_EarlyGuess_3Letters_7XPBonus` → RED
- [x] **TEST:** `TwistBoss_EarlyGuess_4Letters_5XPBonus` → RED
- [x] **TEST:** `TwistBoss_EarlyGuess_5Letters_2XPBonus` → RED
- [x] **TEST:** `TwistBoss_AllRevealed_0XPBonus` → RED
- [x] Vytvořit `ITwistBossRules` interface
- [x] Implementovat: 12 slov, start s 2 revealed, reveal every 3s, max 5 revealed
- [x] Vytvořit `TwistRoundState` DTO (RevealedPositions, NextRevealAt, BonusXP)
- [x] **GREEN:** Testy prochází

### T-206.5: Boss Service & Endpoints (TDD) ✅
- [x] **TEST:** `BossService_StartBossGame_Marathon_ReturnsCorrectSettings` → RED
- [x] **TEST:** `BossService_StartBossGame_Condition_ReturnsCorrectSettings` → RED
- [x] **TEST:** `BossService_StartBossGame_Twist_ReturnsCorrectSettings` → RED
- [x] **TEST:** `BossService_GetBossRules_Marathon_ReturnsMarathonRules` → RED
- [x] Rozšířit `POST /api/v1/game/start` o BossType parametr
- [x] Rozšířit `GameRoundResult` DTO o boss-specifická data (ForbiddenLetter, RevealedPositions, BonusXP)
- [x] Vytvořit `GET /api/v1/game/{id}/boss-state` pro Twist reveal stav
- [x] **GREEN:** Testy prochází

### T-206.6: Frontend - Boss Level komponenty (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `MarathonBoss_Renders_ProgressBar20Words` → RED
- [x] **TEST (bUnit):** `ConditionBoss_Renders_ForbiddenLetterWarning` → RED
- [x] **TEST (bUnit):** `TwistBoss_Renders_RevealGrid` → RED
- [x] Vytvořit `MarathonBoss.razor` - extends GameArena s 16/20 progress, hearts bez regen, dark red-orange header
- [x] Vytvořit `ConditionBoss.razor` - warning box: `TmAlert Severity="Warning"` s "🚫 ZAKÁZANÉ PÍSMENO: {X}", dashed red border, pulse animation
- [x] Vytvořit `TwistBoss.razor` - reveal grid s pozicemi (shown/hidden), reveal timer `TmProgressBar`, early guess bonus display
- [x] Boss Result Screens:
  - Victory: Modal s stats (time, accuracy, lives, combo), XP rewards
  - Defeat: Modal s dark theme, motivační zpráva, `TmButton` "Zkusit znovu"
- [x] **GREEN:** Build prochází (testy vyžadují finalizaci API)
- [x] **REFACTOR:** Styling dle UI-UX-007

---

## T-207: UC-017 Nastavení profilu 🔄

### T-207.1: Backend - User Settings Endpoints (TDD) ✅
- [x] **TEST:** `GetUserProfile_Returns200WithProfile` → RED
- [x] **TEST:** `UpdateProfile_ValidData_Returns200` → RED
- [x] **TEST:** `UpdateProfile_DuplicateUsername_Returns409` → RED
- [x] **TEST:** `ChangePassword_ValidOldPassword_Returns200` → RED
- [x] **TEST:** `ChangePassword_InvalidOldPassword_Returns400` → RED
- [x] Vytvořit DTOs: `UserProfileDto`, `UpdateProfileRequest`, `ChangePasswordRequest`, `UserPreferencesDto`, `PrivacySettingsDto`
- [x] Vytvořit validátory: `UpdateProfileValidator`, `ChangePasswordValidator` s lokalizací
- [x] Vytvořit `GET /api/v1/users/me` endpoint
- [x] Vytvořit `PUT /api/v1/users/me` endpoint
- [x] Vytvořit `PUT /api/v1/users/me/password` endpoint
- [x] Vytvořit `PUT /api/v1/users/me/preferences` endpoint
- [x] Vytvořit `PUT /api/v1/users/me/privacy` endpoint
- [x] Vytvořit `POST /api/v1/users/me/avatar` endpoint (file upload)
- [x] **GREEN:** Testy prochází

### T-207.2: Frontend - Settings Page (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `SettingsPage_Renders_AllSections` → RED
- [x] **TEST (bUnit):** `SettingsPage_UpdateUsername_CallsApi` → RED
- [x] **TEST (bUnit):** `SettingsPage_ChangePassword_ValidatesAndSubmits` → RED
- [x] Vytvořit `Settings.razor` (`@page "/settings"`)
- [x] `@inject IStringLocalizer<Settings> L`
- [x] Layout: `TmTabs` s panely pro každou sekci

- [x] **Profil sekce** (`TmTabPanel`):
  - `TmAvatar` s upload tlačítkem (preview + crop)
  - `TmFormField` + `TmTextInput` pro Username (s debounced availability check → `TmIcon` check/x)
  - `TmFormField` + `TmTextInput` pro Email
  - `<FluentValidationValidator />`
  - `TmButton` "Uložit" (Primary)

- [x] **Heslo sekce** (`TmTabPanel`):
  - `TmFormField` + `TmTextInput` Type="password" pro aktuální heslo
  - `TmFormField` + `TmTextInput` Type="password" pro nové heslo + `TmPasswordStrengthIndicator`
  - `TmFormField` + `TmTextInput` Type="password" pro potvrzení
  - `<FluentValidationValidator />`
  - `TmButton` "Změnit heslo" (Primary)

- [x] **Notifikace sekce** (`TmTabPanel`):
  - `TmToggle` Push notifikace
  - `TmToggle` Email notifikace
  - `TmTimePicker` Streak reminder čas
  - `TmToggle` League updates
  - `TmToggle` Achievement notifications
  - `TmToggle` Daily challenge reminder

- [x] **Zobrazení sekce** (`TmTabPanel`):
  - `TmRadioGroup` Theme: Light / Dark / Auto (→ `ThemeService.Toggle()`)
  - `TmSelect` Jazyk (CS, EN, DE)
  - `TmToggle` Animace on/off
  - `TmToggle` Zvuky on/off

- [x] **Soukromí sekce** (`TmTabPanel`):
  - `TmRadioGroup` Profil visibility: Public / Friends / Private
  - `TmToggle` Leaderboard visibility
  - `TmToggle` Stats sharing

- [x] **Danger Zone** (`TmTabPanel`):
  - `TmButton Variant="Outline"` Odhlásit se
  - `TmButton Variant="Danger"` Deaktivovat účet (s confirm `TmModal`)
  - `TmButton Variant="Danger"` Smazat účet a data (s double confirm `TmModal` + type "DELETE")

- [x] Všechny texty z `Settings.resx`
- [x] `ToastService.ShowSuccess()` po úspěšné změně
- [x] **GREEN:** Testy prochází
- [x] **REFACTOR:** Styling dle UI-UX-010

---

## T-208: UI/UX Polishing 🔄

### T-208.1: Loading States (Tempo.Blazor) ✅
- [x] Přidat `TmSkeleton` (Text, Circle, Rectangle varianty) na všechny stránky při načítání
- [x] Dashboard: 4× Skeleton rectangle pro stat cards + skeleton karty
- [x] Leagues: Skeleton pro leaderboard řádky
- [x] Achievements: Skeleton grid pro achievement karty
- [x] Game: Skeleton pro scrambled word a controls
- [x] Settings: Skeleton pro form fields

### T-208.2: Error Boundaries ✅
- [x] Vytvořit `ErrorBoundary.razor` komponentu s `TmAlert Severity="Error"`
- [x] Texty z `ErrorBoundary.resx`
- [x] Retry `TmButton`
- [x] Zabalit každou stránku do ErrorBoundary
- [x] Implementovat global error handler pro unhandled exceptions

### T-208.3: Toast Notifications ✅
- [x] Přidat `TmToastContainer` do `MainLayout.razor`
- [x] Vytvořit `NotificationHelper` service pro standardizované toasty
- [x] Success toasty: XP gained, level up, achievement unlocked
- [x] Error toasty: API errors, connection lost
- [x] Warning toasty: streak ending, lives low
- [x] Info toasty: daily challenge available
- [x] Auto-dismiss po 5s, max 3 viditelné

### T-208.4: Animace a přechody ✅
- [x] Page transitions: fade-in na route change
- [x] Stagger animations: sekvenční load elementů na dashboard (100ms intervals)
- [x] Count-up animace pro čísla (XP, streak, accuracy)
- [x] Hover efekty na kartách: scale 1.02, shadow increase
- [x] `@media (prefers-reduced-motion: reduce)` → vypnout animace

### T-208.5: Mobile Responsive ✅
- [x] Dashboard: 4 col → 2×2 → stack
- [x] Paths: horizontal scroll → vertical stack
- [x] Leaderboard: compact mode s menšími avatary
- [x] Game: menší písmena (36px), full-width input
- [x] Navigation: `TmSidebar` collapsible na mobilu, hamburger menu
- [x] Modaly: full-screen na mobilu (`TmModal Size="FullScreen"` na breakpoint < 640px)

---

## Ověření dokončení fáze

- [x] Password reset flow kompletní (request → email → reset → login)
- [x] Ligy fungují: přiřazení, leaderboard, weekly reset, promotion/demotion
- [x] Denní výzva: daily word, modifiers, leaderboard
- [x] Achievementy: tracking, unlock, progress, categories
- [x] Boss levely: Marathon (20 slov), Condition (forbidden letter), Twist (reveal)
- [x] Settings: profil, heslo, notifikace, theme, privacy, danger zone
- [x] Loading states se skeleton na všech stránkách
- [x] Error boundaries s retry
- [x] Toast notifikace fungují
- [x] Responsive na mobilu
- [x] Všechny texty z .resx
- [ ] `dotnet test` → všechny testy zelené
