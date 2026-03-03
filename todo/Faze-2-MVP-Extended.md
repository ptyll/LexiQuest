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

## T-200: UC-003 Obnova hesla

### T-200.1: Backend - PasswordResetService (TDD)
- [ ] **TEST:** `PasswordResetService_RequestReset_ValidEmail_GeneratesToken` → RED
- [ ] **TEST:** `PasswordResetService_RequestReset_InvalidEmail_Returns200OK` (neodhaluje existenci) → RED
- [ ] **TEST:** `PasswordResetService_RequestReset_TokenExpires_In1Hour` → RED
- [ ] **TEST:** `PasswordResetService_ResetPassword_ValidToken_ChangesPassword` → RED
- [ ] **TEST:** `PasswordResetService_ResetPassword_ExpiredToken_Returns400` → RED
- [ ] **TEST:** `PasswordResetService_ResetPassword_UsedToken_Returns400` → RED
- [ ] **TEST:** `PasswordResetService_ResetPassword_InvalidToken_Returns400` → RED
- [ ] **TEST:** `PasswordResetService_ResetPassword_SameAsOld_Returns400` → RED
- [ ] Vytvořit `IPasswordResetService` interface
- [ ] Vytvořit `PasswordResetToken` entitu (Token, UserId, ExpiresAt, UsedAt)
- [ ] EF Core konfigurace + migrace
- [ ] Vytvořit DTOs v Shared: `RequestPasswordResetDto` (Email), `ResetPasswordDto` (Token, NewPassword, ConfirmPassword)
- [ ] Implementovat `PasswordResetService` s token generací (secure random)
- [ ] **GREEN:** Všechny testy prochází

### T-200.2: Backend - Email Service (TDD)
- [ ] **TEST:** `EmailService_SendPasswordReset_SendsEmail` → RED
- [ ] **TEST:** `EmailService_SendWelcome_SendsEmail` → RED
- [ ] Vytvořit `IEmailService` interface (SendPasswordResetEmail, SendWelcomeEmail)
- [ ] Implementovat `EmailService` (SendGrid nebo SMTP)
- [ ] Email templates z .resx souborů (`PasswordResetEmail.resx`, `WelcomeEmail.resx`)
- [ ] **GREEN:** Testy prochází

### T-200.3: Backend - Password Reset Validators (TDD)
- [ ] **TEST:** `RequestPasswordResetValidator_EmptyEmail_ReturnsError` → RED
- [ ] **TEST:** `ResetPasswordValidator_WeakPassword_ReturnsError` → RED
- [ ] **TEST:** `ResetPasswordValidator_Mismatch_ReturnsError` → RED
- [ ] Vytvořit validátory s lokalizovanými zprávami
- [ ] **GREEN:** Testy prochází

### T-200.4: Backend - Endpoints
- [ ] Vytvořit `POST /api/v1/users/password-reset/request` (vždy vrací 200)
- [ ] Vytvořit `POST /api/v1/users/password-reset/confirm` (validuje token + mění heslo)
- [ ] **GREEN:** Testy prochází

### T-200.5: Frontend - Password Reset Pages (Tempo.Blazor)
- [ ] **TEST (bUnit):** `PasswordResetRequest_Renders_EmailField` → RED
- [ ] **TEST (bUnit):** `PasswordResetConfirm_Renders_PasswordFields` → RED
- [ ] Vytvořit `PasswordResetRequest.razor` (`@page "/password-reset"`)
  - `TmCard` (Elevated, centrovaný), `TmFormField` + `TmTextInput` pro email
  - `<FluentValidationValidator />`
  - `TmButton` pro odeslání, `TmAlert` pro potvrzení odeslání
- [ ] Vytvořit `PasswordResetConfirm.razor` (`@page "/password-reset/{Token}"`)
  - `TmFormField` + `TmTextInput` pro nové heslo + `TmPasswordStrengthIndicator`
  - `TmFormField` + `TmTextInput` pro potvrzení hesla
  - `<FluentValidationValidator />`
  - `TmButton` pro odeslání, redirect na `/login` po úspěchu
- [ ] Všechny texty z `PasswordReset.resx`
- [ ] **GREEN:** Testy prochází

---

## T-201: UC-013 Ligy - Backend

