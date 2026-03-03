# LexiQuest - Architektonický dokument

## Přehled technologií

| Vrstva | Technologie | Verze |
|--------|-------------|-------|
| Backend API | .NET | 10 |
| Frontend | Blazor | .NET 10 |
| Databáze | **Microsoft SQL Server** | 2022+ |
| ~~Cache~~ | ~~Redis~~ | ~~7+~~ |
| Message Queue | **In-Memory / Hangfire** | - |
| Real-time | SignalR | .NET 10 |
| Validace | FluentValidation | 11+ |
| Testing | xUnit, NSubstitute, FluentAssertions | - |

## Vrstvená architektura

```
┌─────────────────────────────────────────────────────────────┐
│                    LexiQuest.Blazor                          │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────────────┐ │
│  │   Pages      │ │  Components  │ │   Shared/Layout      │ │
│  └──────────────┘ └──────────────┘ └──────────────────────┘ │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Services (HttpClient)                      │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    LexiQuest.Api                             │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────────────┐ │
│  │  Controllers │ │  Endpoints   │ │  Middleware          │ │
│  └──────────────┘ └──────────────┘ └──────────────────────┘ │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              Application Services                       │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   LexiQuest.Core                             │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────────────┐ │
│  │    Domain    │ │   Services   │ │   Specifications     │ │
│  │    Models    │ │   (Logic)    │ │                      │ │
│  └──────────────┘ └──────────────┘ └──────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                LexiQuest.Infrastructure                      │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────────────┐ │
│  │     EF Core  │ │ ~~Redis~~    │ │  Identity/JWT        │ │
│  │ Repositories │ │ ~~Cache~~    │ │  External APIs       │ │
│  └──────────────┘ └──────────────┘ └──────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Poznámky k infrastruktuře (bez Dockeru)

- **Databáze:** MSSQL LocalDB (vývoj) / MSSQL Server (produkce)
- **Cache:** In-Memory caching (IMemoryCache) místo Redis
- **Background jobs:** Hangfire s MSSQL storage místo Redis
- **File storage:** Lokální filesystem (vývoj) / Azure Blob (produkce)

## Domain Model

### Core Entities

```csharp
// User Aggregate
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; }
    public UserStats Stats { get; private set; }
    public UserPreferences Preferences { get; private set; }
    public Streak Streak { get; private set; }
    public PremiumStatus Premium { get; private set; }
    public ICollection<UserAchievement> Achievements { get; private set; }
    public ICollection<CompletedLevel> CompletedLevels { get; private set; }
}

// Word Aggregate
public class Word
{
    public Guid Id { get; private set; }
    public string Original { get; private set; }
    public string Normalized { get; private set; }
    public int Length { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public int FrequencyRank { get; private set; }
    public WordCategory Category { get; private set; }
    public List<string> AlternativeForms { get; private set; }
    
    public string Scramble(Random rng)
    {
        // Fisher-Yates shuffle ensuring result != original
    }
}

// Game Session Aggregate
public class GameSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public GameMode Mode { get; private set; }
    public Guid? PathId { get; private set; }
    public int CurrentLevel { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public int LivesRemaining { get; private set; }
    public int TotalXP { get; private set; }
    public List<GameRound> Rounds { get; private set; }
    public GameSessionStatus Status { get; private set; }
}

public class GameRound
{
    public Guid Id { get; private set; }
    public Guid WordId { get; private set; }
    public string Scrambled { get; private set; }
    public List<char> ForbiddenLetters { get; private set; } // pro Boss level
    public int RevealedLettersCount { get; private set; } // pro Twist mód
    public DateTime? StartedAt { get; private set; }
    public DateTime? AnsweredAt { get; private set; }
    public string UserAnswer { get; private set; }
    public bool IsCorrect { get; private set; }
    public int XPEarned { get; private set; }
}

// League Aggregate
public class League
{
    public Guid Id { get; private set; }
    public LeagueTier Tier { get; private set; }
    public DateTime WeekStart { get; private set; }
    public List<LeagueParticipant> Participants { get; private set; }
    public bool IsActive { get; private set; }
}

public class LeagueParticipant
{
    public Guid UserId { get; private set; }
    public int WeeklyXP { get; private set; }
    public int Rank { get; private set; }
    public bool IsPromoted { get; private set; }
    public bool IsDemoted { get; private set; }
}
```

## Fluent Validation Rules

### Backend Validators

```csharp
public class GameAnswerValidator : AbstractValidator<SubmitAnswerRequest>
{
    public GameAnswerValidator()
    {
        RuleFor(x => x.Answer)
            .NotEmpty()
            .WithMessageLocalization("Validation.Answer.Empty")
            .MaximumLength(50)
            .WithMessageLocalization("Validation.Answer.TooLong");
            
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessageLocalization("Validation.Session.Required");
            
        RuleFor(x => x.TimeSpentMs)
            .GreaterThanOrEqualTo(0)
            .WithMessageLocalization("Validation.Time.Invalid");
    }
}

public class UserRegistrationValidator : AbstractValidator<RegisterRequest>
{
    public UserRegistrationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessageLocalization("Validation.Email.Invalid");
            
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(30)
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessageLocalization("Validation.Username.Invalid");
            
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessageLocalization("Validation.Password.Uppercase")
            .Matches("[a-z]").WithMessageLocalization("Validation.Password.Lowercase")
            .Matches("[0-9]").WithMessageLocalization("Validation.Password.Digit")
            .Matches("[^a-zA-Z0-9]").WithMessageLocalization("Validation.Password.Special");
            
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessageLocalization("Validation.Password.Mismatch");
    }
}
```

### Frontend Validators

```csharp
public class GameAnswerFluentValidator : FluentValidator<AnswerModel>
{
    public GameAnswerFluentValidator(IStringLocalizer<Resources> localizer) : base(localizer)
    {
        RuleFor(x => x.Answer)
            .NotEmpty()
            .WithLocalizedMessage("Validation.Answer.Empty")
            .Must(BeSingleWord)
            .WithLocalizedMessage("Validation.Answer.SingleWordOnly");
    }
    
