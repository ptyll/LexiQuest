# Fáze 0: Základní Setup (Příprava)

> **Cíl:** Vytvořit kompletní projektovou strukturu, databázi, autentizaci, lokalizaci a caching.
> **Technologie:** .NET 10, Blazor, MSSQL LocalDB, EF Core, ASP.NET Core Identity, JWT, FluentValidation
> **NuGet balíčky:** Tempo.Blazor, Tempo.Blazor.Abstractions, Tempo.Blazor.FluentValidation

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

## T-000: Projektová struktura

### T-000.1: Solution a projekty
- [ ] Vytvořit solution soubor `LexiQuest.sln` v root adresáři
- [ ] Vytvořit projekt `src/LexiQuest.Api/LexiQuest.Api.csproj` (ASP.NET Core Web API, .NET 10)
- [ ] Vytvořit projekt `src/LexiQuest.Core/LexiQuest.Core.csproj` (Class Library, .NET 10)
- [ ] Vytvořit projekt `src/LexiQuest.Infrastructure/LexiQuest.Infrastructure.csproj` (Class Library, .NET 10)
- [ ] Vytvořit projekt `src/LexiQuest.Blazor/LexiQuest.Blazor.csproj` (Blazor WebAssembly Standalone, .NET 10)
- [ ] Vytvořit projekt `src/LexiQuest.Shared/LexiQuest.Shared.csproj` (Class Library, .NET 10)

### T-000.2: Testovací projekty
- [ ] Vytvořit projekt `tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj` (xUnit, .NET 10)
- [ ] Vytvořit projekt `tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj` (xUnit, .NET 10)
- [ ] Vytvořit projekt `tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj` (xUnit + bUnit, .NET 10)
- [ ] Vytvořit projekt `tests/LexiQuest.Infrastructure.Tests/LexiQuest.Infrastructure.Tests.csproj` (xUnit, .NET 10)

### T-000.3: Projektové reference
- [ ] `LexiQuest.Api` → reference na `Core`, `Infrastructure`, `Shared`
- [ ] `LexiQuest.Core` → reference na `Shared`
- [ ] `LexiQuest.Infrastructure` → reference na `Core`, `Shared`
- [ ] `LexiQuest.Blazor` → reference na `Shared`
- [ ] `LexiQuest.Core.Tests` → reference na `Core`, `Shared`
- [ ] `LexiQuest.Api.Tests` → reference na `Api`, `Core`, `Infrastructure`, `Shared`
- [ ] `LexiQuest.Blazor.Tests` → reference na `Blazor`, `Shared`
- [ ] `LexiQuest.Infrastructure.Tests` → reference na `Infrastructure`, `Core`, `Shared`

### T-000.4: NuGet balíčky - Backend
- [ ] `LexiQuest.Api` → Microsoft.AspNetCore.Authentication.JwtBearer, FluentValidation.AspNetCore, Serilog.AspNetCore, Swashbuckle.AspNetCore, Hangfire.AspNetCore, Hangfire.SqlServer
- [ ] `LexiQuest.Core` → FluentValidation, Microsoft.Extensions.Localization.Abstractions
- [ ] `LexiQuest.Infrastructure` → Microsoft.EntityFrameworkCore.SqlServer, Microsoft.AspNetCore.Identity.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Tools
- [ ] `LexiQuest.Shared` → FluentValidation (pro shared validátory), System.ComponentModel.Annotations

### T-000.5: NuGet balíčky - Frontend (Tempo.Blazor)
- [ ] `LexiQuest.Blazor` → Tempo.Blazor (hlavní komponenty)
- [ ] `LexiQuest.Blazor` → Tempo.Blazor.Abstractions (rozhraní a modely)
- [ ] `LexiQuest.Blazor` → Tempo.Blazor.FluentValidation (FluentValidationValidator)
- [ ] `LexiQuest.Blazor` → Microsoft.Extensions.Http (HttpClient DI)
- [ ] `LexiQuest.Blazor` → Microsoft.Extensions.Localization (IStringLocalizer)

### T-000.6: NuGet balíčky - Testy
- [ ] Všechny test projekty → xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk
- [ ] Všechny test projekty → NSubstitute, FluentAssertions
- [ ] `LexiQuest.Blazor.Tests` → bunit
- [ ] `LexiQuest.Api.Tests` → Microsoft.AspNetCore.Mvc.Testing, Microsoft.EntityFrameworkCore.InMemory

