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

## T-100: UC-001 Registrace uživatele ✅

### T-100.1: Backend - RegisterRequest Validator (TDD) ✅
- [x] **TEST:** `RegisterRequestValidator_EmptyEmail_ReturnsError` → GREEN
- [x] **TEST:** `RegisterRequestValidator_InvalidEmail_ReturnsError` → GREEN
- [x] **TEST:** `RegisterRequestValidator_EmptyUsername_ReturnsError` → GREEN
- [x] **TEST:** `RegisterRequestValidator_UsernameTooShort_ReturnsError` (< 3 znaky) → GREEN
- [x] **TEST:** `RegisterRequestValidator_UsernameTooLong_ReturnsError` (> 30 znaků) → GREEN
- [x] **TEST:** `RegisterRequestValidator_UsernameInvalidChars_ReturnsError` (jen a-zA-Z0-9_) → GREEN
- [x] **TEST:** `RegisterRequestValidator_PasswordTooShort_ReturnsError` (< 8 znaků) → GREEN
- [x] **TEST:** `RegisterRequestValidator_PasswordMissingUppercase_ReturnsError` → GREEN
- [x] **TEST:** `RegisterRequestValidator_PasswordMissingLowercase_ReturnsError` → GREEN
- [x] **TEST:** `RegisterRequestValidator_PasswordMissingDigit_ReturnsError` → GREEN
- [x] **TEST:** `RegisterRequestValidator_PasswordMissingSpecialChar_ReturnsError` → GREEN
- [x] **TEST:** `RegisterRequestValidator_PasswordMismatch_ReturnsError` → GREEN
- [x] **TEST:** `RegisterRequestValidator_TermsNotAccepted_ReturnsError` → GREEN
- [x] **TEST:** `RegisterRequestValidator_ValidRequest_NoErrors` → GREEN
- [x] Vytvořit `RegisterRequestValidator : AbstractValidator<RegisterRequest>` v Core/Validators/
- [x] Použít `IStringLocalizer<RegisterRequestValidator>` pro všechny chybové zprávy z .resx
- [x] **GREEN:** Všechny testy prochází (14/14)
- [x] **REFACTOR:** Extrahovat společné pravidla (email, password)

### T-100.2: Backend - UserService.Register (TDD) ✅
- [x] **TEST:** `UserService_Register_ValidData_CreatesUser` → GREEN
- [x] **TEST:** `UserService_Register_DuplicateEmail_Returns409Conflict` → GREEN
- [x] **TEST:** `UserService_Register_DuplicateUsername_Returns409Conflict` → GREEN
- [x] **TEST:** `UserService_Register_InitializesDefaultStats` (0 XP, Level 1, Bronze) → GREEN
- [x] **TEST:** `UserService_Register_InitializesDefaultStreak` (0 days) → GREEN
- [x] **TEST:** `UserService_Register_GeneratesTokens` (AccessToken + RefreshToken) → GREEN
- [x] Vytvořit `IUserService` interface v Core/Interfaces/
- [x] Implementovat `UserService` v Core/Services/
- [x] Použít `IPasswordHasher<User>` pro hashování hesla
- [x] Inicializovat UserStats, Streak, Preferences s default hodnotami
- [x] Generovat JWT a RefreshToken přes `ITokenService`
- [x] Uložit změny přes `IUnitOfWork`
- [x] **GREEN:** Všechny testy prochází (7/7)
- [x] **REFACTOR:** Vyčistit, přidat logging

### T-100.3: Backend - Register Endpoint (TDD) ✅
- [x] **TEST:** `RegisterEndpoint_ValidRequest_Returns201Created` → GREEN
- [x] **TEST:** `RegisterEndpoint_InvalidRequest_Returns400WithValidationErrors` → GREEN
- [x] **TEST:** `RegisterEndpoint_DuplicateEmail_Returns409Conflict` → GREEN
- [x] Vytvořit `POST /api/v1/users/register` endpoint (Minimal API)
- [x] Endpoint volá `IUserService.Register()`
- [x] Vrací `AuthResponse` DTO s HTTP 201 Created
- [x] Validace přes FluentValidation middleware (automatická)
- [x] **GREEN:** Testy prochází
- [x] **REFACTOR:** Přidat endpoint dokumentaci pro Swagger

### T-100.4: Frontend - RegisterModel a Validator ✅
- [x] Vytvořit `RegisterModel` v Blazor/Models/ (Email, Username, Password, ConfirmPassword, AcceptTerms)
- [x] **TEST (bUnit):** `RegisterModelValidator_EmptyEmail_ShowsError` → GREEN
- [x] **TEST (bUnit):** `RegisterModelValidator_InvalidPassword_ShowsError` → GREEN
- [x] **TEST (bUnit):** `RegisterModelValidator_PasswordMismatch_ShowsError` → GREEN
- [x] **TEST (bUnit):** `RegisterModelValidator_ValidModel_NoErrors` → GREEN
- [x] Vytvořit `RegisterModelValidator : AbstractValidator<RegisterModel>` v Blazor/Validators/
- [x] Použít `IStringLocalizer<ValidationMessages>` pro lokalizované chybové zprávy
- [x] **GREEN:** Všechny testy prochází

### T-100.5: Frontend - AuthService ✅
- [x] **TEST:** `AuthService_Register_CallsApiAndReturnsTokens` → GREEN
- [x] **TEST:** `AuthService_Register_StoresTokensInLocalStorage` → GREEN
- [x] Vytvořit `IAuthService` interface v Blazor/Services/
- [x] Implementovat `AuthService` - volá `POST /api/v1/users/register`
- [x] Implementovat `AuthStateProvider` (CustomAuthenticationStateProvider)
- [x] Token storage v LocalStorage (AccessToken, RefreshToken)
- [x] **GREEN:** Testy prochází

