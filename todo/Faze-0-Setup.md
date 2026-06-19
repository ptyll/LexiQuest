# Fáze 0: Základní Setup (Příprava) ✅

> **Cíl:** Vytvořit kompletní projektovou strukturu, databázi, autentizaci, lokalizaci a caching.
> **Technologie:** .NET 10, Blazor, MSSQL LocalDB, EF Core, ASP.NET Core Identity, JWT, FluentValidation
> **NuGet balíčky:** Tempo.Blazor, Tempo.Blazor.Abstractions, Tempo.Blazor.FluentValidation
> **Status:** ✅ **HOTOVÉ**

---

## ⚠️ KRITICKÁ PRAVIDLA

- **TDD:** Test FIRST → RED → GREEN → REFACTOR
- **Žádné hardcoded texty** → vše z `.resx` souborů
- **FluentValidation** na FE (Tempo.Blazor.FluentValidation) i BE
- **DTOs** vždy v `LexiQuest.Shared`
- **Žádné wrapper třídy** pro API odpovědi → HTTP status kódy
- **Unit of Work** pattern pro atomické transakce
- **Produkční kód** od prvního řádku

---

## T-000: Projektová struktura ✅

### T-000.1: Solution a projekty ✅
- [x] Vytvořit solution soubor `LexiQuest.sln` v root adresáři
- [x] Vytvořit projekt `src/LexiQuest.Api/LexiQuest.Api.csproj` (ASP.NET Core Web API, .NET 10)
- [x] Vytvořit projekt `src/LexiQuest.Core/LexiQuest.Core.csproj` (Class Library, .NET 10)
- [x] Vytvořit projekt `src/LexiQuest.Infrastructure/LexiQuest.Infrastructure.csproj` (Class Library, .NET 10)
- [x] Vytvořit projekt `src/LexiQuest.Blazor/LexiQuest.Blazor.csproj` (Blazor WebAssembly Standalone, .NET 10)
- [x] Vytvořit projekt `src/LexiQuest.Shared/LexiQuest.Shared.csproj` (Class Library, .NET 10)

### T-000.2: Testovací projekty ✅
- [x] Vytvořit projekt `tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj` (xUnit, .NET 10)
- [x] Vytvořit projekt `tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj` (xUnit, .NET 10)
- [x] Vytvořit projekt `tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj` (xUnit + bUnit, .NET 10)
- [x] Vytvořit projekt `tests/LexiQuest.Infrastructure.Tests/LexiQuest.Infrastructure.Tests.csproj` (xUnit, .NET 10)

### T-000.3: Projektové reference ✅
- [x] `LexiQuest.Api` → reference na `Core`, `Infrastructure`, `Shared`
- [x] `LexiQuest.Core` → reference na `Shared`
- [x] `LexiQuest.Infrastructure` → reference na `Core`, `Shared`
- [x] `LexiQuest.Blazor` → reference na `Shared`
- [x] `LexiQuest.Core.Tests` → reference na `Core`, `Shared`
- [x] `LexiQuest.Api.Tests` → reference na `Api`, `Core`, `Infrastructure`, `Shared`
- [x] `LexiQuest.Blazor.Tests` → reference na `Blazor`, `Shared`
- [x] `LexiQuest.Infrastructure.Tests` → reference na `Infrastructure`, `Core`, `Shared`

### T-000.4: NuGet balíčky - Backend ✅
- [x] `LexiQuest.Api` → Microsoft.AspNetCore.Authentication.JwtBearer, FluentValidation.AspNetCore, Serilog.AspNetCore, Swashbuckle.AspNetCore, Hangfire.AspNetCore, Hangfire.SqlServer
- [x] `LexiQuest.Core` → FluentValidation, Microsoft.Extensions.Localization.Abstractions
- [x] `LexiQuest.Infrastructure` → Microsoft.EntityFrameworkCore.SqlServer, Microsoft.AspNetCore.Identity.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Tools
- [x] `LexiQuest.Shared` → FluentValidation (pro shared validátory), System.ComponentModel.Annotations

### T-000.5: NuGet balíčky - Frontend (Tempo.Blazor) ✅
- [x] `LexiQuest.Blazor` → Tempo.Blazor (hlavní komponenty)
- [x] `LexiQuest.Blazor` → Tempo.Blazor.Abstractions (rozhraní a modely)
- [x] `LexiQuest.Blazor` → Tempo.Blazor.FluentValidation (FluentValidationValidator)
- [x] `LexiQuest.Blazor` → Microsoft.Extensions.Http (HttpClient DI)
- [x] `LexiQuest.Blazor` → Microsoft.Extensions.Localization (IStringLocalizer)

