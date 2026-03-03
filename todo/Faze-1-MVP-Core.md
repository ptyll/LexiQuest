# Fáze 1: MVP Core (Týden 1-2)

> **Cíl:** Registrace, přihlášení, herní smyčka, životy, XP/levely, cesty, streak, dashboard
> **Závislost:** Fáze 0 kompletně dokončena
> **Tempo.Blazor komponenty:** TmTextInput, TmButton, TmCard, TmStatCard, TmModal, TmProgressBar, TmAlert, TmToastContainer, TmBadge, TmAvatar, TmFormField, TmFormSection, TmSpinner, TmSkeleton, TmTooltip, TmTabs, TmIcon, FluentValidationValidator, TmPasswordStrengthIndicator

---

## ⚠️ KRITICKÁ PRAVIDLA

- **TDD:** Test FIRST → RED → GREEN → REFACTOR
- **Žádné hardcoded texty** → vše z `.resx`
- **FluentValidation** přes `FluentValidationValidator` (Tempo.Blazor.FluentValidation) na FE
- **FluentValidation** s `IStringLocalizer` na BE
- **DTOs** v `LexiQuest.Shared`
- **HTTP status kódy** místo wrapper tříd
- **Produkční kód** od prvního řádku

---

## T-100: UC-001 Registrace uživatele

### T-100.1: Backend - RegisterRequest Validator (TDD)
- [ ] **TEST:** `RegisterRequestValidator_EmptyEmail_ReturnsError` → RED
- [ ] **TEST:** `RegisterRequestValidator_InvalidEmail_ReturnsError` → RED
- [ ] **TEST:** `RegisterRequestValidator_EmptyUsername_ReturnsError` → RED
- [ ] **TEST:** `RegisterRequestValidator_UsernameTooShort_ReturnsError` (< 3 znaky) → RED
- [ ] **TEST:** `RegisterRequestValidator_UsernameTooLong_ReturnsError` (> 30 znaků) → RED
- [ ] **TEST:** `RegisterRequestValidator_UsernameInvalidChars_ReturnsError` (jen a-zA-Z0-9_) → RED
- [ ] **TEST:** `RegisterRequestValidator_PasswordTooShort_ReturnsError` (< 8 znaků) → RED
- [ ] **TEST:** `RegisterRequestValidator_PasswordMissingUppercase_ReturnsError` → RED
- [ ] **TEST:** `RegisterRequestValidator_PasswordMissingLowercase_ReturnsError` → RED
- [ ] **TEST:** `RegisterRequestValidator_PasswordMissingDigit_ReturnsError` → RED
- [ ] **TEST:** `RegisterRequestValidator_PasswordMissingSpecialChar_ReturnsError` → RED
- [ ] **TEST:** `RegisterRequestValidator_PasswordMismatch_ReturnsError` → RED
- [ ] **TEST:** `RegisterRequestValidator_TermsNotAccepted_ReturnsError` → RED
- [ ] **TEST:** `RegisterRequestValidator_ValidRequest_NoErrors` → RED
- [ ] Vytvořit `RegisterRequestValidator : AbstractValidator<RegisterRequest>` v Core/Validators/
- [ ] Použít `IStringLocalizer<ValidationMessages>` pro všechny chybové zprávy z .resx
- [ ] **GREEN:** Všechny testy prochází
- [ ] **REFACTOR:** Extrahovat společné pravidla (email, password)

### T-100.2: Backend - UserService.Register (TDD)
- [ ] **TEST:** `UserService_Register_ValidData_CreatesUser` → RED
- [ ] **TEST:** `UserService_Register_DuplicateEmail_Returns409Conflict` → RED
- [ ] **TEST:** `UserService_Register_DuplicateUsername_Returns409Conflict` → RED
- [ ] **TEST:** `UserService_Register_InitializesDefaultStats` (0 XP, Level 1, Bronze) → RED
- [ ] **TEST:** `UserService_Register_InitializesDefaultStreak` (0 days) → RED
- [ ] **TEST:** `UserService_Register_GeneratesTokens` (AccessToken + RefreshToken) → RED
- [ ] Vytvořit `IUserService` interface v Core/Interfaces/
- [ ] Implementovat `UserService` v Core/Services/
- [ ] Použít Identity `UserManager<User>` pro vytvoření uživatele
- [ ] Inicializovat UserStats, Streak, Preferences s default hodnotami
- [ ] Generovat JWT a RefreshToken přes `ITokenService`
- [ ] Uložit RefreshToken do DB přes `IUnitOfWork`
- [ ] **GREEN:** Všechny testy prochází
- [ ] **REFACTOR:** Vyčistit, přidat logging

### T-100.3: Backend - Register Endpoint (TDD)
- [ ] **TEST:** `RegisterEndpoint_ValidRequest_Returns201Created` → RED
- [ ] **TEST:** `RegisterEndpoint_InvalidRequest_Returns400WithValidationErrors` → RED
- [ ] **TEST:** `RegisterEndpoint_DuplicateEmail_Returns409Conflict` → RED
- [ ] Vytvořit `POST /api/v1/users/register` endpoint (Minimal API nebo Controller)
- [ ] Endpoint volá `IUserService.Register()`
- [ ] Vrací `AuthResponse` DTO s HTTP 201 Created
- [ ] Validace přes FluentValidation middleware (automatická)
- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Přidat endpoint dokumentaci pro Swagger

### T-100.4: Frontend - RegisterModel a Validator
- [ ] Vytvořit `RegisterModel` v Blazor/Models/ (Email, Username, Password, ConfirmPassword, AcceptTerms)
- [ ] **TEST (bUnit):** `RegisterModelValidator_EmptyEmail_ShowsError` → RED
- [ ] **TEST (bUnit):** `RegisterModelValidator_InvalidPassword_ShowsError` → RED
- [ ] **TEST (bUnit):** `RegisterModelValidator_PasswordMismatch_ShowsError` → RED
- [ ] **TEST (bUnit):** `RegisterModelValidator_ValidModel_NoErrors` → RED
- [ ] Vytvořit `RegisterModelValidator : AbstractValidator<RegisterModel>` v Blazor/Validators/
- [ ] Použít `IStringLocalizer<ValidationMessages>` pro lokalizované chybové zprávy
- [ ] **GREEN:** Všechny testy prochází