### T-100.6: Frontend - Register Page (Tempo.Blazor komponenty) ✅
- [x] **TEST (bUnit):** `RegisterPage_Renders_AllFormFields` → GREEN
- [x] **TEST (bUnit):** `RegisterPage_InvalidForm_ShowsValidationErrors` → GREEN
- [x] **TEST (bUnit):** `RegisterPage_SubmitValid_CallsAuthService` → GREEN
- [x] Vytvořit `Register.razor` stránku (`@page "/register"`)
- [x] `@inject IStringLocalizer<Register> L`
- [x] Layout: Centrovaný `TmCard` (max-width 420px, Elevated variant)
- [x] Logo LexiQuest nahoře s animací (fade+scale)
- [x] `<EditForm Model="model" OnValidSubmit="HandleRegister">`
- [x] `<FluentValidationValidator />` (z Tempo.Blazor.FluentValidation)
- [x] Form fields s TmTextInput
- [x] `<TmPasswordStrengthIndicator Password="@model.Password" />`
- [x] `<TmCheckbox @bind-Value="model.AcceptTerms" Label="@L["Checkbox.Terms"]" />`
- [x] Submit button s loading stavem
- [x] Link na login
- [x] `<TmValidationSummary />` pro souhrn chyb
- [x] Při úspěchu → NavigationManager.NavigateTo("/dashboard")
- [x] Při chybě → `TmAlert` s chybovou zprávou z API
- [x] **GREEN:** Všechny testy prochází
- [x] **REFACTOR:** CSS styling dle UI-UX-003 (orange focus, input states)

---

## T-101: UC-002 Přihlášení uživatele ✅

### T-101.1: Backend - LoginRequestValidator (TDD) ✅
- [x] **TEST:** `LoginRequestValidator_EmptyEmail_ReturnsError`
- [x] **TEST:** `LoginRequestValidator_InvalidEmail_ReturnsError`
- [x] **TEST:** `LoginRequestValidator_EmptyPassword_ReturnsError`
- [x] **TEST:** `LoginRequestValidator_ValidRequest_NoErrors`
- [x] Vytvořit `LoginRequestValidator : AbstractValidator<LoginRequest>` s lokalizovanými zprávami z `.resx`
- [x] **GREEN:** 4/4 testy prochází

### T-101.2: Backend - LoginService (TDD) ✅
- [x] **TEST:** `LoginService_ValidCredentials_ReturnsAuthResponse`
- [x] **TEST:** `LoginService_InvalidEmail_Returns401`
- [x] **TEST:** `LoginService_InvalidPassword_Returns401`
- [x] **TEST:** `LoginService_InvalidPassword_IncrementsFailedAttempts`
- [x] **TEST:** `LoginService_LockedAccount_Returns423Locked`
- [x] **TEST:** `LoginService_ValidLogin_ResetsFailedAttempts`
- [x] **TEST:** `LoginService_ValidLogin_UpdatesLastLoginAt`
- [x] **TEST:** `LoginService_FiveFailedAttempts_LocksAccount`
- [x] Vytvořit `ILoginService` interface
- [x] Implementovat `LoginService` - generic error message
- [x] Implementovat lockout logiku (5 pokusů → zámek)
- [x] **GREEN:** 8/8 testy prochází

### T-101.3: Backend - Login Endpoint ✅
- [x] **TEST:** `LoginEndpoint_ValidCredentials_Returns200WithTokens`
- [x] **TEST:** `LoginEndpoint_InvalidCredentials_Returns401`
- [x] **TEST:** `LoginEndpoint_NonexistentEmail_Returns401`
- [x] **TEST:** `LoginEndpoint_InvalidRequest_Returns400`
- [x] **TEST:** `LoginEndpoint_LockedAccount_Returns423`
- [x] Vytvořit `POST /api/v1/users/login` endpoint
- [x] **GREEN:** 5/5 integračních testů prochází

### T-101.4: Frontend - LoginModel a Validator ✅
- [x] **TEST:** `LoginModelValidator_EmptyEmail_ShowsError`
- [x] **TEST:** `LoginModelValidator_InvalidEmail_ShowsError`
- [x] **TEST:** `LoginModelValidator_EmptyPassword_ShowsError`
- [x] **TEST:** `LoginModelValidator_ValidModel_NoErrors`
- [x] Vytvořit `LoginModel` (Email, Password, RememberMe)
- [x] Vytvořit `LoginModelValidator : AbstractValidator<LoginModel>` s lokalizací z `.resx`
- [x] **GREEN:** 4/4 testy prochází

### T-101.5: Frontend - Login Page (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `LoginPage_Renders_EmailAndPasswordFields`
- [x] **TEST (bUnit):** `LoginPage_Renders_RememberMeCheckbox`
- [x] **TEST (bUnit):** `LoginPage_Renders_SubmitButton`
- [x] **TEST (bUnit):** `LoginPage_Renders_RegisterLink`
- [x] **TEST (bUnit):** `LoginPage_InvalidForm_ShowsErrorMessage`
- [x] **TEST (bUnit):** `LoginPage_SubmitValid_CallsAuthService`
- [x] Vytvořit `Login.razor` (`@page "/login"`)
- [x] `@inject IStringLocalizer<Login> L`
- [x] Layout: Centrovaný card (max-width 420px)
- [x] Logo + `@L["Title"]` heading + `@L["Subtitle"]`
- [x] `<EditForm>` + `<FluentValidationValidator />`
- [x] InputText pro Email a Password
- [x] InputCheckbox pro RememberMe
- [x] Submit button s loading stavem
- [x] Link "Zapomenuté heslo" → `/password-reset`
- [x] Google login button (placeholder)
- [x] Link na registraci
- [x] Error handling: `TmAlert` při neúspěšném loginu
- [x] Po úspěchu → redirect na `/dashboard`
- [x] **GREEN:** 6/6 testů prochází

### T-101.6: Frontend - HTTP interceptor pro JWT ✅
- [x] **TEST:** `SendAsync_UserAuthenticated_AddsAuthorizationHeader`
- [x] **TEST:** `SendAsync_UserNotAuthenticated_DoesNotAddAuthorizationHeader`
- [x] **TEST:** `SendAsync_TokenNull_DoesNotAddAuthorizationHeader`
- [x] **TEST:** `SendAsync_Receives401_AttemptsTokenRefresh`
- [x] **TEST:** `SendAsync_RefreshFails_Returns401Response`
- [x] Vytvořit `AuthorizationMessageHandler : DelegatingHandler`
- [x] Automaticky přidávat `Authorization: Bearer {token}` do requestů
- [x] Při 401 → zkusit refresh token → při selhání → redirect na login
- [x] Zaregistrovat handler v DI pro HttpClient
- [x] Přidat `RefreshTokenAsync` do `IAuthService`
- [x] **GREEN:** 5/5 testů prochází