### T-201.1: Domain Entities (TDD)
- [ ] **TEST:** `League_Create_SetsCorrectDefaults` → RED
- [ ] **TEST:** `LeagueParticipant_AddXP_UpdatesWeeklyXP` → RED
- [ ] **TEST:** `LeagueParticipant_Rank_CalculatesCorrectly` → RED
- [ ] Vytvořit `League` entitu (Id, Tier, WeekStart, WeekEnd, IsActive, Participants)
- [ ] Vytvořit `LeagueParticipant` entitu (UserId, LeagueId, WeeklyXP, Rank, IsPromoted, IsDemoted)
- [ ] Vytvořit `LeagueTier` enum (Bronze, Silver, Gold, Diamond, Legend)
- [ ] Vytvořit `LeagueChangeStatus` enum (Promoted, Demoted, Stayed)
- [ ] EF Core konfigurace + migrace
- [ ] **GREEN:** Testy prochází

### T-201.2: LeagueService (TDD)
- [ ] **TEST:** `LeagueService_AssignNewUser_PlacesInBronze` → RED
- [ ] **TEST:** `LeagueService_AssignUser_Bronze_Max30Participants` → RED
- [ ] **TEST:** `LeagueService_GetCurrentLeague_ReturnsActiveLeague` → RED
- [ ] **TEST:** `LeagueService_AddXP_UpdatesParticipantXP` → RED
- [ ] **TEST:** `LeagueService_GetLeaderboard_ReturnsSortedByXP` → RED
- [ ] **TEST:** `LeagueService_CalculatePromotions_Top5Promoted` → RED
- [ ] **TEST:** `LeagueService_CalculateDemotions_Bottom5Demoted` → RED
- [ ] **TEST:** `LeagueService_LegendTier_Top3Promoted` → RED
- [ ] **TEST:** `LeagueService_LegendTier_Bottom10Demoted` → RED
- [ ] **TEST:** `LeagueService_GetRewards_BronzeTier_Returns50XP` → RED
- [ ] **TEST:** `LeagueService_GetRewards_DiamondTier_Returns500XP` → RED
- [ ] Vytvořit `ILeagueService` interface
- [ ] Vytvořit DTOs: `LeagueInfoDto`, `LeagueParticipantDto`, `LeagueLeaderboardDto`
- [ ] Implementovat `LeagueService`
- [ ] **GREEN:** Všechny testy prochází

### T-201.3: Weekly League Reset Job (TDD)
- [ ] **TEST:** `LeagueResetJob_Execute_CreatesNewWeekLeagues` → RED
- [ ] **TEST:** `LeagueResetJob_Execute_MovesPromotedUsersUp` → RED
- [ ] **TEST:** `LeagueResetJob_Execute_MovesDemotedUsersDown` → RED
- [ ] **TEST:** `LeagueResetJob_Execute_ResetsWeeklyXP` → RED
- [ ] Vytvořit `LeagueResetJob` (Hangfire RecurringJob, běží v pondělí 00:00 UTC)
- [ ] Implementovat promotion/demotion logic
- [ ] Implementovat nové přiřazení do skupin (30 hráčů)
- [ ] **GREEN:** Testy prochází
- [ ] Zaregistrovat Hangfire job v Program.cs

### T-201.4: League Endpoints
- [ ] **TEST:** `GetCurrentLeagueEndpoint_Returns200` → RED
- [ ] **TEST:** `GetLeagueHistoryEndpoint_ReturnsPastLeagues` → RED
- [ ] Vytvořit `GET /api/v1/leagues/current` endpoint
- [ ] Vytvořit `GET /api/v1/leagues/history` endpoint
- [ ] **GREEN:** Testy prochází

---

## T-202: UC-013 Ligy - Frontend

### T-202.1: LeagueService Frontend
- [ ] **TEST:** `LeagueService_GetCurrentLeague_ReturnsLeagueInfo` → RED
- [ ] Implementovat `LeagueService` v Blazor/Services/
- [ ] **GREEN:** Test prochází