### T-100.5: Frontend - AuthService
- [ ] **TEST:** `AuthService_Register_CallsApiAndReturnsTokens` → RED
- [ ] **TEST:** `AuthService_Register_StoresTokensInLocalStorage` → RED
- [ ] Vytvořit `IAuthService` interface v Blazor/Services/
- [ ] Implementovat `AuthService` - volá `POST /api/v1/users/register`
- [ ] Implementovat `AuthStateProvider` (CustomAuthenticationStateProvider)
- [ ] Token storage v LocalStorage (AccessToken, RefreshToken)
- [ ] **GREEN:** Testy prochází

### T-100.6: Frontend - Register Page (Tempo.Blazor komponenty)
- [ ] **TEST (bUnit):** `RegisterPage_Renders_AllFormFields` → RED
- [ ] **TEST (bUnit):** `RegisterPage_InvalidForm_ShowsValidationErrors` → RED
- [ ] **TEST (bUnit):** `RegisterPage_SubmitValid_CallsAuthService` → RED
- [ ] Vytvořit `Register.razor` stránku (`@page "/register"`)
- [ ] Použít `@inject IStringLocalizer<Register> L` pro všechny texty
- [ ] Layout: Centrovaný `TmCard` (max-width 420px, Elevated variant)
- [ ] Logo LexiQuest nahoře s animací (fade+scale)
- [ ] `<EditForm Model="model" OnValidSubmit="HandleRegister">`
- [ ] `<FluentValidationValidator />` (z Tempo.Blazor.FluentValidation)
- [ ] `<TmFormField Label="@L["Input.Email.Label"]">` + `<TmTextInput @bind-Value="model.Email" Placeholder="@L["Input.Email.Placeholder"]" />`
- [ ] `<TmFormField Label="@L["Input.Username.Label"]">` + `<TmTextInput @bind-Value="model.Username" Placeholder="@L["Input.Username.Placeholder"]" />`
- [ ] `<TmFormField Label="@L["Input.Password.Label"]">` + `<TmTextInput @bind-Value="model.Password" Type="password" />` + `<TmPasswordStrengthIndicator Password="@model.Password" />`
- [ ] `<TmFormField Label="@L["Input.ConfirmPassword.Label"]">` + `<TmTextInput @bind-Value="model.ConfirmPassword" Type="password" />`
- [ ] `<TmCheckbox @bind-Value="model.AcceptTerms" Label="@L["Checkbox.Terms"]" />`
- [ ] `<TmButton Variant="ButtonVariant.Primary" ButtonType="ButtonType.Submit" IsLoading="@isLoading" Block="true">@L["Button.Submit"]</TmButton>`
- [ ] Google OAuth tlačítko: `<TmButton Variant="ButtonVariant.Outline">@L["Button.Google"]</TmButton>`
- [ ] Link na login: `@L["Login.Prompt"]` + NavigationManager link
- [ ] `<TmValidationSummary />` pro souhrn chyb
- [ ] Při úspěchu → NavigationManager.NavigateTo("/dashboard")
- [ ] Při chybě → `TmAlert` s chybovou zprávou z API
- [ ] **GREEN:** Všechny testy prochází
- [ ] **REFACTOR:** CSS styling dle UI-UX-003 (orange focus, input states)

### T-100.7: Integrační test
- [ ] **TEST:** End-to-end test: Register → API call → DB záznam → Token vrácen → Redirect
- [ ] Ověřit že validace funguje na FE i BE
- [ ] Ověřit že duplicitní email vrací 409
- [ ] Ověřit že token je validní JWT

---

## T-101: UC-002 Přihlášení uživatele

### T-101.1: Backend - LoginRequestValidator (TDD)
- [ ] **TEST:** `LoginRequestValidator_EmptyEmail_ReturnsError` → RED
- [ ] **TEST:** `LoginRequestValidator_EmptyPassword_ReturnsError` → RED
- [ ] **TEST:** `LoginRequestValidator_ValidRequest_NoErrors` → RED
- [ ] Vytvořit `LoginRequestValidator : AbstractValidator<LoginRequest>` s lokalizovanými zprávami
- [ ] **GREEN:** Testy prochází

### T-101.2: Backend - LoginService (TDD)
- [ ] **TEST:** `LoginService_ValidCredentials_ReturnsAuthResponse` → RED
- [ ] **TEST:** `LoginService_InvalidEmail_Returns401` → RED
- [ ] **TEST:** `LoginService_InvalidPassword_Returns401` → RED
- [ ] **TEST:** `LoginService_InvalidPassword_IncrementsFailedAttempts` → RED
- [ ] **TEST:** `LoginService_LockedAccount_Returns423Locked` → RED
- [ ] **TEST:** `LoginService_ValidLogin_ResetsFailedAttempts` → RED
- [ ] **TEST:** `LoginService_ValidLogin_UpdatesLastLoginAt` → RED
- [ ] **TEST:** `LoginService_ValidLogin_ChecksStreakWarning` (< 6h) → RED
- [ ] Vytvořit `ILoginService` interface
- [ ] Implementovat `LoginService` - generic error message (neodhalovat jestli email existuje)
- [ ] Implementovat lockout logiku (5 pokusů → zámek)
- [ ] Streak warning check: pokud streak končí za < 6h, přidat do response
- [ ] **GREEN:** Všechny testy prochází

### T-101.3: Backend - Login Endpoint
- [ ] **TEST:** `LoginEndpoint_ValidCredentials_Returns200WithTokens` → RED
- [ ] **TEST:** `LoginEndpoint_InvalidCredentials_Returns401` → RED
- [ ] Vytvořit `POST /api/v1/users/login` endpoint
- [ ] Vytvořit `POST /api/v1/users/refresh` endpoint pro token refresh
- [ ] Vytvořit `POST /api/v1/users/logout` endpoint (revoke refresh token)
- [ ] **GREEN:** Testy prochází

### T-101.4: Frontend - LoginModel a Validator
- [ ] **TEST:** `LoginModelValidator_EmptyEmail_ShowsError` → RED
- [ ] **TEST:** `LoginModelValidator_ValidModel_NoErrors` → RED
- [ ] Vytvořit `LoginModel` (Email, Password, RememberMe)
- [ ] Vytvořit `LoginModelValidator : AbstractValidator<LoginModel>` s lokalizací
- [ ] **GREEN:** Testy prochází