---

## T-102: UC-004 Základní herní smyčka ✅

### T-102.1: Backend - WordRepository (TDD) ✅
- [x] **TEST:** `WordRepository_GetByDifficulty_ReturnsCorrectWords` → GREEN
- [x] **TEST:** `WordRepository_GetByCategory_ReturnsCorrectWords` → GREEN
- [x] **TEST:** `WordRepository_GetRandom_ReturnsRandomWord` → GREEN
- [x] **TEST:** `WordRepository_GetRandomBatch_ReturnsNonRepeating` → GREEN
- [x] **TEST:** `WordRepository_GetRandom_WithDifficultyFilter_ReturnsCorrectDifficulty` → GREEN
- [x] **TEST:** `WordRepository_GetRandomBatch_WithDifficultyFilter_ReturnsCorrectWords` → GREEN
- [x] Vytvořit `IWordRepository` interface v Core/Interfaces/
- [x] Implementovat `WordRepository` v Infrastructure/Persistence/Repositories/
- [x] Implementovat GetByDifficulty, GetByCategory, GetRandom, GetRandomBatch
- [x] **GREEN:** 6/6 testů prochází

### T-102.2: Backend - Scramble algoritmus (TDD) ✅
- [x] **TEST:** `ScrambleService_Scramble_UsesFisherYatesShuffle` → GREEN
- [x] **TEST:** `ScrambleService_Scramble_NeverReturnsOriginal` → GREEN
- [x] **TEST:** `ScrambleService_Scramble_ContainsSameLetters` → GREEN
- [x] **TEST:** `ScrambleService_Scramble_HandlesShortWords` (2-3 písmena) → GREEN
- [x] **TEST:** `ScrambleService_Scramble_HandlesDuplicateLetters` ("ANNA") → GREEN
- [x] **TEST:** `ScrambleService_Scramble_AllIdenticalLetters_ReturnsOriginal` → GREEN
- [x] **TEST:** `ScrambleService_Scramble_MultipleRunsProduceDifferentResults` → GREEN
- [x] Implementovat Fisher-Yates shuffle v `Word.Scramble()`
- [x] Zajistit že výsledek ≠ originál (retry pokud ano)
- [x] **GREEN:** 8/8 testů prochází

### T-102.3: Backend - XP Calculation Service (TDD) ✅
- [x] **TEST:** `XpCalculator_CorrectAnswer_Returns10BaseXP` → GREEN
- [x] **TEST:** `XpCalculator_FastAnswer_Under3s_Returns5SpeedBonus` → GREEN
- [x] **TEST:** `XpCalculator_FastAnswer_Under5s_Returns3SpeedBonus` → GREEN
- [x] **TEST:** `XpCalculator_FastAnswer_Under10s_Returns1SpeedBonus` → GREEN
- [x] **TEST:** `XpCalculator_SlowAnswer_NoSpeedBonus` → GREEN
- [x] **TEST:** `XpCalculator_Combo3Plus_Returns1Point2Multiplier` → GREEN
- [x] **TEST:** `XpCalculator_Combo5Plus_Returns1Point5Multiplier` → GREEN
- [x] **TEST:** `XpCalculator_Combo10Plus_Returns2xMultiplier` → GREEN
- [x] **TEST:** `XpCalculator_WrongAnswer_Returns0XP` → GREEN
- [x] **TEST:** `XpCalculator_StreakBonus_5PlusCorrect_Returns2ExtraXP` → GREEN
- [x] **TEST:** `XpCalculator_FullCalculation_WithAllBonuses` → GREEN
- [x] **TEST:** `XpCalculator_StreakBonus_VariousStreaks` (Theory) → GREEN
- [x] Vytvořit `IXpCalculator` interface
- [x] Implementovat `XpCalculator` s formulí: `Floor((Base + SpeedBonus) * ComboMultiplier) + StreakBonus`
- [x] **GREEN:** 19/19 testů prochází
- [x] **REFACTOR:** Konstanty extrahovány jako private const

### T-102.4: Backend - GameSessionService (TDD) ✅
- [x] **TEST:** `GameSessionService_StartGame_CreatesSession` → GREEN
- [x] **TEST:** `GameSessionService_StartGame_SetsCorrectMode` → GREEN
- [x] **TEST:** `GameSessionService_StartGame_GeneratesFirstRound` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_Correct_IncreasesXP` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_Correct_IncreasesCombo` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_Wrong_ResetsCombo` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_Wrong_DecreasesLife` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_CaseInsensitive_Lowercase` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_CaseInsensitive_Uppercase` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_CaseInsensitive_MixedCase` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_TrimsWhitespace_Leading` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_TrimsWhitespace_Trailing` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_TrimsWhitespace_Both` → GREEN
- [x] **TEST:** `GameSessionService_SubmitAnswer_DiacriticsMustMatch` → GREEN
- [x] **TEST:** `GameSessionService_NextRound_GeneratesNewScrambledWord` → GREEN
- [x] **TEST:** `GameSessionService_AllRoundsComplete_EndsSession` → GREEN
- [x] Vytvořit `IGameSessionService` interface
- [x] Vytvořit `StartGameRequest` DTO v Shared (Mode, PathId?, DifficultyLevel)
- [x] Vytvořit `SubmitAnswerRequest` DTO v Shared (SessionId, Answer, TimeSpentMs)
- [x] Vytvořit `GameRoundResult` DTO v Shared (IsCorrect, CorrectAnswer, XPEarned, SpeedBonus, ComboCount, IsLevelComplete)
- [x] Vytvořit `ScrambledWordDto` DTO v Shared (SessionId, RoundNumber, ScrambledWord, WordLength, Difficulty, TimeLimit)
- [x] Implementovat `GameSessionService`
- [x] **GREEN:** 16/16 testů prochází
- [x] **REFACTOR:** Vyčištěno, rozděleny odpovědnosti