### T-000.7: Základní konfigurace API projektu
- [ ] Vytvořit `Program.cs` s minimální konfigurací (builder + app pipeline)
- [ ] Vytvořit `appsettings.json` s MSSQL connection stringem pro LocalDB
- [ ] Vytvořit `appsettings.Development.json`
- [ ] Nastavit CORS policy pro Blazor WASM origin
- [ ] Nastavit Swagger/OpenAPI
- [ ] Nastavit Serilog (console + file sink)
- [ ] Přidat health check endpoint (`/health`)

### T-000.8: Základní konfigurace Blazor projektu
- [ ] Vytvořit `Program.cs` s registrací služeb
- [ ] Zaregistrovat `AddTempoBlazor()` v DI containeru
- [ ] Zaregistrovat `AddTempoFluentValidation()` se scan assembly
- [ ] Nastavit `HttpClient` s base address na API
- [ ] Přidat Tempo.Blazor CSS do `index.html` (tempo-blazor.css)
- [ ] Nastavit lokalizaci (CultureInfo "cs")
- [ ] Vytvořit základní `MainLayout.razor` s TmTopBar a TmSidebar
- [ ] Vytvořit `App.razor` s routingem

### T-000.9: Ověření buildu
- [ ] Ověřit že `dotnet build` projde bez chyb pro celý solution
- [ ] Ověřit že `dotnet test` projde (prázdné testy)
- [ ] Ověřit že API startuje a vrací Swagger UI
- [ ] Ověřit že Blazor WASM startuje a zobrazí prázdnou stránku s TmTopBar

---

## T-001: Databázová infrastruktura (MSSQL)

### T-001.1: DbContext - TDD
- [ ] **TEST:** Napsat test `LexiQuestDbContext_Constructor_CreatesInstance` → RED
- [ ] Vytvořit `LexiQuestDbContext` v Infrastructure projektu
- [ ] Přidat DbSet<User>, DbSet<Word>, DbSet<GameSession> (prázdné entity zatím)
- [ ] **GREEN:** Test prochází
- [ ] **REFACTOR:** Vyčistit DbContext

### T-001.2: Domain Entity - User
- [ ] **TEST:** Napsat test `User_Create_SetsDefaultValues` (Id, Stats, Streak) → RED
- [ ] Vytvořit `User` entitu v Core/Domain/Entities/
- [ ] Vytvořit `UserStats` value object (TotalXP, Level, Accuracy, TotalWordsSolved, AverageResponseTime)
- [ ] Vytvořit `UserPreferences` value object (Theme, Language, AnimationsEnabled, SoundsEnabled)
- [ ] Vytvořit `Streak` value object (CurrentDays, LongestDays, LastActivityDate)
- [ ] Vytvořit `PremiumStatus` value object (IsPremium, ExpiresAt, Plan)
- [ ] **GREEN:** Test prochází
- [ ] **REFACTOR:** Přidat privátní settery, encapsulace

### T-001.3: Domain Entity - Word
- [ ] **TEST:** Napsat test `Word_Create_SetsProperties` → RED
- [ ] **TEST:** Napsat test `Word_Scramble_ReturnsDifferentOrder` (Fisher-Yates) → RED
- [ ] **TEST:** Napsat test `Word_Scramble_NeverReturnsOriginal` → RED
- [ ] Vytvořit `Word` entitu v Core/Domain/Entities/
- [ ] Vytvořit `DifficultyLevel` enum (Beginner, Intermediate, Advanced, Expert)
- [ ] Vytvořit `WordCategory` enum (Animals, Food, Colors, Nature, Technology, atd.)
- [ ] Implementovat `Scramble(Random rng)` metodu s Fisher-Yates shuffle
- [ ] **GREEN:** Všechny testy prochází
- [ ] **REFACTOR:** Optimalizovat Scramble algoritmus

### T-001.4: Domain Entity - GameSession
- [ ] **TEST:** Napsat test `GameSession_Create_InitializesCorrectly` → RED
- [ ] **TEST:** Napsat test `GameSession_AddRound_AddsToRoundsList` → RED
- [ ] Vytvořit `GameSession` entitu v Core/Domain/Entities/
- [ ] Vytvořit `GameRound` entitu
- [ ] Vytvořit `GameMode` enum (Training, Timed, Path, DailyChallenge, Boss)
- [ ] Vytvořit `GameSessionStatus` enum (Active, Completed, Failed, Abandoned)
- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Encapsulace, invarianty