### T-000.6: NuGet balíčky - Testy ✅
- [x] Všechny test projekty → xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk
- [x] Všechny test projekty → NSubstitute, FluentAssertions
- [x] `LexiQuest.Blazor.Tests` → bunit
- [x] `LexiQuest.Api.Tests` → Microsoft.AspNetCore.Mvc.Testing, Microsoft.EntityFrameworkCore.InMemory

### T-000.7: Základní konfigurace API projektu ✅
- [x] Vytvořit `Program.cs` s minimální konfigurací (builder + app pipeline)
- [x] Vytvořit `appsettings.json` s MSSQL connection stringem pro LocalDB
- [x] Vytvořit `appsettings.Development.json`
- [x] Nastavit CORS policy pro Blazor WASM origin
- [x] Nastavit Swagger/OpenAPI
- [x] Nastavit Serilog (console + file sink)
- [x] Přidat health check endpoint (`/health`)

### T-000.8: Základní konfigurace Blazor projektu ✅
- [x] Vytvořit `Program.cs` s registrací služeb
- [x] Zaregistrovat `AddTempoBlazor()` v DI containeru
- [x] Zaregistrovat `AddTempoFluentValidation()` se scan assembly
- [x] Nastavit `HttpClient` s base address na API
- [x] Přidat Tempo.Blazor CSS do `index.html` (tempo-blazor.css)
- [x] Nastavit lokalizaci (CultureInfo "cs")
- [x] Vytvořit základní `MainLayout.razor` s TmTopBar a TmSidebar
- [x] Vytvořit `App.razor` s routingem

### T-000.9: Ověření buildu ✅
- [x] Ověřit že `dotnet build` projde bez chyb pro celý solution
- [x] Ověřit že `dotnet test` projde (prázdné testy)
- [x] Ověřit že API startuje a vrací Swagger UI
- [x] Ověřit že Blazor WASM startuje a zobrazí prázdnou stránku s TmTopBar

---

## T-001: Databázová infrastruktura (MSSQL) ✅

### T-001.1: DbContext - TDD ✅
- [x] **TEST:** Napsat test `LexiQuestDbContext_Constructor_CreatesInstance` → RED
- [x] Vytvořit `LexiQuestDbContext` v Infrastructure projektu
- [x] Přidat DbSet<User>, DbSet<Word>, DbSet<GameSession> (prázdné entity zatím)
- [x] **GREEN:** Test prochází
- [x] **REFACTOR:** Vyčistit DbContext

### T-001.2: Domain Entity - User ✅
- [x] **TEST:** Napsat test `User_Create_SetsDefaultValues` (Id, Stats, Streak) → RED
- [x] Vytvořit `User` entitu v Core/Domain/Entities/
- [x] Vytvořit `UserStats` value object (TotalXP, Level, Accuracy, TotalWordsSolved, AverageResponseTime)
- [x] Vytvořit `UserPreferences` value object (Theme, Language, AnimationsEnabled, SoundsEnabled)
- [x] Vytvořit `Streak` value object (CurrentDays, LongestDays, LastActivityDate)
- [x] Vytvořit `PremiumStatus` value object (IsPremium, ExpiresAt, Plan)
- [x] **GREEN:** Test prochází
- [x] **REFACTOR:** Přidat privátní settery, encapsulace

### T-001.3: Domain Entity - Word ✅
- [x] **TEST:** Napsat test `Word_Create_SetsProperties` → RED
- [x] **TEST:** Napsat test `Word_Scramble_ReturnsDifferentOrder` (Fisher-Yates) → RED
- [x] **TEST:** Napsat test `Word_Scramble_NeverReturnsOriginal` → RED
- [x] Vytvořit `Word` entitu v Core/Domain/Entities/
- [x] Vytvořit `DifficultyLevel` enum (Beginner, Intermediate, Advanced, Expert)
- [x] Vytvořit `WordCategory` enum (Animals, Food, Colors, Nature, Technology, atd.)
- [x] Implementovat `Scramble(Random rng)` metodu s Fisher-Yates shuffle
- [x] **GREEN:** Všechny testy prochází
- [x] **REFACTOR:** Optimalizovat Scramble algoritmus

### T-001.4: Domain Entity - GameSession ✅
- [x] **TEST:** Napsat test `GameSession_Create_InitializesCorrectly` → RED
- [x] **TEST:** Napsat test `GameSession_AddRound_AddsToRoundsList` → RED
- [x] Vytvořit `GameSession` entitu v Core/Domain/Entities/
- [x] Vytvořit `GameRound` entitu
- [x] Vytvořit `GameMode` enum (Training, Timed, Path, DailyChallenge, Boss)
- [x] Vytvořit `GameSessionStatus` enum (Active, Completed, Failed, Abandoned)
- [x] **GREEN:** Testy prochází
- [x] **REFACTOR:** Encapsulace, invarianty