### T-202.2: Leagues Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `LeaguesPage_Renders_LeagueHeaderWithTier` → RED
- [ ] **TEST (bUnit):** `LeaguesPage_Renders_UserPositionCard` → RED
- [ ] **TEST (bUnit):** `LeaguesPage_Renders_Leaderboard` → RED
- [ ] Vytvořit `Leagues.razor` stránku (`@page "/leagues"`)
- [ ] `@inject IStringLocalizer<Leagues> L`
- [ ] **League Header**: `TmCard` s `TmIcon` (medal/trophy), tier name, week info, countdown timer
- [ ] **User Position Card**: `TmCard` (Elevated) s rank číslem, `TmAvatar`, XP, `TmProgressBar` k promo/demo
- [ ] **Leaderboard**: Použít vlastní list (ne TmDataTable - leaderboard je vizuálně specifický)
  - Řádky s pozicí, `TmAvatar`, username, XP, trend `TmIcon` (▲/→/▼)
  - Medaile 🥇🥈🥉 pro top 3
  - Zvýraznění aktuálního uživatele (primary-50 bg, 2px border)
  - Promo zóna: zelený pozadí (top 5)
  - Demo zóna: červené pozadí (bottom 5)
- [ ] **League History**: `TmAccordion` s předchozími týdny
- [ ] **Rewards sekce**: `TmCard` s odměnami per tier
- [ ] Countdown timer: zelená >24h, oranžová <24h (pulse), červená <6h (pulse+shake)
- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Styling dle UI-UX-008

---

## T-203: UC-014 Denní výzva

### T-203.1: Backend - DailyChallengeService (TDD)
- [ ] **TEST:** `DailyChallengeService_GetToday_ReturnsChallengeForCurrentDate` → RED
- [ ] **TEST:** `DailyChallengeService_GetToday_SameWordForAllUsers` → RED
- [ ] **TEST:** `DailyChallengeService_GetModifier_Monday_ReturnsCategory` → RED
- [ ] **TEST:** `DailyChallengeService_GetModifier_Tuesday_ReturnsSpeed` → RED
- [ ] **TEST:** `DailyChallengeService_GetModifier_Wednesday_ReturnsNoHints` → RED
- [ ] **TEST:** `DailyChallengeService_GetModifier_Thursday_ReturnsDoubleLetters` → RED
- [ ] **TEST:** `DailyChallengeService_GetModifier_Friday_ReturnsTeam` → RED
- [ ] **TEST:** `DailyChallengeService_GetModifier_Saturday_ReturnsHard` → RED
- [ ] **TEST:** `DailyChallengeService_GetModifier_Sunday_ReturnsEasy` → RED
- [ ] **TEST:** `DailyChallengeService_SubmitChallenge_CalculatesXPWithModifier` → RED
- [ ] **TEST:** `DailyChallengeService_AlreadyCompleted_ReturnsForbidden` → RED
- [ ] Vytvořit `IDailyChallengeService` interface
- [ ] Vytvořit `DailyChallenge` entitu (Date, WordId, Modifier, CreatedAt)
- [ ] Vytvořit `DailyChallengeCompletion` entitu (UserId, ChallengeDate, TimeMs, XPEarned)
- [ ] Vytvořit `DailyModifier` enum (Category, Speed, NoHints, DoubleLetters, Team, Hard, Easy)
- [ ] Vytvořit DTOs: `DailyChallengeDto`, `DailyLeaderboardEntryDto`
- [ ] EF Core konfigurace + migrace
- [ ] Implementovat `DailyChallengeService` s daily selection (deterministický seed z data)
- [ ] **GREEN:** Všechny testy prochází

### T-203.2: Backend - Daily Challenge Endpoints
- [ ] Vytvořit `GET /api/v1/game/daily` (vrací dnešní challenge info)
- [ ] Vytvořit `POST /api/v1/game/daily/start` (startuje challenge session)
- [ ] Vytvořit `GET /api/v1/game/daily/leaderboard` (denní žebříček)

### T-203.3: Frontend - DailyChallenge Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `DailyChallengePage_Renders_TodaysChallenge` → RED
- [ ] **TEST (bUnit):** `DailyChallengePage_ShowsModifier_Badge` → RED
- [ ] **TEST (bUnit):** `DailyChallengePage_Completed_ShowsResults` → RED
- [ ] Vytvořit `DailyChallenge.razor` (`@page "/daily-challenge"`)
- [ ] `@inject IStringLocalizer<DailyChallenge> L`
- [ ] Header: `TmBadge` s modifier (barva dle typu), countdown do resetu
- [ ] Challenge info: `TmCard` s modifier pravidly, XP multiplikátor
- [ ] Play button: `TmButton` (Primary, Block)
- [ ] Completed state: `TmCard` s výsledky (čas, XP, rank v leaderboardu)
- [ ] Leaderboard: List s top 10 hráči (čas, XP), `TmAvatar` + username
- [ ] **GREEN:** Testy prochází