### T-001.5: EF Core konfigurace
- [ ] Vytvořit `UserConfiguration : IEntityTypeConfiguration<User>` (indexy na Email, Username)
- [ ] Vytvořit `WordConfiguration : IEntityTypeConfiguration<Word>` (index na Difficulty, Category)
- [ ] Vytvořit `GameSessionConfiguration : IEntityTypeConfiguration<GameSession>`
- [ ] Vytvořit `GameRoundConfiguration : IEntityTypeConfiguration<GameRound>`
- [ ] Nastavit owned types pro UserStats, UserPreferences, Streak, PremiumStatus
- [ ] Přidat konfigurace do DbContext (OnModelCreating)

### T-001.6: MSSQL Connection a migrace
- [ ] Zaregistrovat DbContext v `Program.cs` s `UseSqlServer` a retry policy
- [ ] Vytvořit initial migration: `dotnet ef migrations add InitialCreate`
- [ ] Aplikovat migraci: `dotnet ef database update`
- [ ] Ověřit že databáze existuje v LocalDB

### T-001.7: Seed data
- [ ] **TEST:** Napsat test `WordSeedData_Contains_MinimumWords` (alespoň 100 slov) → RED
- [ ] Vytvořit `SeedData` třídu v Infrastructure/Persistence/
- [ ] Vytvořit seed data pro slova - Beginner (3-5 písmen): 50 slov (zvířata, jídlo, barvy)
- [ ] Vytvořit seed data pro slova - Intermediate (5-7 písmen): 30 slov
- [ ] Vytvořit seed data pro slova - Advanced (7-10 písmen): 15 slov
- [ ] Vytvořit seed data pro slova - Expert (10+ písmen): 5 slov
- [ ] Implementovat `HasData()` v EF Core konfiguraci nebo `IHostedService` seeder
- [ ] **GREEN:** Test prochází
- [ ] Ověřit seed data v databázi po migraci

### T-001.8: Unit of Work pattern
- [ ] **TEST:** Napsat test `UnitOfWork_SaveChanges_PersistsAllChanges` → RED
- [ ] Vytvořit `IUnitOfWork` interface v Core/Interfaces/
- [ ] Implementovat `UnitOfWork` v Infrastructure (wrapping DbContext.SaveChangesAsync)
- [ ] Zaregistrovat v DI jako Scoped
- [ ] **GREEN:** Test prochází

---

## T-002: Autentizační infrastruktura

### T-002.1: ASP.NET Core Identity setup
- [ ] Přidat `IdentityUser` base class do User entity (nebo vlastní ApplicationUser)
- [ ] Nastavit `AddIdentity<User, IdentityRole>()` v Program.cs
- [ ] Konfigurovat password policy (min 8, uppercase, lowercase, digit, special)
- [ ] Konfigurovat lockout policy (5 failed attempts, 15 min lockout)
- [ ] Vytvořit Identity migraci a aplikovat

### T-002.2: JWT konfigurace
- [ ] Přidat JWT settings do appsettings.json (Issuer, Audience, SecretKey, AccessTokenExpiry, RefreshTokenExpiry)
- [ ] Vytvořit `JwtSettings` POCO třídu v Shared
- [ ] Nastavit `AddAuthentication().AddJwtBearer()` v Program.cs
- [ ] Konfigurovat token validation parameters

### T-002.3: Token Service - TDD
- [ ] **TEST:** Napsat test `TokenService_GenerateAccessToken_ReturnsValidJwt` → RED
- [ ] **TEST:** Napsat test `TokenService_GenerateAccessToken_ContainsUserClaims` (userId, email, username) → RED
- [ ] **TEST:** Napsat test `TokenService_GenerateRefreshToken_ReturnsUniqueToken` → RED
- [ ] **TEST:** Napsat test `TokenService_ValidateRefreshToken_ReturnsTrueForValid` → RED
- [ ] **TEST:** Napsat test `TokenService_ValidateRefreshToken_ReturnsFalseForExpired` → RED
- [ ] Vytvořit `ITokenService` interface v Core/Interfaces/
- [ ] Implementovat `TokenService` v Infrastructure/Auth/
- [ ] **GREEN:** Všechny testy prochází
- [ ] Zaregistrovat v DI

