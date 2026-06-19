# LexiQuest - AI Agent Guide

> **Language Note:** This project uses Czech language for all UI texts, documentation, and user-facing content. All resource files (.resx) are in Czech. Code, comments, and technical documentation may be in English.

## Project Overview

LexiQuest is an interactive Czech-language word puzzle game where players unscramble letters to form words. It features RPG-like progression with XP/levels, streaks, leagues, achievements, boss levels, and multiplayer modes.

**Target Framework:** .NET 10  
**Solution File:** `LexiQuest.slnx`

---

## Architecture

### Layered Architecture (Clean Architecture)

```
┌─────────────────────────────────────────────────────────┐
│  LexiQuest.Blazor (Blazor WebAssembly)                  │
│  - Pages, Components, Layout                            │
│  - Tempo.Blazor UI components                           │
│  - HttpClient services                                  │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  LexiQuest.Api (ASP.NET Core Web API)                   │
│  - Controllers, Endpoints                               │
│  - JWT Authentication                                   │
│  - Middleware                                           │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  LexiQuest.Core (Domain + Application)                  │
│  - Domain Entities, Value Objects                       │
│  - Domain Services, Specifications                      │
│  - Interfaces                                           │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  LexiQuest.Infrastructure                               │
│  - EF Core Repositories                                 │
│  - Identity/JWT Implementation                          │
│  - In-Memory Caching                                    │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│  LexiQuest.Shared                                       │
│  - DTOs (Data Transfer Objects)                         │
│  - Shared Validators                                    │
└─────────────────────────────────────────────────────────┘
```

### Project References

| Project | References |
|---------|------------|
| LexiQuest.Api | Core, Infrastructure, Shared |
| LexiQuest.Blazor | Shared |
| LexiQuest.Core | Shared |
| LexiQuest.Infrastructure | Core, Shared |
| LexiQuest.Core.Tests | Core, Shared |
| LexiQuest.Api.Tests | Api, Core, Infrastructure, Shared |
| LexiQuest.Blazor.Tests | Blazor, Shared |
| LexiQuest.Infrastructure.Tests | Infrastructure, Core, Shared |

---

## Technology Stack

| Layer | Technology | Package/Version |
|-------|------------|-----------------|
| Backend | .NET | 10.0 |
| Frontend | Blazor WebAssembly | .NET 10.0 |
| Database | Microsoft SQL Server | 2022+ / LocalDB |
| ORM | Entity Framework Core | 10.0.3 |
| Auth | ASP.NET Core Identity + JWT | 10.0.3 |
| Validation | FluentValidation | 12.1.1 |
| Logging | Serilog | 10.0.0 |
| Background Jobs | Hangfire + MSSQL | 1.8.23 |
| API Docs | Swashbuckle.AspNetCore | 10.1.4 |
| UI Components | Tempo.Blazor | 1.0.0-ci-* |
| Testing | xUnit, NSubstitute, FluentAssertions | Latest |
| Blazor Testing | bUnit | 2.6.2 |

### Tempo.Blazor Components

The project uses custom `Tempo.Blazor` NuGet packages from the author's GitHub:
- `Tempo.Blazor` - Main UI components (buttons, inputs, data tables, etc.)
- `Tempo.Blazor.Abstractions` - Interfaces and models
- `Tempo.Blazor.FluentValidation` - FluentValidation integration for Blazor

---

## Build and Run Commands

### Build
```bash
# Build entire solution
dotnet build LexiQuest.slnx

# Build specific project
dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj
dotnet build src/LexiQuest.Blazor/LexiQuest.Blazor.csproj
```

### Run
```bash
# Run API (from src/LexiQuest.Api directory)
cd src/LexiQuest.Api
dotnet run
# API will be available at: https://localhost:5000
# Swagger UI at: https://localhost:5000/swagger

# Run Blazor (from src/LexiQuest.Blazor directory)
cd src/LexiQuest.Blazor
dotnet run
# Blazor app at: https://localhost:5001
```

### Database Migrations
```bash
# Add migration (from src/LexiQuest.Api directory)
dotnet ef migrations add MigrationName --project ../LexiQuest.Infrastructure/LexiQuest.Infrastructure.csproj --startup-project .

# Update database
dotnet ef database update --project ../LexiQuest.Infrastructure/LexiQuest.Infrastructure.csproj --startup-project .
```

### Testing
```bash
# Run all tests
dotnet test

# Run with verbosity
dotnet test --verbosity normal

# Run specific test project
dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj
```

---

## Development Conventions

### 1. Test-Driven Development (TDD) - STRICT