### T-001.5: EF Core konfigurace ✅
- [x] Vytvořit `UserConfiguration : IEntityTypeConfiguration<User>` (indexy na Email, Username)
- [x] Vytvořit `WordConfiguration : IEntityTypeConfiguration<Word>` (index na Difficulty, Category)
- [x] Vytvořit `GameSessionConfiguration : IEntityTypeConfiguration<GameSession>`
- [x] Vytvořit `GameRoundConfiguration : IEntityTypeConfiguration<GameRound>`
- [x] Nastavit owned types pro UserStats, UserPreferences, Streak, PremiumStatus
- [x] Přidat konfigurace do DbContext (OnModelCreating)

### T-001.6: MSSQL Connection a migrace ✅
- [x] Zaregistrovat DbContext v `Program.cs` s `UseSqlServer` a retry policy
- [x] Vytvořit initial migration: `dotnet ef migrations add InitialCreate`
- [x] Aplikovat migraci: `dotnet ef database update`
- [x] Ověřit že databáze existuje v LocalDB

### T-001.7: Seed data ✅
- [x] **TEST:** Napsat test `WordSeedData_Contains_MinimumWords` (alespoň 100 slov) → RED
- [x] Vytvořit `SeedData` třídu v Infrastructure/Persistence/
- [x] Vytvořit seed data pro slova - Beginner (3-5 písmen): 50 slov (zvířata, jídlo, barvy)
- [x] Vytvořit seed data pro slova - Intermediate (5-7 písmen): 30 slov
- [x] Vytvořit seed data pro slova - Advanced (7-10 písmen): 15 slov
- [x] Vytvořit seed data pro slova - Expert (10+ písmen): 5 slov
- [x] Implementovat `HasData()` v EF Core konfiguraci nebo `IHostedService` seeder
- [x] **GREEN:** Test prochází
- [x] Ověřit seed data v databázi po migraci

### T-001.8: Unit of Work pattern ✅
- [x] **TEST:** Napsat test `UnitOfWork_SaveChanges_PersistsAllChanges` → RED
- [x] Vytvořit `IUnitOfWork` interface v Core/Interfaces/
- [x] Implementovat `UnitOfWork` v Infrastructure (wrapping DbContext.SaveChangesAsync)
- [x] Zaregistrovat v DI jako Scoped
- [x] **GREEN:** Test prochází

---

## T-002: Autentizační infrastruktura ✅

### T-002.1: ASP.NET Core Identity setup ✅
- [x] Přidat `IdentityUser` base class do User entity (nebo vlastní ApplicationUser)
- [x] Nastavit `AddIdentity<User, IdentityRole>()` v Program.cs
- [x] Konfigurovat password policy (min 8, uppercase, lowercase, digit, special)
- [x] Konfigurovat lockout policy (5 failed attempts, 15 min lockout)
- [x] Vytvořit Identity migraci a aplikovat

### T-002.2: JWT konfigurace ✅
- [x] Přidat JWT settings do appsettings.json (Issuer, Audience, SecretKey, AccessTokenExpiry, RefreshTokenExpiry)
- [x] Vytvořit `JwtSettings` POCO třídu v Shared
- [x] Nastavit `AddAuthentication().AddJwtBearer()` v Program.cs
- [x] Konfigurovat token validation parameters

### T-002.3: Token Service - TDD ✅
- [x] **TEST:** Napsat test `TokenService_GenerateAccessToken_ReturnsValidJwt` → RED
- [x] **TEST:** Napsat test `TokenService_GenerateAccessToken_ContainsUserClaims` (userId, email, username) → RED
- [x] **TEST:** Napsat test `TokenService_GenerateRefreshToken_ReturnsUniqueToken` → RED
- [x] **TEST:** Napsat test `TokenService_ValidateRefreshToken_ReturnsTrueForValid` → RED
- [x] **TEST:** Napsat test `TokenService_ValidateRefreshToken_ReturnsFalseForExpired` → RED
- [x] Vytvořit `ITokenService` interface v Core/Interfaces/
- [x] Implementovat `TokenService` v Infrastructure/Auth/
- [x] **GREEN:** Všechny testy prochází
- [x] Zaregistrovat v DI