### T-101.5: Frontend - Login Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `LoginPage_Renders_EmailAndPasswordFields` → RED
- [ ] **TEST (bUnit):** `LoginPage_InvalidForm_ShowsErrors` → RED
- [ ] **TEST (bUnit):** `LoginPage_SubmitValid_CallsAuthService` → RED
- [ ] Vytvořit `Login.razor` (`@page "/login"`)
- [ ] `@inject IStringLocalizer<Login> L`
- [ ] Layout: `TmCard` (Elevated, 420px, centrovaný)
- [ ] Logo + `@L["Title"]` heading + `@L["Subtitle"]`
- [ ] `<EditForm>` + `<FluentValidationValidator />`
- [ ] `<TmFormField>` + `<TmTextInput>` pro Email
- [ ] `<TmFormField>` + `<TmTextInput Type="password">` pro Password
- [ ] `<TmCheckbox>` pro RememberMe
- [ ] `<TmButton Variant="Primary" IsLoading="@isLoading" Block="true">@L["Button.Submit"]</TmButton>`
- [ ] Link "Zapomenuté heslo" → `/password-reset`
- [ ] Google login button
- [ ] Link na registraci
- [ ] Error handling: `TmAlert Severity="Error"` při neúspěšném loginu
- [ ] Streak warning: `TmModal` pokud streak < 6h zbývá
- [ ] Po úspěchu → redirect na `/dashboard`
- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Styling dle UI-UX-003

### T-101.6: Frontend - HTTP interceptor pro JWT
- [ ] Vytvořit `AuthorizationMessageHandler : DelegatingHandler`
- [ ] Automaticky přidávat `Authorization: Bearer {token}` do requestů
- [ ] Při 401 → zkusit refresh token → při selhání → redirect na login
- [ ] Zaregistrovat handler v DI pro HttpClient

### T-101.7: Integrační test
- [ ] **TEST:** Login flow: zadání credentials → API → token → redirect na dashboard
- [ ] Ověřit lockout po 5 neúspěšných pokusech
- [ ] Ověřit refresh token flow

---

## T-102: UC-004 Základní herní smyčka

### T-102.1: Backend - WordRepository (TDD)
- [ ] **TEST:** `WordRepository_GetByDifficulty_ReturnsCorrectWords` → RED
- [ ] **TEST:** `WordRepository_GetByCategory_ReturnsCorrectWords` → RED
- [ ] **TEST:** `WordRepository_GetRandom_ReturnsRandomWord` → RED
- [ ] **TEST:** `WordRepository_GetRandomBatch_ReturnsNonRepeating` → RED
- [ ] Vytvořit `IWordRepository` interface v Core/Interfaces/
- [ ] Implementovat `WordRepository` v Infrastructure/Repositories/
- [ ] Implementovat GetByDifficulty, GetByCategory, GetRandom, GetRandomBatch
- [ ] **GREEN:** Testy prochází

### T-102.2: Backend - Scramble algoritmus (TDD)
- [ ] **TEST:** `ScrambleService_Scramble_UsesFisherYatesShuffle` → RED
- [ ] **TEST:** `ScrambleService_Scramble_NeverReturnsOriginal` → RED
- [ ] **TEST:** `ScrambleService_Scramble_ContainsSameLetters` → RED
- [ ] **TEST:** `ScrambleService_Scramble_HandlesShortWords` (2-3 písmena) → RED
- [ ] **TEST:** `ScrambleService_Scramble_HandlesDuplicateLetters` ("ANNA") → RED
- [ ] Implementovat Fisher-Yates shuffle v `Word.Scramble()` nebo `IScrambleService`
- [ ] Zajistit že výsledek ≠ originál (retry pokud ano)
- [ ] **GREEN:** Testy prochází

### T-102.3: Backend - XP Calculation Service (TDD)
- [ ] **TEST:** `XpCalculator_CorrectAnswer_Returns10BaseXP` → RED
- [ ] **TEST:** `XpCalculator_FastAnswer_Under3s_Returns5SpeedBonus` → RED
- [ ] **TEST:** `XpCalculator_FastAnswer_Under5s_Returns3SpeedBonus` → RED
- [ ] **TEST:** `XpCalculator_FastAnswer_Under10s_Returns1SpeedBonus` → RED
- [ ] **TEST:** `XpCalculator_SlowAnswer_NoSpeedBonus` → RED
- [ ] **TEST:** `XpCalculator_Combo3Plus_Returns1Point2Multiplier` → RED
- [ ] **TEST:** `XpCalculator_Combo5Plus_Returns1Point5Multiplier` → RED
- [ ] **TEST:** `XpCalculator_Combo10Plus_Returns2xMultiplier` → RED
- [ ] **TEST:** `XpCalculator_WrongAnswer_Returns0XP` → RED
- [ ] **TEST:** `XpCalculator_StreakBonus_5PlusCorrect_Returns2ExtraXP` → RED
- [ ] Vytvořit `IXpCalculator` interface
- [ ] Implementovat `XpCalculator` s formulí: `Floor((Base + SpeedBonus) * ComboMultiplier) + StreakBonus`
- [ ] **GREEN:** Všechny testy prochází
- [ ] **REFACTOR:** Extrahovat konstanty do konfigurační třídy