---

## T-204: UC-016 Achievementy - Backend

### T-204.1: Domain Entities (TDD)
- [ ] **TEST:** `Achievement_Create_SetsProperties` → RED
- [ ] **TEST:** `UserAchievement_Unlock_SetsUnlockedAt` → RED
- [ ] Vytvořit `Achievement` entitu (Id, Key, Category, XPReward, IconName, RequiredValue)
- [ ] Vytvořit `UserAchievement` entitu (UserId, AchievementId, UnlockedAt, Progress)
- [ ] Vytvořit `AchievementCategory` enum (Performance, Streak, Difficulty, Special)
- [ ] EF Core konfigurace + migrace
- [ ] Seed data pro všechny achievementy (First Word, 100 Words, 1K Words, Streak 3/7/14/30/365, Path completions, atd.)
- [ ] **GREEN:** Testy prochází

### T-204.2: AchievementService (TDD)
- [ ] **TEST:** `AchievementService_CheckWordSolved_FirstWord_UnlocksAchievement` → RED
- [ ] **TEST:** `AchievementService_CheckWordSolved_100Words_UnlocksAchievement` → RED
- [ ] **TEST:** `AchievementService_CheckStreak_3Days_UnlocksAchievement` → RED
- [ ] **TEST:** `AchievementService_CheckStreak_7Days_UnlocksAchievementAnd50XP` → RED
- [ ] **TEST:** `AchievementService_CheckPathComplete_UnlocksAchievement` → RED
- [ ] **TEST:** `AchievementService_CheckPerfectBoss_UnlocksAchievement` → RED
- [ ] **TEST:** `AchievementService_GetProgress_ReturnsCorrectPercentage` → RED
- [ ] **TEST:** `AchievementService_AlreadyUnlocked_DoesNotDuplicate` → RED
- [ ] Vytvořit `IAchievementService` interface
- [ ] Vytvořit DTOs: `AchievementDto`, `UserAchievementDto`, `AchievementProgressDto`
- [ ] Implementovat `AchievementService` s event-driven checking
- [ ] Zavolat po: WordSolved, LevelCompleted, StreakUpdated, PathCompleted, BossDefeated
- [ ] **GREEN:** Všechny testy prochází

### T-204.3: Achievement Endpoints
- [ ] Vytvořit `GET /api/v1/achievements` (všechny achievementy s progress)
- [ ] Vytvořit `GET /api/v1/achievements/{id}` (detail)
- [ ] Vytvořit `GET /api/v1/users/me/achievements` (uživatelovy achievementy)

---

## T-205: UC-016 Achievementy - Frontend

### T-205.1: Achievements Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `AchievementsPage_Renders_ProgressBar` → RED
- [ ] **TEST (bUnit):** `AchievementsPage_Renders_CategoryTabs` → RED
- [ ] **TEST (bUnit):** `AchievementsPage_FiltersByCategory` → RED
- [ ] Vytvořit `Achievements.razor` (`@page "/achievements"`)
- [ ] `@inject IStringLocalizer<Achievements> L`
- [ ] Progress header: `TmProgressBar` s "12/50 unlocked" textem
- [ ] Category filter: `TmTabs` + `TmTabPanel` (All, Performance 🎯, Streak 🔥, Difficulty 🧠, Special 🏆)
- [ ] Grid karet s achievementy
- [ ] **GREEN:** Test prochází

### T-205.2: AchievementCard komponenta (Tempo.Blazor)
- [ ] **TEST (bUnit):** `AchievementCard_Unlocked_ShowsGoldBorder` → RED
- [ ] **TEST (bUnit):** `AchievementCard_InProgress_ShowsProgressBar` → RED
- [ ] **TEST (bUnit):** `AchievementCard_Locked_ShowsLockIcon` → RED
- [ ] Vytvořit `AchievementCard.razor`
- [ ] Unlocked state: `TmCard` se zlatým pozadím (gold-50), `TmBadge` (Success), `TmIcon` barevná, datum odemčení, XP reward
- [ ] In Progress state: `TmCard` bílá, `TmIcon` šedá (30% opacity), `TmProgressBar` s %, zbývající požadavek
- [ ] Locked state: `TmCard` šedá (gray-100), `TmIcon` (lock), dashed border, opacity 0.5, requirements text
- [ ] **GREEN:** Testy prochází