```
Step 1: Write FAILING test (RED) - MUST fail before implementation
Step 2: Write MINIMAL code to pass (GREEN)
Step 3: Refactor (clean code without changing functionality)
Step 4: Repeat for next task
```

**Test Naming Convention:**
```csharp
[UnitOfWork]_[Scenario]_[ExpectedResult]

// Examples:
SubmitAnswer_CorrectAnswer_IncreasesXP
SubmitAnswer_WrongAnswer_DecreasesLife
StartGame_ExpertMode_AppliesTimeLimit
```

### 2. No Hardcoded Strings - ALL from Resources

❌ **FORBIDDEN:**
```csharp
<button>Save</button>
throw new Exception("Email is required");
```

✅ **REQUIRED:**
```csharp
<button>@Localizer["Button_Save"]</button>
throw new Exception(Localizer["Validation.Email.Required"]);
```

All UI texts must be in `.resx` files in the `Resources/` directories.

### 3. Fluent Validation

**Backend:**
```csharp
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(localizer["Validation.Email.Required"]);
    }
}
```

**Frontend (Blazor):**
Uses `Tempo.Blazor.FluentValidation` with `AddTempoFluentValidation()`.

### 4. DTOs Always in Shared Project

All request/response DTOs go to `LexiQuest.Shared/DTOs/` namespace.

### 5. No Wrapper Classes for API Responses

❌ **FORBIDDEN:**
```csharp
public class ApiResponse<T> { public T Data { get; set; } public bool Success { get; set; } }
```

✅ **REQUIRED:**
Use HTTP status codes directly. Return DTOs or ProblemDetails.

### 6. Unit of Work Pattern

Use `IUnitOfWork` for atomic transactions:

```csharp
public class SomeService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task DoWork()
    {
        // ... repository operations ...
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### 7. Code Style

- Use `ImplicitUsings` and `Nullable` enabled (configured in all projects)
- Prefer `var` when type is obvious
- Use expression-bodied members for simple methods
- Private fields: camelCase with underscore prefix
- Public properties: PascalCase
- Async methods: suffix with `Async`

---

## Localization

### Czech Language (Primary)

All user-facing content is in Czech. Resource files structure:

```
LexiQuest.Api/
└── Resources/
    ├── Validation/
    │   └── ValidationMessages.resx
    ├── Email/
    │   ├── WelcomeEmail.resx
    │   └── PasswordResetEmail.resx
    └── Errors/
        └── ErrorMessages.resx

LexiQuest.Blazor/
└── Resources/
    ├── Components/
    │   ├── GameArena.resx
    │   ├── XpBar.resx
    │   └── ...
    ├── Pages/
    │   ├── Dashboard.resx
    │   ├── Login.resx
    │   └── ...
    ├── Shared/
    │   └── Navigation.resx
    └── Validation/
        └── ValidationMessages.resx
```

### Culture Configuration

- API: Uses `AddLocalization()` with ResourcesPath = "Resources"
- Blazor: Culture is hardcoded to Czech (`new CultureInfo("cs")`)

---

## Database Configuration

### Development (MSSQL LocalDB)

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LexiQuest;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Production

```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=LexiQuest;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}
```

### EF Core Configuration

```csharp
builder.Services.AddDbContext<LexiQuestDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
```

---

## Authentication & JWT

### JWT Settings (appsettings.Development.json)

```json
{
  "JwtSettings": {
    "SecretKey": "LexiQuest-Dev-Secret-Key-That-Is-Long-Enough-For-HS256-Algorithm-!!",
    "Issuer": "LexiQuest",
    "Audience": "LexiQuestClient",
    "AccessTokenExpiryMinutes": 30,
    "RefreshTokenExpiryDays": 7
  }
}
```

**IMPORTANT:** In production, `SecretKey` must be stored in environment variables or secrets manager, NOT in appsettings.json.

---

## Testing Strategy

### Test Projects

| Project | Type | Framework |
|---------|------|-----------|
| LexiQuest.Core.Tests | Unit | xUnit + FluentAssertions |
| LexiQuest.Infrastructure.Tests | Unit/Integration | xUnit + EF InMemory |
| LexiQuest.Api.Tests | Integration | xUnit + WebApplicationFactory |
| LexiQuest.Blazor.Tests | Component | xUnit + bUnit |

### Mocking

Use **NSubstitute** for mocking:

```csharp
var mockService = Substitute.For<IMyService>();
mockService.GetData().Returns(new Data { ... });
```

### Test Pattern

```csharp
public class UserTests
{
    [Fact]
    public void Create_SetsDefaultValues()
    {
        // Arrange
        var email = "test@example.com";
        var username = "testuser";
        
        // Act
        var user = User.Create(email, username);
        
        // Assert
        user.Email.Should().Be(email);
        user.Stats.Level.Should().Be(1);
    }
}
```

---

## Domain Model Overview

### Core Entities

- **User** - Player account with stats, preferences, streak, premium status
- **Word** - Word dictionary entry with difficulty, category, scramble method
- **GameSession** - Active or completed game with rounds
- **GameRound** - Individual round within a session
- **RefreshToken** - JWT refresh token storage

### Value Objects

- **UserStats** - XP, level, accuracy, words solved
- **UserPreferences** - Theme, language, sound/animation settings
- **Streak** - Current and longest daily streak
- **PremiumStatus** - Premium subscription info

### Enums

- `GameMode` - Classic, TimeAttack, Marathon, etc.
- `DifficultyLevel` - Easy, Medium, Hard, Expert
- `GameSessionStatus` - InProgress, Completed, Abandoned
- `WordCategory` - Animals, Food, Science, etc.

---

## API Endpoints (Planned)

### Auth
```
POST /api/v1/users/register
POST /api/v1/users/login
POST /api/v1/users/refresh
POST /api/v1/users/logout
GET  /api/v1/users/me
```

### Game
```
POST /api/v1/game/start
POST /api/v1/game/{id}/answer
GET  /api/v1/game/{id}/hint
POST /api/v1/game/{id}/forfeit
GET  /api/v1/game/daily
```

### Guest (No Auth Required)
```
POST /api/v1/game/guest/start
POST /api/v1/game/guest/answer
GET  /api/v1/guest/status
```

---

## Caching

Uses **In-Memory Caching** (IMemoryCache) instead of Redis:

```csharp
// Registration
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Usage
public class SomeService
{
    private readonly IMemoryCache _cache;
    