### T-102.4: Backend - GameSessionService (TDD)
- [ ] **TEST:** `GameSessionService_StartGame_CreatesSession` → RED
- [ ] **TEST:** `GameSessionService_StartGame_SetsCorrectMode` → RED
- [ ] **TEST:** `GameSessionService_StartGame_GeneratesFirstRound` → RED
- [ ] **TEST:** `GameSessionService_SubmitAnswer_Correct_IncreasesXP` → RED
- [ ] **TEST:** `GameSessionService_SubmitAnswer_Correct_IncreasesCombo` → RED
- [ ] **TEST:** `GameSessionService_SubmitAnswer_Wrong_ResetsCombo` → RED
- [ ] **TEST:** `GameSessionService_SubmitAnswer_Wrong_DecreasesLife` → RED
- [ ] **TEST:** `GameSessionService_SubmitAnswer_CaseInsensitive` → RED
- [ ] **TEST:** `GameSessionService_SubmitAnswer_TrimsWhitespace` → RED
- [ ] **TEST:** `GameSessionService_SubmitAnswer_DiacriticsMustMatch` → RED
- [ ] **TEST:** `GameSessionService_NextRound_GeneratesNewScrambledWord` → RED
- [ ] **TEST:** `GameSessionService_AllRoundsComplete_EndsSession` → RED
- [ ] Vytvořit `IGameSessionService` interface
- [ ] Vytvořit `StartGameRequest` DTO v Shared (Mode, PathId?, DifficultyLevel)
- [ ] Vytvořit `SubmitAnswerRequest` DTO v Shared (SessionId, Answer, TimeSpentMs)
- [ ] Vytvořit `GameRoundResult` DTO v Shared (IsCorrect, CorrectAnswer, XPEarned, SpeedBonus, ComboCount, IsLevelComplete)
- [ ] Vytvořit `ScrambledWordDto` DTO v Shared (SessionId, RoundNumber, ScrambledWord, WordLength, Difficulty, TimeLimit)
- [ ] Implementovat `GameSessionService`
- [ ] **GREEN:** Všechny testy prochází
- [ ] **REFACTOR:** Vyčistit, rozdělit odpovědnosti

### T-102.5: Backend - SubmitAnswerValidator (TDD)
- [ ] **TEST:** `SubmitAnswerValidator_EmptyAnswer_ReturnsError` → RED
- [ ] **TEST:** `SubmitAnswerValidator_AnswerTooLong_ReturnsError` (> 50 znaků) → RED
- [ ] **TEST:** `SubmitAnswerValidator_EmptySessionId_ReturnsError` → RED
- [ ] **TEST:** `SubmitAnswerValidator_NegativeTime_ReturnsError` → RED
- [ ] Vytvořit `SubmitAnswerValidator : AbstractValidator<SubmitAnswerRequest>` s lokalizací
- [ ] **GREEN:** Testy prochází

### T-102.6: Backend - Game Endpoints
- [ ] **TEST:** `StartGameEndpoint_ValidRequest_Returns201WithScrambledWord` → RED
- [ ] **TEST:** `SubmitAnswerEndpoint_CorrectAnswer_Returns200WithResult` → RED
- [ ] **TEST:** `SubmitAnswerEndpoint_Unauthorized_Returns401` → RED
- [ ] Vytvořit `POST /api/v1/game/start` endpoint (vrací ScrambledWordDto)
- [ ] Vytvořit `POST /api/v1/game/{id}/answer` endpoint (vrací GameRoundResult)
- [ ] Vytvořit `GET /api/v1/game/{id}/hint` endpoint (vrací HintDto, stojí XP)
- [ ] Vytvořit `POST /api/v1/game/{id}/forfeit` endpoint
- [ ] Všechny endpointy vyžadují `[Authorize]`
- [ ] **GREEN:** Testy prochází

### T-102.7: Frontend - GameService
- [ ] **TEST:** `GameService_StartGame_ReturnsScrambledWord` → RED
- [ ] **TEST:** `GameService_SubmitAnswer_ReturnsResult` → RED
- [ ] Vytvořit `IGameService` interface v Blazor/Services/
- [ ] Implementovat `GameService` - HTTP volání na game endpointy
- [ ] **GREEN:** Testy prochází

### T-102.8: Frontend - GameArena komponenta (Tempo.Blazor)
- [ ] **TEST (bUnit):** `GameArena_Renders_ScrambledWordLetters` → RED
- [ ] **TEST (bUnit):** `GameArena_SubmitCorrectAnswer_ShowsSuccessFeedback` → RED
- [ ] **TEST (bUnit):** `GameArena_SubmitWrongAnswer_ShowsErrorFeedback` → RED
- [ ] **TEST (bUnit):** `GameArena_TimerExpires_SubmitsEmptyAnswer` → RED
- [ ] Vytvořit `GameArena.razor` komponentu
- [ ] `@inject IStringLocalizer<GameArena> L`
- [ ] Header: `TmButton` (zpět), název levelu, `TmBadge` streak, XP counter
- [ ] Scrambled Word Display: Velká písmena v `TmCard` boxech (48px font, animace deal-in)
- [ ] Answer Input: `<TmTextInput @bind-Value="answer" Placeholder="@L["Answer.Placeholder"]" />` (72px výška, uppercase, centrované)
- [ ] Submit: `<TmButton Variant="Primary" OnClick="SubmitAnswer" IsLoading="@isSubmitting">@L["Answer.Submit"]</TmButton>`
- [ ] Skip: `<TmButton Variant="Ghost" OnClick="SkipRound">@L["Answer.Skip"]</TmButton>`
- [ ] Hint: `<TmButton Variant="Outline" OnClick="RequestHint" Disabled="@(!hintAvailable)">@L["Hint.Button"] @string.Format(L["Hint.Cost"], hintXpCost)</TmButton>`
- [ ] Feedback - Correct: zelený `TmAlert` s animací (bounce, +XP float up)
- [ ] Feedback - Wrong: červený `TmAlert` se shake animací, zobrazit správnou odpověď
- [ ] Combo display: `TmBadge` s `@string.Format(L["Combo.Multiplier"], comboCount)`
- [ ] Level progress: `@string.Format(L["Level.Progress"], currentRound, totalRounds)`
- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Animace, CSS transitions

### T-102.9: Frontend - Timer komponenta
- [ ] **TEST (bUnit):** `TimerComponent_StartsCountdown_FromGivenSeconds` → RED
- [ ] **TEST (bUnit):** `TimerComponent_ReachesZero_FiresTimeUpEvent` → RED
- [ ] **TEST (bUnit):** `TimerComponent_ColorChanges_AtThresholds` → RED
- [ ] Vytvořit `GameTimer.razor` komponentu
- [ ] `TmProgressBar` s vizuálním odpočtem (Mode: Determinate)
- [ ] Barvy: zelená (>10s), oranžová (5-10s), červená (<5s) s pulsem
- [ ] Čas zobrazení: `@L["Timer.Format"]` formát MM:SS
- [ ] Pauza když okno ztratí focus (Page Visibility API)
- [ ] EventCallback `OnTimeUp`
- [ ] **GREEN:** Testy prochází