### T-205.3: AchievementUnlock Modal (Tempo.Blazor)
- [ ] **TEST (bUnit):** `AchievementUnlockModal_Renders_AchievementInfo` → RED
- [ ] Vytvořit `AchievementUnlockModal.razor`
- [ ] `TmModal` (Size: Medium) s backdrop blur
- [ ] Badge icon s glow efektem (CSS box-shadow animation)
- [ ] Confetti burst animace
- [ ] Achievement name, description, XP reward
- [ ] `TmButton` "Skvělé!" → zavře modal
- [ ] **GREEN:** Test prochází

---

## T-206: UC-008,009,010 Boss Levely

### T-206.1: Backend - Boss Types v GameSession (TDD)
- [ ] **TEST:** `GameSession_CreateBoss_Marathon_Sets20WordsAnd3Lives` → RED
- [ ] **TEST:** `GameSession_CreateBoss_Condition_SetsForbiddenLetterPattern` → RED
- [ ] **TEST:** `GameSession_CreateBoss_Twist_SetsRevealMechanic` → RED
- [ ] Vytvořit `BossType` enum (Marathon, Condition, Twist)
- [ ] Rozšířit `GameSession` o BossType? property
- [ ] Rozšířit `GameRound` o ForbiddenLetters, RevealedLettersCount
- [ ] EF Core migrace
- [ ] **GREEN:** Testy prochází

### T-206.2: Marathon Boss Rules (TDD)
- [ ] **TEST:** `MarathonBoss_Start_20Words3Lives` → RED
- [ ] **TEST:** `MarathonBoss_WrongAnswer_DecreasesLife_NoRegen` → RED
- [ ] **TEST:** `MarathonBoss_0Lives_GameOver` → RED
- [ ] **TEST:** `MarathonBoss_AllCorrect_PerfectBonus200XP` → RED
- [ ] **TEST:** `MarathonBoss_Completed_WithLosses_100XP` → RED
- [ ] **TEST:** `MarathonBoss_Under5Min_SpeedBonus50XP` → RED
- [ ] Vytvořit `IMarathonBossRules` interface
- [ ] Implementovat pravidla: 20 slov, 3 životy, žádná regenerace, 15s per slovo
- [ ] **GREEN:** Všechny testy prochází

### T-206.3: Condition Boss Rules - Forbidden Letter (TDD)
- [ ] **TEST:** `ConditionBoss_Start_15Words` → RED
- [ ] **TEST:** `ConditionBoss_Every3rdWord_HasForbiddenLetter` → RED
- [ ] **TEST:** `ConditionBoss_AnswerContainsForbidden_Penalty5XP` → RED
- [ ] **TEST:** `ConditionBoss_AnswerWithoutForbidden_NoPenalty` → RED
- [ ] **TEST:** `ConditionBoss_WordSelection_EnsuresAlternativeExists` → RED
- [ ] Vytvořit `IConditionBossRules` interface
- [ ] Implementovat: 15 slov, forbidden letter pattern (1-2 normal, 3 forbidden, 4-5 normal, 6 forbidden...)
- [ ] Word selection: ověřit že existuje validní odpověď bez forbidden letter
- [ ] **GREEN:** Testy prochází

### T-206.4: Twist Boss Rules - Progressive Reveal (TDD)
- [ ] **TEST:** `TwistBoss_Start_12Words_2RevealedLetters` → RED
- [ ] **TEST:** `TwistBoss_RevealLetter_Every3Seconds` → RED
- [ ] **TEST:** `TwistBoss_EarlyGuess_2Letters_10XPBonus` → RED
- [ ] **TEST:** `TwistBoss_EarlyGuess_3Letters_7XPBonus` → RED
- [ ] **TEST:** `TwistBoss_EarlyGuess_4Letters_5XPBonus` → RED
- [ ] **TEST:** `TwistBoss_EarlyGuess_5Letters_2XPBonus` → RED
- [ ] **TEST:** `TwistBoss_AllRevealed_0XPBonus` → RED
- [ ] Vytvořit `ITwistBossRules` interface
- [ ] Implementovat: 12 slov, start s 2 revealed, reveal every 3s, max 5 revealed
- [ ] Vytvořit `TwistRoundState` DTO (RevealedPositions, NextRevealAt, BonusXP)
- [ ] **GREEN:** Testy prochází