### T-102.5: Backend - SubmitAnswerValidator (TDD) ✅
- [x] **TEST:** `SubmitAnswerValidator_EmptyAnswer_ReturnsError` → GREEN
- [x] **TEST:** `SubmitAnswerValidator_AnswerTooLong_ReturnsError` (> 50 znaků) → GREEN
- [x] **TEST:** `SubmitAnswerValidator_EmptySessionId_ReturnsError` → GREEN
- [x] **TEST:** `SubmitAnswerValidator_NegativeTime_ReturnsError` → GREEN
- [x] **TEST:** `SubmitAnswerValidator_ValidRequest_NoErrors` → GREEN
- [x] **TEST:** `SubmitAnswerValidator_AnswerExactly50Characters_IsValid` → GREEN
- [x] **TEST:** `SubmitAnswerValidator_ZeroTime_IsValid` → GREEN
- [x] Vytvořit `SubmitAnswerValidator : AbstractValidator<SubmitAnswerRequest>` s lokalizací
- [x] **GREEN:** 7/7 testů prochází

### T-102.6: Backend - Game Endpoints ✅
- [x] **TEST:** `StartGameEndpoint_ValidRequest_Returns201WithScrambledWord` → GREEN
- [x] **TEST:** `StartGameEndpoint_Unauthorized_Returns401` → GREEN
- [x] **TEST:** `SubmitAnswerEndpoint_CorrectAnswer_Returns200WithResult` → GREEN
- [x] **TEST:** `SubmitAnswerEndpoint_WrongAnswer_Returns200WithZeroXP` → GREEN
- [x] **TEST:** `SubmitAnswerEndpoint_SessionIdMismatch_Returns400` → GREEN
- [x] **TEST:** `GetGameStateEndpoint_ExistingSession_Returns200` → GREEN
- [x] **TEST:** `GetGameStateEndpoint_NonExistentSession_Returns404` → GREEN
- [x] **TEST:** `ForfeitGameEndpoint_ValidRequest_Returns204` → GREEN
- [x] Vytvořit `POST /api/v1/game/start` endpoint (vrací ScrambledWordDto)
- [x] Vytvořit `POST /api/v1/game/{id}/answer` endpoint (vrací GameRoundResult)
- [x] Vytvořit `GET /api/v1/game/{id}` endpoint (vrací stav hry)
- [x] Vytvořit `POST /api/v1/game/{id}/forfeit` endpoint
- [x] Všechny endpointy vyžadují `[Authorize]`
- [x] **GREEN:** 8/8 testů prochází

### T-102.7: Frontend - GameService ✅
- [x] **TEST:** `GameService_StartGame_ReturnsScrambledWord` → GREEN
- [x] **TEST:** `GameService_StartGame_Unauthorized_ReturnsNull` → GREEN
- [x] **TEST:** `GameService_SubmitAnswer_ReturnsResult` → GREEN
- [x] **TEST:** `GameService_SubmitAnswer_WrongAnswer_ReturnsZeroXP` → GREEN
- [x] **TEST:** `GameService_GetGameState_ReturnsState` → GREEN
- [x] **TEST:** `GameService_GetGameState_NotFound_ReturnsNull` → GREEN
- [x] **TEST:** `GameService_ForfeitGame_ReturnsTrue` → GREEN
- [x] **TEST:** `GameService_ForfeitGame_Failure_ReturnsFalse` → GREEN
- [x] Vytvořit `IGameService` interface v Blazor/Services/
- [x] Implementovat `GameService` - HTTP volání na game endpointy
- [x] **GREEN:** 8/8 testů prochází

### T-102.8: Frontend - GameArena komponenta ✅
- [x] **TEST (bUnit):** `GameArena_Renders_ScrambledWordLetters` → GREEN
- [x] **TEST (bUnit):** `GameArena_Renders_AnswerInput` → GREEN
- [x] **TEST (bUnit):** `GameArena_Renders_SubmitButton` → GREEN
- [x] **TEST (bUnit):** `GameArena_Renders_SkipButton` → GREEN
- [x] **TEST (bUnit):** `GameArena_Renders_LevelProgress` → GREEN
- [x] **TEST (bUnit):** `GameArena_WithCombo_ShowsComboBadge` → GREEN
- [x] **TEST (bUnit):** `GameArena_WithoutCombo_HidesComboBadge` → GREEN
- [x] **TEST (bUnit):** `GameArena_ShowResult_Correct_DisplaysSuccessFeedback` → GREEN
- [x] **TEST (bUnit):** `GameArena_ShowResult_Wrong_DisplaysErrorFeedback` → GREEN
- [x] **TEST (bUnit):** `GameArena_LevelComplete_ShowsOverlay` → GREEN
- [x] **TEST (bUnit):** `GameArena_UpdateScrambledWord_UpdatesDisplay` → GREEN
- [x] **TEST (bUnit):** `GameArena_EmptyAnswer_DisablesSubmitButton` → GREEN
- [x] **TEST (bUnit):** `GameArena_EnteredAnswer_EnablesSubmitButton` → GREEN
- [x] **TEST (bUnit):** `GameArena_EmptyScrambledWord_HidesLetters` → GREEN
- [x] **TEST (bUnit):** `GameArena_CorrectAnswerAfterShowResult_ClearsInput` → GREEN
- [x] **TEST (bUnit):** `GameArena_WrongAnswerAfterShowResult_KeepsInput` → GREEN
- [x] **TEST (bUnit):** `GameArena_InputWithWhitespace_TrimmedWhenValidated` → GREEN
- [x] Vytvořit `GameArena.razor` komponentu
- [x] `@inject IStringLocalizer<GameArena> L`
- [x] Header: zpět tlačítko, název levelu, combo badge
- [x] Scrambled Word Display: Velká písmena v boxech s animací
- [x] Answer Input: input s uppercase, placeholder
- [x] Submit a Skip tlačítka
- [x] Feedback: zelený/červený alert s animací
- [x] Level Complete overlay
- [x] **GREEN:** 17/17 testů prochází
- [x] **REFACTOR:** CSS animace (shake, pulse, deal-in)