### T-002.4: Refresh Token persistence
- [ ] **TEST:** Napsat test `RefreshToken_Entity_StoresCorrectData` → RED
- [ ] Vytvořit `RefreshToken` entitu (Token, UserId, ExpiresAt, CreatedAt, RevokedAt, ReplacedByToken)
- [ ] Vytvořit EF Core konfiguraci pro RefreshToken
- [ ] Přidat migraci a aplikovat
- [ ] **GREEN:** Test prochází

### T-002.5: Auth middleware
- [ ] Nastavit `UseAuthentication()` a `UseAuthorization()` v pipeline
- [ ] Vytvořit `[Authorize]` attribute na chráněných endpointech
- [ ] Vytvořit extension metodu `GetUserId()` na ClaimsPrincipal

### T-002.6: Auth DTOs (Shared projekt)
- [ ] Vytvořit `RegisterRequest` DTO (Email, Username, Password, ConfirmPassword, AcceptTerms)
- [ ] Vytvořit `LoginRequest` DTO (Email, Password, RememberMe)
- [ ] Vytvořit `AuthResponse` DTO (AccessToken, RefreshToken, ExpiresAt)
- [ ] Vytvořit `RefreshTokenRequest` DTO (RefreshToken)
- [ ] Vytvořit `UserDto` DTO (Id, Email, Username, Stats, Streak, Premium)

---

## T-003: Resource soubory struktura

### T-003.1: Blazor Resources - Stránky
- [ ] Vytvořit adresář `LexiQuest.Blazor/Resources/Pages/`
- [ ] Vytvořit `Login.resx` s klíči: Title, Subtitle, Input.Email.Label, Input.Email.Placeholder, Input.Password.Label, Input.Password.Placeholder, Link.ForgotPassword, Button.Submit, Button.Google, Register.Prompt, Register.Link, Error.InvalidCredentials, Error.AccountLocked, Loading.Text
- [ ] Vytvořit `Register.resx` s klíči: Title, Subtitle, Input.Email.Label/Placeholder, Input.Username.Label/Placeholder, Input.Password.Label/Placeholder, Input.ConfirmPassword.Label/Placeholder, Checkbox.Terms, Button.Submit, Button.Google, Login.Prompt, Login.Link, Loading.Text
- [ ] Vytvořit `Dashboard.resx` s klíči: Title, Stat.XP, Stat.Streak, Stat.Accuracy, Stat.AvgTime, DailyChallenge.Title, League.Title, Paths.Title, Achievements.Title, QuickAction.Training, QuickAction.TimeAttack, QuickAction.Duel, QuickAction.Shop
- [ ] Vytvořit `Game.resx` s klíči: Timer.Label, Lives.Label, Hint.Button, Hint.Cost, Answer.Placeholder, Answer.Submit, Answer.Skip, Combo.Multiplier, Speed.Bonus, Level.Progress, GameOver.Title, GameOver.Retry, LevelComplete.Title
- [ ] Vytvořit `Paths.resx`, `BossLevel.resx`, `Leagues.resx`, `Statistics.resx`, `Achievements.resx`, `Profile.resx`, `Premium.resx`, `Shop.resx`, `Multiplayer.resx`, `Settings.resx`, `DailyChallenge.resx`, `PasswordReset.resx`, `NotFound.resx`

### T-003.2: Blazor Resources - Komponenty
- [ ] Vytvořit adresář `LexiQuest.Blazor/Resources/Components/`
- [ ] Vytvořit `GameArena.resx` s klíči dle docs/resources/ResourceStructure.md
- [ ] Vytvořit `StreakIndicator.resx` s klíči pro dny, status, tooltip, shield
- [ ] Vytvořit `XpBar.resx`, `LivesIndicator.resx`, `Leaderboard.resx`, `Heatmap.resx`, `Timer.resx`, `HintButton.resx`, `WordDisplay.resx`, `AchievementCard.resx`, `LeagueCard.resx`, `PathNode.resx`, `BossModifiers.resx`, `ShopItem.resx`, `AvatarSelector.resx`, `ComboDisplay.resx`, `LevelComplete.resx`, `GameOver.resx`, `Notifications.resx`