### T-002.4: Refresh Token persistence ✅
- [x] **TEST:** Napsat test `RefreshToken_Entity_StoresCorrectData` → RED
- [x] Vytvořit `RefreshToken` entitu (Token, UserId, ExpiresAt, CreatedAt, RevokedAt, ReplacedByToken)
- [x] Vytvořit EF Core konfiguraci pro RefreshToken
- [x] Přidat migraci a aplikovat
- [x] **GREEN:** Test prochází

### T-002.5: Auth middleware ✅
- [x] Nastavit `UseAuthentication()` a `UseAuthorization()` v pipeline
- [x] Vytvořit `[Authorize]` attribute na chráněných endpointech
- [x] Vytvořit extension metodu `GetUserId()` na ClaimsPrincipal

### T-002.6: Auth DTOs (Shared projekt) ✅
- [x] Vytvořit `RegisterRequest` DTO (Email, Username, Password, ConfirmPassword, AcceptTerms)
- [x] Vytvořit `LoginRequest` DTO (Email, Password, RememberMe)
- [x] Vytvořit `AuthResponse` DTO (AccessToken, RefreshToken, ExpiresAt)
- [x] Vytvořit `RefreshTokenRequest` DTO (RefreshToken)
- [x] Vytvořit `UserDto` DTO (Id, Email, Username, Stats, Streak, Premium)

---

## T-003: Resource soubory struktura ✅

### T-003.1: Blazor Resources - Stránky ✅
- [x] Vytvořit adresář `LexiQuest.Blazor/Resources/Pages/`
- [x] Vytvořit `Login.resx` s klíči: Title, Subtitle, Input.Email.Label, Input.Email.Placeholder, Input.Password.Label, Input.Password.Placeholder, Link.ForgotPassword, Button.Submit, Button.Google, Register.Prompt, Register.Link, Error.InvalidCredentials, Error.AccountLocked, Loading.Text
- [x] Vytvořit `Register.resx` s klíči: Title, Subtitle, Input.Email.Label/Placeholder, Input.Username.Label/Placeholder, Input.Password.Label/Placeholder, Input.ConfirmPassword.Label/Placeholder, Checkbox.Terms, Button.Submit, Button.Google, Login.Prompt, Login.Link, Loading.Text
- [x] Vytvořit `Dashboard.resx` s klíči: Title, Stat.XP, Stat.Streak, Stat.Accuracy, Stat.AvgTime, DailyChallenge.Title, League.Title, Paths.Title, Achievements.Title, QuickAction.Training, QuickAction.TimeAttack, QuickAction.Duel, QuickAction.Shop
- [x] Vytvořit `Game.resx` s klíči: Timer.Label, Lives.Label, Hint.Button, Hint.Cost, Answer.Placeholder, Answer.Submit, Answer.Skip, Combo.Multiplier, Speed.Bonus, Level.Progress, GameOver.Title, GameOver.Retry, LevelComplete.Title
- [x] Vytvořit `Paths.resx`, `BossLevel.resx`, `Leagues.resx`, `Statistics.resx`, `Achievements.resx`, `Profile.resx`, `Premium.resx`, `Shop.resx`, `Multiplayer.resx`, `Settings.resx`, `DailyChallenge.resx`, `PasswordReset.resx`, `NotFound.resx`

### T-003.2: Blazor Resources - Komponenty ✅
- [x] Vytvořit adresář `LexiQuest.Blazor/Resources/Components/`
- [x] Vytvořit `GameArena.resx` s klíči dle docs/resources/ResourceStructure.md
- [x] Vytvořit `StreakIndicator.resx` s klíči pro dny, status, tooltip, shield
- [x] Vytvořit `XpBar.resx`, `LivesIndicator.resx`, `Leaderboard.resx`, `Heatmap.resx`, `Timer.resx`, `HintButton.resx`, `WordDisplay.resx`, `AchievementCard.resx`, `LeagueCard.resx`, `PathNode.resx`, `BossModifiers.resx`, `ShopItem.resx`, `AvatarSelector.resx`, `ComboDisplay.resx`, `LevelComplete.resx`, `GameOver.resx`, `Notifications.resx`