### T-102.9: Frontend - Timer komponenta ✅
- [x] **TEST (bUnit):** `GameTimer_Renders_ProgressBar` → GREEN
- [x] **TEST (bUnit):** `GameTimer_Renders_TimeText` → GREEN
- [x] **TEST (bUnit):** `GameTimer_HighTime_ShowsNormalClass` → GREEN
- [x] **TEST (bUnit):** `GameTimer_MediumTime_ShowsWarningClass` → GREEN
- [x] **TEST (bUnit):** `GameTimer_LowTime_ShowsCriticalClass` → GREEN
- [x] **TEST (bUnit):** `GameTimer_ProgressBarWidth_CalculatedCorrectly` → GREEN
- [x] Vytvořit `GameTimer.razor` komponentu
- [x] Progress bar s vizuálním odpočtem
- [x] Barvy: zelená (>35%), oranžová (15-35%), červená (<15%) s pulsem
- [x] Čas zobrazení ve formátu MM:SS
- [x] EventCallback `OnTimeUp`
- [x] **GREEN:** 6/6 testů prochází

### T-102.10: Frontend - Game Page ✅
- [x] **TEST (bUnit):** `GamePage_InitialState_ShowsStartScreen` → GREEN
- [x] **TEST (bUnit):** `GamePage_HasModeButtons` → GREEN
- [x] **TEST (bUnit):** `GamePage_ClickTraining_StartsGame` → GREEN
- [x] **TEST (bUnit):** `GamePage_WhileLoading_ShowsSpinner` → GREEN
- [x] **TEST (bUnit):** `GamePage_WithSessionId_LoadsGameState` → GREEN
- [x] Vytvořit `Game.razor` stránku (`@page "/game"`, `@page "/game/{SessionId}"`)
- [x] `@inject IStringLocalizer<Game> L`
- [x] Orchestruje GameArena, Timer komponenty
- [x] Zobrazí loading state (spinner) při načítání
- [x] Error handling s retry tlačítkem
- [x] Start screen s výběrem herního módu
- [x] **GREEN:** 5/5 testů prochází

### T-102.11: Integrační test ✅
- [x] **TEST:** `CompleteGameCycle_Start_To_Answer_XP_To_NextRound_To_Complete` → GREEN
- [x] **TEST:** `GameCycle_XPCalculation_EndToEnd` → GREEN
- [x] **TEST:** `GameCycle_ComboMechanic_IncreasesWithCorrectAnswers` → GREEN
- [x] **TEST:** `GameCycle_WrongAnswer_ResetsCombo` → GREEN
- [x] **TEST:** `GameCycle_TimerMechanic_ExpiresSubmitsEmptyAnswer` → GREEN
- [x] Ověřit XP calculation end-to-end
- [x] Ověřit combo mechaniku
- [x] Ověřit timer mechaniku
- [x] **GREEN:** 5/5 integračních testů prochází

---

## T-103: UC-005 Životy systém ✅

### T-103.1: Backend - LivesService (TDD) ✅
- [x] **TEST:** `LivesService_GetMaxLives_TrainingMode_ReturnsInfinite` → GREEN
- [x] **TEST:** `LivesService_GetMaxLives_BeginnerPath_Returns5` → GREEN
- [x] **TEST:** `LivesService_GetMaxLives_IntermediatePath_Returns4` → GREEN
- [x] **TEST:** `LivesService_GetMaxLives_AdvancedPath_Returns3` → GREEN
- [x] **TEST:** `LivesService_GetMaxLives_ExpertPath_Returns3` → GREEN
- [x] **TEST:** `LivesService_LoseLife_DecrementsCount` → GREEN
- [x] **TEST:** `LivesService_LoseLife_AtZero_ReturnsGameOver` → GREEN
- [x] **TEST:** `LivesService_RegenerateLife_AfterInterval_IncrementsCount` → GREEN
- [x] **TEST:** `LivesService_RegenerateLife_AtMax_DoesNotExceed` → GREEN
- [x] **TEST:** `LivesService_LoseLife_SetsNextRegenTime` → GREEN
- [x] **TEST:** `LivesService_RefillLives_SetsToMax` → GREEN
- [x] **TEST:** `LivesService_LoseLife_InTrainingMode_DoesNotDecrement` → GREEN
- [x] **TEST:** `LivesService_GetLivesStatus_TrainingMode_ReturnsInfinite` → GREEN
- [x] Vytvořit `ILivesService` interface
- [x] Vytvořit `LivesStatus` DTO v Shared (Current, Max, NextRegenAt, IsInfinite)
- [x] Implementovat `LivesService` s regeneration logic per difficulty
- [x] **GREEN:** 14/14 testů prochází

### T-103.2: Backend - LiveRegenerationService (Background) ✅
- [x] Vytvořit regenerační logiku v LivesService
- [x] Advanced: regen každých 60 min, Intermediate: každých 30 min, Beginner: 20 min
- [x] Automatická regenerace při získání statusu životů
- [x] **GREEN:** Logika implementována

### T-103.3: Backend - Integrace do GameSession ✅
- [x] Upravit `GameSessionService.SubmitAnswer` - dekrementovat životy při chybné odpovědi
- [x] Přidat `LivesRemaining` do `GameRoundResult` DTO
- [x] **GREEN:** Životy integrovány do herní smyčky

### T-103.4: Frontend - LivesIndicator komponenta (Tempo.Blazor) ✅
- [x] Vytvořit `LivesIndicator.razor` komponentu
- [x] `@inject IStringLocalizer<LivesIndicator> L`
- [x] Zobrazit srdíčka s plnými/prázdnými stavy
- [x] Regen timer zobrazení
- [x] Animace: srdce se zmenší při ztrátě (scale down + fade)
- [x] **GREEN:** Komponenta vytvořena

---

## T-104: UC-006 XP a Level systém ✅

### T-104.1: Backend - LevelCalculator (TDD) ✅
- [x] **TEST:** `LevelCalculator_Level1_Requires100XP` → GREEN
- [x] **TEST:** `LevelCalculator_Level2_Requires150XP` → GREEN
- [x] **TEST:** `LevelCalculator_Level5_Requires506XP` → GREEN
- [x] **TEST:** `LevelCalculator_Level10_Requires3844XP` → GREEN
- [x] **TEST:** `LevelCalculator_GetLevel_FromTotalXP_ReturnsCorrectLevel` → GREEN
- [x] **TEST:** `LevelCalculator_GetProgress_ReturnsPercentageInCurrentLevel` → GREEN
- [x] **TEST:** `LevelCalculator_DetectLevelUp_WhenXPCrossesThreshold_ReturnsTrue` → GREEN
- [x] **TEST:** `LevelCalculator_DetectLevelUp_WhenXPBelowThreshold_ReturnsFalse` → GREEN
- [x] Vytvořit `ILevelCalculator` interface
- [x] Implementovat `LevelCalculator` s exponenciální křivkou: `XP_needed = 100 * 1.5^(level-1)`
- [x] Vytvořit `XPProgress` DTO v Shared (TotalXP, CurrentLevel, XPInCurrentLevel, XPRequiredForNextLevel, ProgressPercentage)
- [x] **GREEN:** Všechny testy prochází