### T-003.3: Blazor Resources - Shared a Validace
- [ ] Vytvořit adresář `LexiQuest.Blazor/Resources/Shared/`
- [ ] Vytvořit `Navigation.resx` (Home, Game, Leagues, Profile, Settings, Logout, atd.)
- [ ] Vytvořit `Footer.resx` (Copyright, Terms, Privacy, Contact)
- [ ] Vytvořit `Loading.resx` (Loading.Default, Loading.Game, Loading.Data)
- [ ] Vytvořit `ErrorBoundary.resx` (Error.Title, Error.Message, Error.Retry)
- [ ] Vytvořit `ConfirmDialog.resx` (Confirm.Title, Confirm.Yes, Confirm.No, Confirm.Cancel)
- [ ] Vytvořit adresář `LexiQuest.Blazor/Resources/Validation/`
- [ ] Vytvořit `ValidationMessages.resx` se všemi validačními klíči (Validation.Email.Required, Validation.Email.Invalid, Validation.Password.MinLength, Validation.Password.Uppercase, Validation.Password.Lowercase, Validation.Password.Digit, Validation.Password.Special, Validation.Password.Mismatch, Validation.Username.Required, Validation.Username.MinLength, Validation.Username.MaxLength, Validation.Username.Invalid, Validation.Terms.Required, Validation.Answer.Empty, Validation.Answer.TooLong)

### T-003.4: API Resources
- [ ] Vytvořit adresář `LexiQuest.Api/Resources/Validation/`
- [ ] Vytvořit `ValidationMessages.resx` (stejné klíče jako Blazor + API-specifické)
- [ ] Vytvořit adresář `LexiQuest.Api/Resources/Errors/`
- [ ] Vytvořit `ErrorMessages.resx` (Error.NotFound, Error.Unauthorized, Error.Forbidden, Error.Conflict, Error.Internal)
- [ ] Vytvořit adresář `LexiQuest.Api/Resources/Email/`
- [ ] Vytvořit `WelcomeEmail.resx`, `PasswordResetEmail.resx`

### T-003.5: Lokalizace konfigurace
- [ ] Nastavit `AddLocalization()` v API Program.cs
- [ ] Nastavit `AddLocalization()` v Blazor Program.cs
- [ ] Konfigurovat `RequestLocalizationOptions` s default culture "cs"
- [ ] Ověřit že `IStringLocalizer<Login>` resolvuje správné texty
- [ ] Vytvořit helper extension `GetLocalizedString()` pro pluralizaci (čeština: 1 den, 2-4 dny, 5+ dní)

---

## T-004: In-Memory Caching

### T-004.1: Cache Service - TDD
- [ ] **TEST:** Napsat test `CacheService_GetOrCreate_ReturnsCachedValue` → RED
- [ ] **TEST:** Napsat test `CacheService_GetOrCreate_CallsFactoryOnMiss` → RED
- [ ] **TEST:** Napsat test `CacheService_Remove_InvalidatesCache` → RED
- [ ] **TEST:** Napsat test `CacheService_GetOrCreate_RespectsExpiration` → RED
- [ ] Vytvořit `ICacheService` interface v Core/Interfaces/
- [ ] Implementovat `MemoryCacheService : ICacheService` v Infrastructure/Caching/
- [ ] Definovat cache key konstanty (`CacheKeys` static class)
- [ ] **GREEN:** Všechny testy prochází
- [ ] **REFACTOR:** Přidat generické metody

### T-004.2: Cache Policies
- [ ] Definovat `CachePolicy` record (AbsoluteExpiration, SlidingExpiration)
- [ ] Vytvořit přednastavené politiky: ShortLived (5min), MediumLived (30min), LongLived (2h), Daily (24h)
- [ ] Zaregistrovat `IMemoryCache` a `ICacheService` v DI
- [ ] Ověřit caching funguje v integraci

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

## Ověření dokončení fáze

- [ ] `dotnet build` projde bez chyb
- [ ] `dotnet test` projde - všechny testy zelené
- [ ] Databáze vytvořena s tabulkami User, Word, GameSession, GameRound, RefreshToken
- [ ] Seed data (100+ slov) v databázi
- [ ] JWT autentizace konfigurována
- [ ] Resource soubory vytvořeny a IStringLocalizer funguje
- [ ] In-Memory cache funguje
- [ ] Blazor WASM startuje s TmTopBar a TmSidebar
- [ ] API Swagger UI dostupné