### T-102.10: Frontend - Game Page
- [ ] Vytvořit `Game.razor` stránku (`@page "/game"`, `@page "/game/{SessionId}"`)
- [ ] `@inject IStringLocalizer<Game> L`
- [ ] Orchestruje GameArena, Timer, LivesIndicator komponenty
- [ ] Zobrazí loading state (`TmSkeleton`) při načítání
- [ ] Error handling s `TmAlert`
- [ ] Game Over modal: `TmModal` s výsledky (Size: Medium)
- [ ] Level Complete modal: `TmModal` s confetti, XP rewards, achievements
- [ ] **REFACTOR:** Responsive layout dle UI-UX-005

### T-102.11: Integrační test
- [ ] **TEST:** Kompletní herní cyklus: Start → Scramble → Answer → XP → Next Round → Complete
- [ ] Ověřit XP calculation end-to-end
- [ ] Ověřit combo mechaniku
- [ ] Ověřit timer mechaniku

---

## T-103: UC-005 Životy systém

### T-103.1: Backend - LivesService (TDD)
- [ ] **TEST:** `LivesService_GetLives_TrainingMode_ReturnsInfinite` → RED
- [ ] **TEST:** `LivesService_GetLives_BeginnerPath_Returns5` → RED
- [ ] **TEST:** `LivesService_GetLives_IntermediatePath_Returns4` → RED
- [ ] **TEST:** `LivesService_GetLives_AdvancedPath_Returns3` → RED
- [ ] **TEST:** `LivesService_GetLives_ExpertPath_Returns3` → RED
- [ ] **TEST:** `LivesService_LoseLife_DecrementsCount` → RED
- [ ] **TEST:** `LivesService_LoseLife_AtZero_ReturnsGameOver` → RED
- [ ] **TEST:** `LivesService_RegenerateLife_AfterInterval_IncrementsCount` → RED
- [ ] **TEST:** `LivesService_RegenerateLife_AtMax_DoesNotExceed` → RED
- [ ] Vytvořit `ILivesService` interface
- [ ] Vytvořit `LivesStatus` DTO v Shared (Current, Max, NextRegenAt, IsInfinite)
- [ ] Implementovat `LivesService` s regeneration logic per difficulty
- [ ] **GREEN:** Všechny testy prochází

### T-103.2: Backend - LiveRegenerationService (Background)
- [ ] **TEST:** `LiveRegenerationService_ProcessesRegeneration_AtCorrectIntervals` → RED
- [ ] Vytvořit `LiveRegenerationService : BackgroundService` (nebo Hangfire RecurringJob)
- [ ] Advanced: regen každých 30 min, Expert: každých 60 min
- [ ] **GREEN:** Test prochází

### T-103.3: Backend - Integrace do GameSession
- [ ] **TEST:** `GameSession_WrongAnswer_DecreasesLife` → RED
- [ ] **TEST:** `GameSession_ZeroLives_StatusChangesToFailed` → RED
- [ ] **TEST:** `GameSession_CorrectAnswer_DoesNotAffectLives` → RED
- [ ] Upravit `GameSessionService.SubmitAnswer` - dekrementovat životy při chybné odpovědi
- [ ] Přidat `LivesRemaining` do `GameRoundResult` DTO
- [ ] **GREEN:** Testy prochází

### T-103.4: Frontend - LivesIndicator komponenta (Tempo.Blazor)
- [ ] **TEST (bUnit):** `LivesIndicator_Renders_CorrectNumberOfHearts` → RED
- [ ] **TEST (bUnit):** `LivesIndicator_ZeroLives_AllHeartsEmpty` → RED
- [ ] **TEST (bUnit):** `LivesIndicator_ShowsRegenTimer_WhenNotFull` → RED
- [ ] Vytvořit `LivesIndicator.razor` komponentu
- [ ] `@inject IStringLocalizer<LivesIndicator> L`
- [ ] Zobrazit srdíčka: `TmIcon` (heart) - plné červené / prázdné šedé
- [ ] Regen timer: `TmTooltip` s časem do dalšího života
- [ ] Label: `@L["Lives.Label"]`
- [ ] Animace: srdce se zmenší při ztrátě (scale down + fade)
- [ ] **GREEN:** Testy prochází

---

## T-104: UC-006 XP a Level systém

### T-104.1: Backend - LevelCalculator (TDD)
- [ ] **TEST:** `LevelCalculator_Level1_Requires100XP` → RED
- [ ] **TEST:** `LevelCalculator_Level2_Requires150XP` → RED
- [ ] **TEST:** `LevelCalculator_Level5_Requires506XP` → RED
- [ ] **TEST:** `LevelCalculator_Level10_Requires3844XP` → RED
- [ ] **TEST:** `LevelCalculator_GetLevel_FromTotalXP_ReturnsCorrectLevel` → RED
- [ ] **TEST:** `LevelCalculator_GetProgress_ReturnsPercentageInCurrentLevel` → RED
- [ ] **TEST:** `LevelCalculator_DetectLevelUp_WhenXPCrossesThreshold_ReturnsTrue` → RED
- [ ] **TEST:** `LevelCalculator_DetectLevelUp_WhenXPBelowThreshold_ReturnsFalse` → RED
- [ ] Vytvořit `ILevelCalculator` interface
- [ ] Implementovat `LevelCalculator` s exponenciální křivkou: `XP_needed = 100 * 1.5^(level-1)`
- [ ] Vytvořit `XPProgress` DTO v Shared (TotalXP, CurrentLevel, XPInCurrentLevel, XPRequiredForNextLevel, ProgressPercentage)
- [ ] **GREEN:** Všechny testy prochází

### T-104.2: Backend - XP Gain Processing (TDD)
- [ ] **TEST:** `XpService_AddXP_UpdatesUserStats` → RED
- [ ] **TEST:** `XpService_AddXP_DetectsLevelUp_ReturnsUnlocks` → RED
- [ ] **TEST:** `XpService_AddXP_MultipleLevelUps_HandlesCorrectly` → RED
- [ ] Vytvořit `IXpService` interface
- [ ] Implementovat `XpService` - aktualizuje UserStats, detekuje level-up
- [ ] Vytvořit `XPGainedEvent` DTO (Amount, Source, Breakdown, LeveledUp, NewLevel, Unlocks)
- [ ] Vytvořit `UnlockableReward` DTO (Type, Name, Description)
- [ ] Definovat unlocks per level (Level 3: Path 2, Level 5: Leagues, Level 10: Path 3, atd.)
- [ ] **GREEN:** Testy prochází