### T-104.2: Backend - XP Gain Processing (TDD) ✅
- [x] **TEST:** `XpService_AddXP_UpdatesUserStats` → GREEN
- [x] **TEST:** `XpService_AddXP_DetectsLevelUp_ReturnsUnlocks` → GREEN
- [x] **TEST:** `XpService_AddXP_MultipleLevelUps_HandlesCorrectly` → GREEN
- [x] **TEST:** `XpService_AddXP_UnlocksPath2_AtLevel3` → GREEN
- [x] **TEST:** `XpService_AddXP_UnlocksLeagues_AtLevel5` → GREEN
- [x] **TEST:** `XpService_AddXP_NoLevelUp_NoUnlocks` → GREEN
- [x] **TEST:** `XpService_AddXP_FromDailyChallenge_ReturnsCorrectSource` → GREEN
- [x] **TEST:** `XpService_AddXP_FromStreak_ReturnsCorrectSource` → GREEN
- [x] **TEST:** `XpService_AddXP_UpdatesUserLevelProperty` → GREEN
- [x] **TEST:** `XpService_AddXP_ReturnsCorrectTotalXP` → GREEN
- [x] Vytvořit `IXpService` interface
- [x] Implementovat `XpService` - aktualizuje UserStats, detekuje level-up
- [x] Vytvořit `XPGainedEvent` DTO (Amount, Source, Breakdown, LeveledUp, NewLevel, Unlocks)
- [x] Vytvořit `UnlockableReward` DTO (Type, Name, Description)
- [x] Definovat unlocks per level (Level 3: Path 2, Level 5: Leagues, Level 10: Path 3, atd.)
- [x] **GREEN:** 10/10 testů prochází

### T-104.3: Frontend - XpBar komponenta (Tempo.Blazor) ✅
- [x] Vytvořit `XpBar.razor` komponentu
- [x] `@inject IStringLocalizer<XpBar> L`
- [x] `TmProgressBar` s gradientem (oranžová → žlutá), Mode: Determinate
- [x] Level číslo vlevo: `TmBadge` s aktuálním levelem
- [x] XP text format "{current}/{required}"
- [x] Animace: smooth fill při získání XP (CSS transition)
- [x] **GREEN:** Komponenta vytvořena

### T-104.4: Frontend - LevelUp Modal (Tempo.Blazor) ✅
- [x] Vytvořit `LevelUpModal.razor` komponentu
- [x] `TmModal` (Size: Medium) s confetti animací na pozadí
- [x] Velké číslo nového levelu s pulsem
- [x] Odměny: `TmCard` karty s unlocked features
- [x] `TmButton` "Pokračovat" → zavře modal
- [x] **GREEN:** Komponenta vytvořena

---

## T-105: UC-007 Cesty - Backend ✅

### T-105.1: Domain Entities - Path, Level ✅
- [x] **TEST:** `LearningPath_Create_SetsDefaultValues` → GREEN
- [x] **TEST:** `PathLevel_Create_InitializesAsLocked` → GREEN
- [x] **TEST:** `PathLevel_Unlock_ChangesStatusToAvailable` → GREEN
- [x] **TEST:** `PathLevel_Complete_ChangesStatusToCompleted` → GREEN
- [x] **TEST:** `PathLevel_CompletePerfect_SetsIsPerfect` → GREEN
- [x] Vytvořit `LearningPath` entitu (Id, Name, Description, Difficulty, TotalLevels, WordLengthMin, WordLengthMax, TimePerWord, HintPolicy)
- [x] Vytvořit `PathLevel` entitu (Id, PathId, LevelNumber, Status, WordCount, IsBoss, CompletedAt, IsPerfect)
- [x] Vytvořit `LevelStatus` enum (Locked, Available, Current, Completed, Perfect, Boss)
- [x] Vytvořit `CompletedLevel` entitu (UserId, LevelId, CompletedAt, XPEarned, IsPerfect)
- [x] **GREEN:** Testy prochází

### T-105.2: PathService (TDD) ✅
- [x] **TEST:** `PathService_GetPaths_Returns4Paths` → GREEN
- [x] **TEST:** `PathService_GetPathProgress_ReturnsCompletedLevels` → GREEN
- [x] **TEST:** `PathService_IsPathUnlocked_BeginnerAlwaysTrue` → GREEN
- [x] **TEST:** `PathService_IsPathUnlocked_Intermediate_RequiresPath1OrLevel5` → GREEN
- [x] **TEST:** `PathService_IsPathUnlocked_Advanced_RequiresPath2Complete` → GREEN
- [x] **TEST:** `PathService_IsPathUnlocked_Expert_RequiresPath3Complete` → GREEN
- [x] Vytvořit `IPathService` interface
- [x] Vytvořit DTOs v Shared: `LearningPathDto`, `PathLevelDto`, `PathProgressDto`
- [x] Implementovat `PathService`
- [x] **GREEN:** 8/8 testů prochází

### T-105.3: Path Endpoints ✅
- [x] Vytvořit `GET /api/v1/paths` endpoint (vrací seznam cest s progress)
- [x] Vytvořit `GET /api/v1/paths/{pathId}/levels` endpoint (vrací levely v cestě)
- [x] Vytvořit `GET /api/v1/paths/{pathId}/levels/{levelId}` endpoint (detail levelu)
- [x] **GREEN:** Endpoints vytvořeny

### T-105.4: Seed data pro cesty ✅
- [x] Vytvořit seed data: Path 1 (Beginner 🌱, 20 levelů, 3-5 písmen)
- [x] Vytvořit seed data: Path 2 (Intermediate 🌿, 25 levelů, 5-7 písmen)
- [x] Vytvořit seed data: Path 3 (Advanced 🌳, 30 levelů, 7-10 písmen)
- [x] Vytvořit seed data: Path 4 (Expert 🔥, 40 levelů, 10+ písmen)
- [x] Přidat boss levely na správné pozice (po X levelech)
- [x] **GREEN:** Seed data vytvořeno