    public async Task<Data> GetDataAsync()
    {
        if (_cache.TryGetValue("key", out Data cached))
            return cached;
            
        var data = await FetchDataAsync();
        _cache.Set("key", data, TimeSpan.FromMinutes(10));
        return data;
    }
}
```

---

## Background Jobs

Uses **Hangfire** with MSSQL storage:

```csharp
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();
```

Common jobs:
- Weekly league reset
- Streak expiration check
- Daily challenge generation

---

## Project Status

### Implemented (Phase 0 Complete)

- ✅ Project structure and solution
- ✅ Database infrastructure (EF Core + MSSQL)
- ✅ JWT authentication
- ✅ Localization setup
- ✅ In-memory caching
- ✅ Tempo.Blazor integration
- ✅ Unit of Work pattern
- ✅ Resource files structure

### In Progress (Phase 1)

- ✅ User registration (TDD) - Backend hotov (23 testů), Frontend hotov (10 testů)
- ✅ User login (TDD) - Backend hotov (9 testů), Frontend hotov (15 testů), HTTP Interceptor (5 testů)
- 🔄 Basic game loop - další priorita
- XP/Level system
- Lives system
- Streak system

See `/todo/` directory for detailed implementation phases (Faze-0-Setup.md through Faze-7-Testing-Deployment.md).

---

## Security Considerations

1. **JWT Secret:** Store in environment variables in production
2. **HTTPS:** Always use HTTPS in production
3. **CORS:** Configured for Blazor client origin only
4. **Input Validation:** All inputs validated with FluentValidation
5. **SQL Injection:** Protected by EF Core parameterized queries
6. **XSS:** Blazor handles most XSS protection automatically

---

## Troubleshooting

### Common Issues

**Build fails with missing Tempo.Blazor packages:**
```bash
# Ensure NuGet source is configured for GitHub Packages
dotnet nuget add source https://nuget.pkg.github.com/USERNAME/index.json \
  --name GitHubPackages \
  --username USERNAME \
  --password YOUR_GITHUB_TOKEN
```

**Database connection fails:**
- Ensure MSSQL LocalDB is installed: `sqllocaldb info`
- Or update connection string to use SQL Server instance

**EF Core migrations not found:**
```bash
# Run from LexiQuest.Api directory with proper project paths
dotnet ef migrations add InitialCreate \
  --project ../LexiQuest.Infrastructure/LexiQuest.Infrastructure.csproj \
  --startup-project .
```

---

## Documentation

- `/docs/architecture/Architecture.md` - Complete architecture documentation (Czech)
- `/docs/uc/` - Use case specifications (UC-001 through UC-027)
- `/docs/ui-ux/` - UI/UX design documents
- `/docs/resources/` - Resource structure documentation
- `/todo/` - Implementation phases and task lists

---

## License

MIT License - See LICENSE file

---

*Last Updated: March 2026*