### T-206.5: Boss Level Endpoints
- [ ] Rozšířit `POST /api/v1/game/start` o BossType parametr
- [ ] Rozšířit `GameRoundResult` DTO o boss-specifická data (ForbiddenLetter, RevealedPositions, BonusXP)
- [ ] Vytvořit `GET /api/v1/game/{id}/boss-state` pro Twist reveal stav

### T-206.6: Frontend - Boss Level komponenty (Tempo.Blazor)
- [ ] **TEST (bUnit):** `MarathonBoss_Renders_ProgressBar20Words` → RED
- [ ] **TEST (bUnit):** `ConditionBoss_Renders_ForbiddenLetterWarning` → RED
- [ ] **TEST (bUnit):** `TwistBoss_Renders_RevealGrid` → RED
- [ ] Vytvořit `MarathonBoss.razor` - extends GameArena s 16/20 progress, hearts bez regen, dark red-orange header
- [ ] Vytvořit `ConditionBoss.razor` - warning box: `TmAlert Severity="Warning"` s "🚫 ZAKÁZANÉ PÍSMENO: {X}", dashed red border, pulse animation
- [ ] Vytvořit `TwistBoss.razor` - reveal grid s pozicemi (shown/hidden), reveal timer `TmProgressBar`, early guess bonus display
- [ ] Boss Result Screens:
  - Victory: `TmModal` s confetti, stats (time, accuracy, lives, combo), XP rewards, badge unlock
  - Defeat: `TmModal` s dark theme, motivační zpráva, `TmButton` "Zkusit znovu"
- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Styling dle UI-UX-007

---

## T-207: UC-017 Nastavení profilu

### T-207.1: Backend - User Settings Endpoints (TDD)
- [ ] **TEST:** `GetUserProfile_Returns200WithProfile` → RED
- [ ] **TEST:** `UpdateProfile_ValidData_Returns200` → RED
- [ ] **TEST:** `UpdateProfile_DuplicateUsername_Returns409` → RED
- [ ] **TEST:** `ChangePassword_ValidOldPassword_Returns200` → RED
- [ ] **TEST:** `ChangePassword_InvalidOldPassword_Returns400` → RED
- [ ] Vytvořit DTOs: `UserProfileDto`, `UpdateProfileRequest`, `ChangePasswordRequest`, `UserPreferencesDto`, `PrivacySettingsDto`
- [ ] Vytvořit validátory: `UpdateProfileValidator`, `ChangePasswordValidator` s lokalizací
- [ ] Vytvořit `GET /api/v1/users/me` endpoint
- [ ] Vytvořit `PUT /api/v1/users/me` endpoint
- [ ] Vytvořit `PUT /api/v1/users/me/password` endpoint
- [ ] Vytvořit `PUT /api/v1/users/me/preferences` endpoint
- [ ] Vytvořit `PUT /api/v1/users/me/privacy` endpoint
- [ ] Vytvořit `POST /api/v1/users/me/avatar` endpoint (file upload)
- [ ] **GREEN:** Testy prochází

### T-207.2: Frontend - Settings Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `SettingsPage_Renders_AllSections` → RED
- [ ] **TEST (bUnit):** `SettingsPage_UpdateUsername_CallsApi` → RED
- [ ] **TEST (bUnit):** `SettingsPage_ChangePassword_ValidatesAndSubmits` → RED
- [ ] Vytvořit `Settings.razor` (`@page "/settings"`)
- [ ] `@inject IStringLocalizer<Settings> L`
- [ ] Layout: `TmTabs` s panely pro každou sekci

- [ ] **Profil sekce** (`TmTabPanel`):
  - `TmAvatar` s upload tlačítkem (preview + crop)
  - `TmFormField` + `TmTextInput` pro Username (s debounced availability check → `TmIcon` check/x)
  - `TmFormField` + `TmTextInput` pro Email
  - `<FluentValidationValidator />`
  - `TmButton` "Uložit" (Primary)

- [ ] **Heslo sekce** (`TmTabPanel`):
  - `TmFormField` + `TmTextInput` Type="password" pro aktuální heslo
  - `TmFormField` + `TmTextInput` Type="password" pro nové heslo + `TmPasswordStrengthIndicator`
  - `TmFormField` + `TmTextInput` Type="password" pro potvrzení
  - `<FluentValidationValidator />`
  - `TmButton` "Změnit heslo" (Primary)