---

## T-106: UC-007 Cesty - Frontend ✅

### T-106.1: PathService Frontend ✅
- [x] Vytvořit `IPathService` interface v Blazor/Services/
- [x] Implementovat `PathService` - HTTP volání na path endpointy
- [x] **GREEN:** Service vytvořen

### T-106.2: PathSelector Page (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `PathSelector_Renders_4PathCards` → GREEN
- [x] **TEST (bUnit):** `PathSelector_LockedPath_ShowsLockIcon` → GREEN
- [x] **TEST (bUnit):** `PathSelector_UnlockedPath_ShowsProgress` → GREEN
- [x] Vytvořit `Paths.razor` stránku (`@page "/paths"`)
- [x] `@inject IStringLocalizer<Paths> L`
- [x] 4× `TmCard` (Elevated) pro každou cestu
- [x] Cestu 1 (🌱): zelený gradient, vždy odemčená
- [x] Cestu 2 (🌿): modro-zelený gradient, podmíněně odemčená
- [x] Cestu 3 (🌳): hnědý gradient, podmíněně odemčená
- [x] Cestu 4 (🔥): červeno-oranžový gradient, podmíněně odemčená
- [x] Zamčené cesty: `TmIcon` (lock), opacity 0.6, `TmTooltip` s požadavky
- [x] Progress bar: `TmProgressBar` s completion %
- [x] `TmBadge` s difficulty level
- [x] Kliknutí → navigace na `/paths/{pathId}`
- [x] **GREEN:** Testy prochází

### T-106.3: PathMap komponenta (Level nodes) ✅
- [x] **TEST (bUnit):** `PathMap_Renders_AllLevelNodes` → GREEN
- [x] **TEST (bUnit):** `PathMap_CurrentLevel_HasPulsingEffect` → GREEN
- [x] **TEST (bUnit):** `PathMap_BossLevel_ShowsCrownIcon` → GREEN
- [x] Vytvořit `PathMap.razor` komponentu
- [x] Tree/network vizualizace s propojenými uzly (SVG paths)
- [x] Level nodes s barevnými stavy:
  - Completed: zelený `TmIcon` (check), bright green bg
  - Current: oranžový s pulsem (CSS animation)
  - Locked: šedý `TmIcon` (lock), opacity 0.6
  - Boss: gradient red-orange, `TmIcon` (crown), gold border
- [x] Čísla levelů v uzlech
- [x] SVG connecting lines s animací (stroke-dasharray)
- [x] `TmProgressBar` celkový progress cesty nahoře
- [x] **GREEN:** Testy prochází

### T-106.4: LevelDetail Modal (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `LevelDetailModal_Renders_LevelInfo` → GREEN
- [x] **TEST (bUnit):** `LevelDetailModal_StartButton_NavigatesToGame` → GREEN
- [x] Vytvořit `LevelDetailModal.razor`
- [x] `TmModal` (Size: Medium) s level info
- [x] Info sekce: počet slov, čas na slovo, hints, životy, odměny (XP)
- [x] `TmButton` "Začít level" → navigace na `/game/{sessionId}`
- [x] Zobrazí preview scrambled word
- [x] **GREEN:** Testy prochází

---

## T-107: UC-011 Streak systém ✅

### T-107.1: Backend - StreakService (TDD) ✅
- [x] **TEST:** `StreakService_CheckStreak_FirstDay_Returns1` → GREEN
- [x] **TEST:** `StreakService_CheckStreak_ConsecutiveDay_Increments` → GREEN
- [x] **TEST:** `StreakService_CheckStreak_SameDay_NoChange` → GREEN
- [x] **TEST:** `StreakService_CheckStreak_MissedDay_ResetsTo1` → GREEN
- [x] **TEST:** `StreakService_CheckStreak_GracePeriod48h_DoesNotReset` → GREEN
- [x] **TEST:** `StreakService_CheckStreak_UpdatesLongest_WhenCurrentExceedsLongest` → GREEN
- [x] **TEST:** `StreakService_GetFireLevel_0Days_ReturnsCold` → GREEN
- [x] **TEST:** `StreakService_GetFireLevel_1to3Days_ReturnsSmall` → GREEN
- [x] **TEST:** `StreakService_GetFireLevel_4to7Days_ReturnsMedium` → GREEN
- [x] **TEST:** `StreakService_GetFireLevel_8to30Days_ReturnsLarge` → GREEN
- [x] **TEST:** `StreakService_GetFireLevel_30PlusDays_ReturnsLegendary` → GREEN
- [x] **TEST:** `StreakService_CheckStreak_ReturnsCorrectFireLevel` → GREEN
- [x] **TEST:** `StreakService_CheckStreak_AtRisk_ReturnsTrue` → GREEN
- [x] **TEST:** `StreakService_CheckStreak_NoActivity_ReturnsCorrectTimeRemaining` → GREEN
- [x] **TEST:** `StreakService_CheckStreak_ReturnsLongestDays` → GREEN
- [x] **TEST:** `StreakService_GetFireLevel_ReturnsCorrectLevel` → GREEN (Theory test)
- [x] Vytvořit `IStreakService` interface
- [x] Vytvořit `StreakStatus` DTO v Shared (CurrentDays, LongestDays, FireLevel, NextResetAt, TimeRemaining, IsAtRisk)
- [x] Vytvořit `FireLevel` enum (Cold, Small, Medium, Large, Legendary)
- [x] Implementovat `StreakService`
- [x] **GREEN:** 19/19 testů prochází

### T-107.2: Backend - Streak Endpoint ✅
- [x] Vytvořit `GET /api/v1/users/me/streak` endpoint
- [x] **GREEN:** Endpoint vytvořen