### T-003.3: Blazor Resources - Shared a Validace ✅
- [x] Vytvořit adresář `LexiQuest.Blazor/Resources/Shared/`
- [x] Vytvořit `Navigation.resx` (Home, Game, Leagues, Profile, Settings, Logout, atd.)
- [x] Vytvořit `Footer.resx` (Copyright, Terms, Privacy, Contact)
- [x] Vytvořit `Loading.resx` (Loading.Default, Loading.Game, Loading.Data)
- [x] Vytvořit `ErrorBoundary.resx` (Error.Title, Error.Message, Error.Retry)
- [x] Vytvořit `ConfirmDialog.resx` (Confirm.Title, Confirm.Yes, Confirm.No, Confirm.Cancel)
- [x] Vytvořit adresář `LexiQuest.Blazor/Resources/Validation/`
- [x] Vytvořit `ValidationMessages.resx` se všemi validačními klíči (Validation.Email.Required, Validation.Email.Invalid, Validation.Password.MinLength, Validation.Password.Uppercase, Validation.Password.Lowercase, Validation.Password.Digit, Validation.Password.Special, Validation.Password.Mismatch, Validation.Username.Required, Validation.Username.MinLength, Validation.Username.MaxLength, Validation.Username.Invalid, Validation.Terms.Required, Validation.Answer.Empty, Validation.Answer.TooLong)

### T-003.4: API Resources ✅
- [x] Vytvořit adresář `LexiQuest.Api/Resources/Validation/`
- [x] Vytvořit `ValidationMessages.resx` (stejné klíče jako Blazor + API-specifické)
- [x] Vytvořit adresář `LexiQuest.Api/Resources/Errors/`
- [x] Vytvořit `ErrorMessages.resx` (Error.NotFound, Error.Unauthorized, Error.Forbidden, Error.Conflict, Error.Internal)
- [x] Vytvořit adresář `LexiQuest.Api/Resources/Email/`
- [x] Vytvořit `WelcomeEmail.resx`, `PasswordResetEmail.resx`

### T-003.5: Lokalizace konfigurace ✅
- [x] Nastavit `AddLocalization()` v API Program.cs
- [x] Nastavit `AddLocalization()` v Blazor Program.cs
- [x] Konfigurovat `RequestLocalizationOptions` s default culture "cs"
- [x] Ověřit že `IStringLocalizer<Login>` resolvuje správné texty
- [x] Vytvořit helper extension `GetLocalizedString()` pro pluralizaci (čeština: 1 den, 2-4 dny, 5+ dní)

---

## T-004: In-Memory Caching ✅

### T-004.1: Cache Service - TDD ✅
- [x] **TEST:** Napsat test `CacheService_GetOrCreate_ReturnsCachedValue` → RED
- [x] **TEST:** Napsat test `CacheService_GetOrCreate_CallsFactoryOnMiss` → RED
- [x] **TEST:** Napsat test `CacheService_Remove_InvalidatesCache` → RED
- [x] **TEST:** Napsat test `CacheService_GetOrCreate_RespectsExpiration` → RED
- [x] Vytvořit `ICacheService` interface v Core/Interfaces/
- [x] Implementovat `MemoryCacheService : ICacheService` v Infrastructure/Caching/
- [x] Definovat cache key konstanty (`CacheKeys` static class)
- [x] **GREEN:** Všechny testy prochází
- [x] **REFACTOR:** Přidat generické metody

### T-004.2: Cache Policies ✅
- [x] Definovat `CachePolicy` record (AbsoluteExpiration, SlidingExpiration)
- [x] Vytvořit přednastavené politiky: ShortLived (5min), MediumLived (30min), LongLived (2h), Daily (24h)
- [x] Zaregistrovat `IMemoryCache` a `ICacheService` v DI
- [x] Ověřit caching funguje v integraci

---

## Tempo.Blazor komponenty použité v této fázi

| Komponenta | Použití |
|------------|---------|
| `TmTopBar` | Hlavní navigační lišta |
| `TmSidebar` | Boční navigace |
| `TmSpinner` | Loading stavy při startu |
| `TmAlert` | Chybové zprávy |
| `TmButton` | Základní tlačítka |
| `ThemeService` | Dark/light mode |

---

## Ověření dokončení fáze ✅

- [x] `dotnet build` projde bez chyb
- [x] `dotnet test` projde - všechny testy zelené (Core + Infrastructure)
- [x] Databáze vytvořena s tabulkami User, Word, GameSession, GameRound, RefreshToken
- [x] Seed data (100+ slov) v databázi
- [x] JWT autentizace konfigurována
- [x] Resource soubory vytvořeny a IStringLocalizer funguje
- [x] In-Memory cache funguje
- [x] Blazor WASM startuje s TmTopBar a TmSidebar
- [x] API Swagger UI dostupné

---

## Statistiky implementace

| Součást | Stav |
|---------|------|
| Projekty | ✅ 5 src + 4 test |
| Entity Framework | ✅ Migrace funguje |
| Identity + JWT | ✅ Autentizace funguje |
| Lokalizace | ✅ .resx soubory |
| Caching | ✅ In-Memory |
| Testy | ✅ 523+ testů |