### T-104.3: Frontend - XpBar komponenta (Tempo.Blazor)
- [ ] **TEST (bUnit):** `XpBar_Renders_CurrentXPAndLevel` → RED
- [ ] **TEST (bUnit):** `XpBar_ShowsCorrectProgress_Percentage` → RED
- [ ] **TEST (bUnit):** `XpBar_AnimatesOnXPGain` → RED
- [ ] Vytvořit `XpBar.razor` komponentu
- [ ] `@inject IStringLocalizer<XpBar> L`
- [ ] `TmProgressBar` s gradientem (oranžová → žlutá), Mode: Determinate
- [ ] Level číslo vlevo: `TmBadge` s aktuálním levelem
- [ ] XP text: `@L["XP.Current"]` format "{current}/{required}"
- [ ] Animace: smooth fill při získání XP (CSS transition)
- [ ] +XP popup: číslo "float up" animace při získání bodů
- [ ] **GREEN:** Testy prochází

### T-104.4: Frontend - LevelUp Modal (Tempo.Blazor)
- [ ] **TEST (bUnit):** `LevelUpModal_Renders_NewLevel` → RED
- [ ] **TEST (bUnit):** `LevelUpModal_ShowsUnlocks_WhenAvailable` → RED
- [ ] Vytvořit `LevelUpModal.razor` komponentu
- [ ] `TmModal` (Size: Medium) s confetti animací na pozadí
- [ ] Velké číslo nového levelu s pulsem
- [ ] Odměny: `TmCard` karty s unlocked features
- [ ] `TmButton` "Pokračovat" → zavře modal
- [ ] **GREEN:** Testy prochází

---

## T-105: UC-007 Cesty - Backend

### T-105.1: Domain Entities - Path, Level
- [ ] **TEST:** `LearningPath_Create_SetsDefaultValues` → RED
- [ ] **TEST:** `PathLevel_Create_InitializesAsLocked` → RED
- [ ] **TEST:** `PathLevel_Unlock_ChangesStatusToAvailable` → RED
- [ ] **TEST:** `PathLevel_Complete_ChangesStatusToCompleted` → RED
- [ ] **TEST:** `PathLevel_CompletePerfect_SetsIsPerfect` → RED
- [ ] Vytvořit `LearningPath` entitu (Id, Name, Description, Difficulty, TotalLevels, WordLengthMin, WordLengthMax, TimePerWord, HintPolicy)
- [ ] Vytvořit `PathLevel` entitu (Id, PathId, LevelNumber, Status, WordCount, IsBoss, CompletedAt, IsPerfect)
- [ ] Vytvořit `LevelStatus` enum (Locked, Available, Current, Completed, Perfect, Boss)
- [ ] Vytvořit `CompletedLevel` entitu (UserId, LevelId, CompletedAt, XPEarned, IsPerfect)
- [ ] EF Core konfigurace + migraci
- [ ] **GREEN:** Testy prochází

### T-105.2: PathService (TDD)
- [ ] **TEST:** `PathService_GetPaths_Returns4Paths` → RED
- [ ] **TEST:** `PathService_GetPathProgress_ReturnsCompletedLevels` → RED
- [ ] **TEST:** `PathService_IsPathUnlocked_BeginnerAlwaysTrue` → RED
- [ ] **TEST:** `PathService_IsPathUnlocked_Intermediate_RequiresPath1OrLevel5` → RED
- [ ] **TEST:** `PathService_IsPathUnlocked_Advanced_RequiresPath2Complete` → RED
- [ ] **TEST:** `PathService_IsPathUnlocked_Expert_RequiresPath3Complete` → RED
- [ ] **TEST:** `PathService_GetNextLevel_ReturnsFirstUncompletedLevel` → RED
- [ ] **TEST:** `PathService_CompleteLevel_UnlocksNextLevel` → RED
- [ ] **TEST:** `PathService_CompleteLevel_LastLevel_CompletesPath` → RED
- [ ] Vytvořit `IPathService` interface
- [ ] Vytvořit DTOs v Shared: `LearningPathDto`, `PathLevelDto`, `PathProgressDto`
- [ ] Implementovat `PathService`
- [ ] **GREEN:** Všechny testy prochází

### T-105.3: Path Endpoints
- [ ] **TEST:** `GetPathsEndpoint_Returns200WithAllPaths` → RED
- [ ] **TEST:** `GetPathLevelsEndpoint_Returns200WithLevels` → RED
- [ ] Vytvořit `GET /api/v1/paths` endpoint (vrací seznam cest s progress)
- [ ] Vytvořit `GET /api/v1/paths/{pathId}/levels` endpoint (vrací levely v cestě)
- [ ] Vytvořit `GET /api/v1/paths/{pathId}/levels/{levelId}` endpoint (detail levelu)
- [ ] **GREEN:** Testy prochází

### T-105.4: Seed data pro cesty
- [ ] Vytvořit seed data: Path 1 (Beginner 🌱, 20 levelů, 3-5 písmen)
- [ ] Vytvořit seed data: Path 2 (Intermediate 🌿, 25 levelů, 5-7 písmen)
- [ ] Vytvořit seed data: Path 3 (Advanced 🌳, 30 levelů, 7-10 písmen)
- [ ] Vytvořit seed data: Path 4 (Expert 🔥, 40 levelů, 10+ písmen)
- [ ] Přidat boss levely na správné pozice (po X levelech)
- [ ] Migrace a seed

---

## T-106: UC-007 Cesty - Frontend

### T-106.1: PathService Frontend
- [ ] **TEST:** `PathService_GetPaths_ReturnsAllPaths` → RED
- [ ] Vytvořit `IPathService` interface v Blazor/Services/
- [ ] Implementovat `PathService` - HTTP volání na path endpointy
- [ ] **GREEN:** Test prochází