- [ ] **Notifikace sekce** (`TmTabPanel`):
  - `TmToggle` Push notifikace
  - `TmToggle` Email notifikace
  - `TmTimePicker` Streak reminder čas
  - `TmToggle` League updates
  - `TmToggle` Achievement notifications
  - `TmToggle` Daily challenge reminder

- [ ] **Zobrazení sekce** (`TmTabPanel`):
  - `TmRadioGroup` Theme: Light / Dark / Auto (→ `ThemeService.Toggle()`)
  - `TmSelect` Jazyk (CS, EN, DE)
  - `TmToggle` Animace on/off
  - `TmToggle` Zvuky on/off

- [ ] **Soukromí sekce** (`TmTabPanel`):
  - `TmRadioGroup` Profil visibility: Public / Friends / Private
  - `TmToggle` Leaderboard visibility
  - `TmToggle` Stats sharing

- [ ] **Danger Zone** (`TmTabPanel`):
  - `TmButton Variant="Outline"` Odhlásit se
  - `TmButton Variant="Danger"` Deaktivovat účet (s confirm `TmModal`)
  - `TmButton Variant="Danger"` Smazat účet a data (s double confirm `TmModal` + type "DELETE")

- [ ] Všechny texty z `Settings.resx`
- [ ] `ToastService.ShowSuccess()` po úspěšné změně
- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Styling dle UI-UX-010

---

## T-208: UI/UX Polishing

### T-208.1: Loading States (Tempo.Blazor)
- [ ] Přidat `TmSkeleton` (Text, Circle, Rectangle varianty) na všechny stránky při načítání
- [ ] Dashboard: 4× Skeleton rectangle pro stat cards + skeleton karty
- [ ] Leagues: Skeleton pro leaderboard řádky
- [ ] Achievements: Skeleton grid pro achievement karty
- [ ] Game: Skeleton pro scrambled word a controls
- [ ] Settings: Skeleton pro form fields

### T-208.2: Error Boundaries
- [ ] Vytvořit `ErrorBoundary.razor` komponentu s `TmAlert Severity="Error"`
- [ ] Texty z `ErrorBoundary.resx`
- [ ] Retry `TmButton`
- [ ] Zabalit každou stránku do ErrorBoundary
- [ ] Implementovat global error handler pro unhandled exceptions

### T-208.3: Toast Notifications
- [ ] Přidat `TmToastContainer` do `MainLayout.razor`
- [ ] Vytvořit `NotificationHelper` service pro standardizované toasty
- [ ] Success toasty: XP gained, level up, achievement unlocked
- [ ] Error toasty: API errors, connection lost
- [ ] Warning toasty: streak ending, lives low
- [ ] Info toasty: daily challenge available
- [ ] Auto-dismiss po 5s, max 3 viditelné

### T-208.4: Animace a přechody
- [ ] Page transitions: fade-in na route change
- [ ] Stagger animations: sekvenční load elementů na dashboard (100ms intervals)
- [ ] Count-up animace pro čísla (XP, streak, accuracy)
- [ ] Hover efekty na kartách: scale 1.02, shadow increase
- [ ] `@media (prefers-reduced-motion: reduce)` → vypnout animace

### T-208.5: Mobile Responsive
- [ ] Dashboard: 4 col → 2×2 → stack
- [ ] Paths: horizontal scroll → vertical stack
- [ ] Leaderboard: compact mode s menšími avatary
- [ ] Game: menší písmena (36px), full-width input
- [ ] Navigation: `TmSidebar` collapsible na mobilu, hamburger menu
- [ ] Modaly: full-screen na mobilu (`TmModal Size="FullScreen"` na breakpoint < 640px)

---

## Ověření dokončení fáze

- [ ] Password reset flow kompletní (request → email → reset → login)
- [ ] Ligy fungují: přiřazení, leaderboard, weekly reset, promotion/demotion
- [ ] Denní výzva: daily word, modifiers, leaderboard
- [ ] Achievementy: tracking, unlock, progress, categories
- [ ] Boss levely: Marathon (20 slov), Condition (forbidden letter), Twist (reveal)
- [ ] Settings: profil, heslo, notifikace, theme, privacy, danger zone
- [ ] Loading states se skeleton na všech stránkách
- [ ] Error boundaries s retry
- [ ] Toast notifikace fungují
- [ ] Responsive na mobilu
- [ ] Všechny texty z .resx
- [ ] `dotnet test` → všechny testy zelené