    private bool BeSingleWord(string answer) => 
        !string.IsNullOrWhiteSpace(answer) && !answer.Contains(' ');
}
```

## Resource Structure

```
LexiQuest.Blazor/
├── Resources/
│   ├── Components/
│   │   ├── GameArena.resx
│   │   ├── StreakIndicator.resx
│   │   └── Leaderboard.resx
│   ├── Pages/
│   │   ├── Index.resx
│   │   ├── Login.resx
│   │   ├── Register.resx
│   │   ├── Dashboard.resx
│   │   ├── Game.resx
│   │   ├── Paths.resx
│   │   ├── BossLevel.resx
│   │   ├── Leagues.resx
│   │   ├── Statistics.resx
│   │   ├── Achievements.resx
│   │   ├── Profile.resx
│   │   ├── Premium.resx
│   │   ├── Shop.resx
│   │   ├── Multiplayer.resx
│   │   └── Settings.resx
│   ├── Validation/
│   │   └── ValidationMessages.resx
│   └── Shared/
│       ├── Navigation.resx
│       ├── Footer.resx
│       └── Notifications.resx
LexiQuest.Api/
├── Resources/
│   ├── Validation/
│   │   └── ValidationMessages.resx
│   └── Email/
│       ├── WelcomeEmail.resx
│       └── PasswordReset.resx
```

## API Endpoints

### Game Controller
```
POST   /api/v1/game/start           - Start new game session
POST   /api/v1/game/{id}/answer     - Submit answer
GET    /api/v1/game/{id}/hint       - Request hint (costs XP)
POST   /api/v1/game/{id}/forfeit    - Forfeit current round
GET    /api/v1/game/daily           - Get daily challenge

// Guest endpoints (bez auth)
POST   /api/v1/game/guest/start     - Start guest game
POST   /api/v1/game/guest/answer    - Guest answer
```

### User Controller
```
POST   /api/v1/users/register
POST   /api/v1/users/login
POST   /api/v1/users/refresh
POST   /api/v1/users/logout
GET    /api/v1/users/me
PUT    /api/v1/users/me
PUT    /api/v1/users/me/password
```

### Statistics Controller
```
GET    /api/v1/stats/dashboard      - User dashboard stats
GET    /api/v1/stats/activity       - Activity heatmap data
GET    /api/v1/stats/leaderboard    - Global/weekly leaderboard
```

### League Controller
```
GET    /api/v1/leagues/current      - Current league info
GET    /api/v1/leagues/history      - Past leagues
```

### Guest Controller
```
GET    /api/v1/guest/status         - Zbývající hry pro guesta
POST   /api/v1/guest/convert        - Převést guest progress na registraci
```

## SignalR Hubs

```csharp
public interface IGameHub
{
    // Real-time updates
    Task JoinLeague(string leagueId);
    Task LeaveLeague(string leagueId);
    Task UpdateProgress(int xp);
    
    // Multiplayer
    Task JoinMatchmaking();
    Task CancelMatchmaking();
    Task SubmitAnswerRealtime(string answer);
}

public interface IGameClient
{
    Task LeagueUpdate(LeagueUpdate update);
    Task MatchFound(MatchInfo match);
    Task OpponentAnswered(OpponentProgress progress);
    Task GameEnded(GameResult result);
    Task StreakWarning(int hoursRemaining);
}
```

## Test Strategy (TDD)

```
Test Naming Convention:
[UnitOfWork]_[Scenario]_[ExpectedResult]

Example:
- SubmitAnswer_CorrectAnswer_IncreasesXP
- SubmitAnswer_WrongAnswer_DecreasesLife
- StartGame_ExpertMode_AppliesTimeLimit

Test Structure:
├── Unit Tests (Core logic)
│   ├── Domain.Tests (Entities, Value Objects)
│   ├── Application.Tests (Services, Validators)
│   └── Infrastructure.Tests (Repositories)
├── Integration Tests (API)
│   └── Controllers, Endpoints
└── E2E Tests (Blazor)
    └── Playwright tests
```

## MSSQL Connection (vývoj bez Dockeru)

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LexiQuest;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=LexiQuest;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}
```

## EF Core MSSQL Specific

```csharp
// Program.cs
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

// Hangfire s MSSQL
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

## In-Memory Caching (místo Redis)

```csharp
// Program.cs
builder.Services.AddMemoryCache();

// Usage
public class SomeService
{
    private readonly IMemoryCache _cache;
    
    public SomeService(IMemoryCache cache)
    {
        _cache = cache;
    }
    
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