### T-106.2: PathSelector Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `PathSelector_Renders_4PathCards` → RED
- [ ] **TEST (bUnit):** `PathSelector_LockedPath_ShowsLockIcon` → RED
- [ ] **TEST (bUnit):** `PathSelector_UnlockedPath_ShowsProgress` → RED
- [ ] Vytvořit `Paths.razor` stránku (`@page "/paths"`)
- [ ] `@inject IStringLocalizer<Paths> L`
- [ ] 4× `TmCard` (Elevated) pro každou cestu
- [ ] Cestu 1 (🌱): zelený gradient, vždy odemčená
- [ ] Cestu 2 (🌿): modro-zelený gradient, podmíněně odemčená
- [ ] Cestu 3 (🌳): hnědý gradient, podmíněně odemčená
- [ ] Cestu 4 (🔥): červeno-oranžový gradient, podmíněně odemčená
- [ ] Zamčené cesty: `TmIcon` (lock), opacity 0.6, `TmTooltip` s požadavky
- [ ] Progress bar: `TmProgressBar` s completion %
- [ ] `TmBadge` s difficulty level
- [ ] Kliknutí → navigace na `/paths/{pathId}`
- [ ] **GREEN:** Testy prochází

### T-106.3: PathMap komponenta (Level nodes)
- [ ] **TEST (bUnit):** `PathMap_Renders_AllLevelNodes` → RED
- [ ] **TEST (bUnit):** `PathMap_CurrentLevel_HasPulsingEffect` → RED
- [ ] **TEST (bUnit):** `PathMap_BossLevel_ShowsCrownIcon` → RED
- [ ] Vytvořit `PathMap.razor` komponentu
- [ ] Tree/network vizualizace s propojenými uzly (SVG paths)
- [ ] Level nodes s barevnými stavy:
  - Completed: zelený `TmIcon` (check), bright green bg
  - Current: oranžový s pulsem (CSS animation)
  - Locked: šedý `TmIcon` (lock), opacity 0.6
  - Boss: gradient red-orange, `TmIcon` (crown), gold border
- [ ] Čísla levelů v uzlech
- [ ] SVG connecting lines s animací (stroke-dasharray)
- [ ] `TmProgressBar` celkový progress cesty nahoře

### T-106.4: LevelDetail Modal (Tempo.Blazor)
- [ ] **TEST (bUnit):** `LevelDetailModal_Renders_LevelInfo` → RED
- [ ] **TEST (bUnit):** `LevelDetailModal_StartButton_NavigatesToGame` → RED
- [ ] Vytvořit `LevelDetailModal.razor`
- [ ] `TmModal` (Size: Medium) s level info
- [ ] Info sekce: počet slov, čas na slovo, hints, životy, odměny (XP)
- [ ] `TmButton` "Začít level" → navigace na `/game/{sessionId}`
- [ ] Zobrazí preview scrambled word
- [ ] **GREEN:** Testy prochází

---

## T-107: UC-011 Streak systém

### T-107.1: Backend - StreakService (TDD)
- [ ] **TEST:** `StreakService_CheckStreak_FirstDay_Returns1` → RED
- [ ] **TEST:** `StreakService_CheckStreak_ConsecutiveDay_Increments` → RED
- [ ] **TEST:** `StreakService_CheckStreak_SameDay_NoChange` → RED
- [ ] **TEST:** `StreakService_CheckStreak_MissedDay_ResetsTo0` → RED
- [ ] **TEST:** `StreakService_CheckStreak_GracePeriod48h_DoesNotReset` → RED
- [ ] **TEST:** `StreakService_UpdateLongest_WhenCurrentExceedsLongest` → RED
- [ ] **TEST:** `StreakService_GetFireLevel_0Days_ReturnsCold` → RED
- [ ] **TEST:** `StreakService_GetFireLevel_1to3Days_ReturnsSmall` → RED
- [ ] **TEST:** `StreakService_GetFireLevel_4to7Days_ReturnsMedium` → RED
- [ ] **TEST:** `StreakService_GetFireLevel_8to30Days_ReturnsLarge` → RED
- [ ] **TEST:** `StreakService_GetFireLevel_30PlusDays_ReturnsLegendary` → RED
- [ ] **TEST:** `StreakService_CheckMilestone_3Days_ReturnsBadge` → RED
- [ ] **TEST:** `StreakService_CheckMilestone_7Days_ReturnsBadgeAnd50XP` → RED
- [ ] **TEST:** `StreakService_TimeToReset_ReturnsCorrectDuration` → RED
- [ ] Vytvořit `IStreakService` interface
- [ ] Vytvořit `StreakStatus` DTO v Shared (CurrentDays, LongestDays, FireLevel, NextResetAt, TimeRemaining, IsAtRisk, Milestones)
- [ ] Vytvořit `StreakMilestone` DTO (Days, Badge, XPReward)
- [ ] Vytvořit `FireLevel` enum (Cold, Small, Medium, Large, Legendary)
- [ ] Implementovat `StreakService`
- [ ] Volat po každém dokončeném levelu
- [ ] **GREEN:** Všechny testy prochází

### T-107.2: Backend - Streak Endpoint
- [ ] **TEST:** `GetStreakEndpoint_Returns200WithStreakStatus` → RED
- [ ] Vytvořit `GET /api/v1/users/me/streak` endpoint
- [ ] **GREEN:** Test prochází

### T-107.3: Frontend - StreakIndicator komponenta (Tempo.Blazor)
- [ ] **TEST (bUnit):** `StreakIndicator_Renders_CurrentStreakDays` → RED
- [ ] **TEST (bUnit):** `StreakIndicator_ShowsCorrectFireEmoji_ByLevel` → RED
- [ ] **TEST (bUnit):** `StreakIndicator_AtRisk_ShowsWarningState` → RED
- [ ] **TEST (bUnit):** `StreakIndicator_ShowsMilestoneProgress` → RED
- [ ] Vytvořit `StreakIndicator.razor` komponentu
- [ ] `@inject IStringLocalizer<StreakIndicator> L`
- [ ] Fire emoji dle úrovně: 🔥 (small), 🔥🔥 (medium), 🔥🔥🔥 (large), 🔥🔥🔥🔥 (legendary)
- [ ] Dny s pluralizací: `@GetStreakText(days)` (1 den, 2-4 dny, 5+ dní)
- [ ] At-risk stav: `TmBadge Variant="Warning"` s pulsující animací, countdown timer
- [ ] `TmTooltip` s popisem: `@L["Tooltip.Description"]`
- [ ] Fire animace: CSS keyframes (pulse, glow efekt)
- [ ] **GREEN:** Testy prochází