### T-107.3: Frontend - StreakIndicator komponenta (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `StreakIndicator_Renders_CurrentStreakDays` → GREEN
- [x] **TEST (bUnit):** `StreakIndicator_ShowsCorrectFireEmoji_ByLevel` → GREEN
- [x] **TEST (bUnit):** `StreakIndicator_AtRisk_ShowsWarningState` → GREEN
- [x] **TEST (bUnit):** `StreakIndicator_ShowsMilestoneProgress` → GREEN
- [x] Vytvořit `StreakIndicator.razor` komponentu
- [x] `@inject IStringLocalizer<StreakIndicator> L`
- [x] Fire emoji dle úrovně: 🔥 (small), 🔥🔥 (medium), 🔥🔥🔥 (large), 🔥🔥🔥🔥 (legendary)
- [x] Dny s pluralizací: `@GetStreakText(days)` (1 den, 2-4 dny, 5+ dní)
- [x] At-risk stav: `TmBadge Variant="Warning"` s pulsující animací, countdown timer
- [x] `TmTooltip` s popisem: `@L["Tooltip.Description"]`
- [x] Fire animace: CSS keyframes (pulse, glow efekt)
- [x] **GREEN:** Testy prochází

---

## T-108: UC-015 Dashboard a Statistiky ✅

### T-108.1: Backend - Dashboard Endpoint (TDD) ✅
- [x] **TEST:** `DashboardEndpoint_Returns200WithUserDashboard` → GREEN
- [x] **TEST:** `DashboardEndpoint_ContainsStatsSummary` → GREEN
- [x] **TEST:** `DashboardEndpoint_ContainsActivityHeatmap` → GREEN
- [x] **TEST:** `DashboardEndpoint_ContainsPathProgress` → GREEN
- [x] **TEST:** `DashboardEndpoint_ContainsRecentAchievements` → GREEN
- [x] Vytvořit `UserDashboard` DTO v Shared (Stats, Heatmap, PathProgress, RecentAchievements, DailyChallenge, LeagueInfo)
- [x] Vytvořit `UserStatsSummary` DTO (TotalXP, CurrentLevel, CurrentStreak, LongestStreak, Accuracy, AverageTime, TotalWordsSolved)
- [x] Vytvořit `ActivityHeatmap` DTO (Days: List<HeatmapDay>)
- [x] Vytvořit `HeatmapDay` DTO (Date, LevelsCompleted, XPGained, IntensityLevel 0-4)
- [x] Vytvořit `GET /api/v1/stats/dashboard` endpoint
- [x] Implementovat dashboard aggregation service
- [x] **GREEN:** Testy prochází

### T-108.2: Frontend - Dashboard Page (Tempo.Blazor) ✅
- [x] **TEST (bUnit):** `DashboardPage_Renders_StatCards` → GREEN
- [x] **TEST (bUnit):** `DashboardPage_Renders_DailyChallengeCard` → GREEN
- [x] **TEST (bUnit):** `DashboardPage_Renders_LeagueCard` → GREEN
- [x] Vytvořit `Dashboard.razor` stránku (`@page "/dashboard"`)
- [x] `@inject IStringLocalizer<Dashboard> L`
- [x] Loading state: 4× `TmSkeleton` (Rectangle) + `TmSkeleton` karty
- [x] **Stat Cards** (4 sloupce): 4× `TmStatCard`
  - XP: `TmStatCard` s `TmIcon` (star), hodnota s count-up animací
  - Streak: `TmStatCard` s StreakIndicator komponentou
  - Accuracy: `TmStatCard` s `TmIcon` (target), procenta
  - Avg Time: `TmStatCard` s `TmIcon` (clock), sekundy
- [x] **Daily Challenge**: `TmCard` s `TmBadge` (modifier tag), scrambled word preview, `TmButton` "Hrát"
- [x] **League Card**: `TmCard` s `TmIcon` (trophy), rank pozice, `TmProgressBar` k promo/demo
- [x] **Paths Progress**: 4× `TmProgressBar` s cestami (barvy dle difficulty)
- [x] **Quick Actions**: 4× `TmButton` karty (Training, Time Attack, 1v1, Shop) v `TmCard`
- [x] Stagger animace na load (100ms intervals)
- [x] **GREEN:** Testy prochází
- [x] **REFACTOR:** Responsive layout (4 col → 2×2 → stack)

### T-108.3: Frontend - ActivityHeatmap komponenta ✅
- [x] **TEST (bUnit):** `ActivityHeatmap_Renders_30DayGrid` → GREEN
- [x] **TEST (bUnit):** `ActivityHeatmap_CorrectColors_ByIntensity` → GREEN
- [x] **TEST (bUnit):** `ActivityHeatmap_Hover_ShowsTooltip` → GREEN
- [x] Vytvořit `ActivityHeatmap.razor` komponentu
- [x] GitHub-style grid: 7 řádků × ~5 sloupců (30 dní)
- [x] Barvy dle intenzity: 0=#ebedf0, 1=#9be9a8, 2=#40c463, 3=#30a14e, 4=#216e39
- [x] `TmTooltip` na hover: datum, počet levelů, XP
- [x] Hover efekt: scale 1.3x
- [x] **GREEN:** Testy prochází

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

- [x] Registrace funguje end-to-end (FE → API → DB → Token)
- [x] Login funguje end-to-end včetně lockout
- [x] Herní smyčka kompletní: start → scramble → answer → XP → next → complete
- [x] Životy fungují správně s regenerací
- [x] XP systém s level-up detekcí a unlocks
- [x] 4 cesty s level nodes a vizualizací
- [x] Streak systém s fire indikátorem a milestones
- [x] Dashboard s KPI kartami, heatmapou, progress bary
- [x] Všechny texty z .resx souborů
- [x] FluentValidation na FE (Tempo.Blazor.FluentValidation) i BE
- [x] `dotnet test` → všechny testy zelené (166/166)
- [x] Responsive design na mobilu i desktopu

---

## Statistiky implementace

| Součást | Počet testů | Stav |
|---------|-------------|------|
| RegisterRequestValidator | 14 | ✅ GREEN |
| UserService | 7 | ✅ GREEN |
| LoginService | 8 | ✅ GREEN |
| XpCalculator | 19 | ✅ GREEN |
| GameSessionService | 16 | ✅ GREEN |
| SubmitAnswerValidator | 7 | ✅ GREEN |
| LevelCalculator | 11 | ✅ GREEN |
| XpService | 10 | ✅ GREEN |
| LivesService | 14 | ✅ GREEN |
| StreakService | 19 | ✅ GREEN |
| PathService | 8 | ✅ GREEN |
| **Celkem Core** | **166** | **✅ GREEN** |