---

## T-108: UC-015 Dashboard a Statistiky

### T-108.1: Backend - Dashboard Endpoint (TDD)
- [ ] **TEST:** `DashboardEndpoint_Returns200WithUserDashboard` → RED
- [ ] **TEST:** `DashboardEndpoint_ContainsStatsSummary` → RED
- [ ] **TEST:** `DashboardEndpoint_ContainsActivityHeatmap` → RED
- [ ] **TEST:** `DashboardEndpoint_ContainsPathProgress` → RED
- [ ] **TEST:** `DashboardEndpoint_ContainsRecentAchievements` → RED
- [ ] Vytvořit `UserDashboard` DTO v Shared (Stats, Heatmap, PathProgress, RecentAchievements, DailyChallenge, LeagueInfo)
- [ ] Vytvořit `UserStatsSummary` DTO (TotalXP, CurrentLevel, CurrentStreak, LongestStreak, Accuracy, AverageTime, TotalWordsSolved)
- [ ] Vytvořit `ActivityHeatmap` DTO (Days: List<HeatmapDay>)
- [ ] Vytvořit `HeatmapDay` DTO (Date, LevelsCompleted, XPGained, IntensityLevel 0-4)
- [ ] Vytvořit `GET /api/v1/stats/dashboard` endpoint
- [ ] Implementovat dashboard aggregation service
- [ ] **GREEN:** Testy prochází

### T-108.2: Frontend - Dashboard Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `DashboardPage_Renders_StatCards` → RED
- [ ] **TEST (bUnit):** `DashboardPage_Renders_DailyChallengeCard` → RED
- [ ] **TEST (bUnit):** `DashboardPage_Renders_LeagueCard` → RED
- [ ] Vytvořit `Dashboard.razor` stránku (`@page "/dashboard"`)
- [ ] `@inject IStringLocalizer<Dashboard> L`
- [ ] Loading state: 4× `TmSkeleton` (Rectangle) + `TmSkeleton` karty
- [ ] **Stat Cards** (4 sloupce): 4× `TmStatCard`
  - XP: `TmStatCard` s `TmIcon` (star), hodnota s count-up animací
  - Streak: `TmStatCard` s StreakIndicator komponentou
  - Accuracy: `TmStatCard` s `TmIcon` (target), procenta
  - Avg Time: `TmStatCard` s `TmIcon` (clock), sekundy
- [ ] **Daily Challenge**: `TmCard` s `TmBadge` (modifier tag), scrambled word preview, `TmButton` "Hrát"
- [ ] **League Card**: `TmCard` s `TmIcon` (trophy), rank pozice, `TmProgressBar` k promo/demo
- [ ] **Paths Progress**: 4× `TmProgressBar` s cestami (barvy dle difficulty)
- [ ] **Quick Actions**: 4× `TmButton` karty (Training, Time Attack, 1v1, Shop) v `TmCard`
- [ ] Stagger animace na load (100ms intervals)
- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Responsive layout (4 col → 2×2 → stack)

### T-108.3: Frontend - ActivityHeatmap komponenta
- [ ] **TEST (bUnit):** `ActivityHeatmap_Renders_30DayGrid` → RED
- [ ] **TEST (bUnit):** `ActivityHeatmap_CorrectColors_ByIntensity` → RED
- [ ] **TEST (bUnit):** `ActivityHeatmap_Hover_ShowsTooltip` → RED
- [ ] Vytvořit `ActivityHeatmap.razor` komponentu
- [ ] GitHub-style grid: 7 řádků × ~5 sloupců (30 dní)
- [ ] Barvy dle intenzity: 0=#ebedf0, 1=#9be9a8, 2=#40c463, 3=#30a14e, 4=#216e39
- [ ] `TmTooltip` na hover: datum, počet levelů, XP
- [ ] Hover efekt: scale 1.3x
- [ ] **GREEN:** Testy prochází

---

## Tempo.Blazor komponenty použité v této fázi

| Komponenta | Použití |
|------------|---------|
| `TmTextInput` | Email, Username, Password, Answer input |
| `TmPasswordStrengthIndicator` | Registrace - síla hesla |
| `TmButton` | Všechna tlačítka (Primary, Outline, Ghost, Danger) |
| `TmCard` | Registrace/Login form, Dashboard karty, Level detail |
| `TmStatCard` | Dashboard KPI (XP, Streak, Accuracy, Time) |
| `TmFormField` | Všechna form pole s labelem a chybou |
| `TmFormSection` | Skupiny formulářových polí |
| `TmCheckbox` | Terms acceptance, Remember me |
| `TmModal` | LevelUp, GameOver, LevelDetail |
| `TmProgressBar` | Timer, XP bar, Path progress, Heatmap |
| `TmAlert` | Error/success feedback |
| `TmBadge` | Streak, Combo, Difficulty, League tier |
| `TmAvatar` | Uživatelský profil |
| `TmSpinner` | Loading stavy |
| `TmSkeleton` | Loading placeholdery |
| `TmTooltip` | Hints, streak info, heatmap hover |
| `TmIcon` | Heart, Star, Lock, Check, Crown, Clock, Target |
| `TmValidationSummary` | Souhrn validačních chyb |
| `FluentValidationValidator` | Všechny formuláře |
| `ToastService` | Success/error notifikace |
| `TmToastContainer` | Container pro toasty |

---

## Ověření dokončení fáze

- [ ] Registrace funguje end-to-end (FE → API → DB → Token)
- [ ] Login funguje end-to-end včetně lockout
- [ ] Herní smyčka kompletní: start → scramble → answer → XP → next → complete
- [ ] Životy fungují správně s regenerací
- [ ] XP systém s level-up detekcí a unlocks
- [ ] 4 cesty s level nodes a vizualizací
- [ ] Streak systém s fire indikátorem a milestones
- [ ] Dashboard s KPI kartami, heatmapou, progress bary
- [ ] Všechny texty z .resx souborů
- [ ] FluentValidation na FE (Tempo.Blazor.FluentValidation) i BE
- [ ] `dotnet test` → všechny testy zelené
- [ ] Responsive design na mobilu i desktopu
