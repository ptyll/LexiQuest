# LexiQuest - Vyvojarska dokumentace

> Pruvodce pro vyvojare pracujici na projektu LexiQuest.

## Prehled projektu

**LexiQuest** je webova hra pro uceni slovicek. Uzivatele se uci nova slova formou interaktivnich hernich kol, sbiranim XP, postupem v ligach a soutezenich s ostatnimi hraci v realnem case. Aplikace podporuje premium predplatne, vlastni slovniky, boss levely, tymy a dalsich pokrocile funkce.

---

## Technologicky stack

| Kategorie | Technologie |
|---|---|
| **Backend** | .NET 10, ASP.NET Core Web API |
| **Frontend** | Blazor WebAssembly |
| **Databaze** | SQL Server (EF Core) |
| **Realtime komunikace** | SignalR (WebSockets) |
| **Platby** | Stripe (predplatne, jednorazy) |
| **UI komponenty** | Tempo.Blazor |
| **Validace** | FluentValidation |
| **Logování** | Serilog (Console + File sink) |
| **Autentizace** | JWT Bearer tokens |
| **Lokalizace** | .resx resource soubory |
| **Testy** | xUnit, bUnit, FluentAssertions, NSubstitute |
| **Cache** | In-Memory cache (MemoryCache) |

---

## Predpoklady

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express nebo plna verze)
- [Node.js 18+](https://nodejs.org/) (pouze pro Playwright E2E testy)
- IDE: Visual Studio 2025, VS Code nebo Rider

---

## Lokalni vyvoj - Prvni spusteni

### 1. Klonovani repozitare

```bash
git clone https://github.com/your-org/lexiquest.git
cd lexiquest
```

### 2. Obnoveni zavislosti

```bash
dotnet restore
```

### 3. Konfigurace

Upravte `src/LexiQuest.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LexiQuest;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "VaseTajneHesloProVyvojMinimalne64ZnakuDlouheAbySplnovaloPozadavkyHS256"
  }
}
```

> **Poznamka:** Soubor `appsettings.Development.json` je v `.gitignore` a musi byt vytvoren lokalne.

### 4. Spusteni databazovych migraci

```bash
dotnet ef database update \
  --project src/LexiQuest.Infrastructure \
  --startup-project src/LexiQuest.Api
```

### 5. Spusteni API

```bash
dotnet run --project src/LexiQuest.Api
```

API bude dostupne na `https://localhost:5000`. Swagger UI: `https://localhost:5000/swagger`

### 6. Spusteni Blazor frontendu

V novem terminalu:

```bash
dotnet run --project src/LexiQuest.Blazor
```

Frontend bude dostupny na `https://localhost:5001`.

---

## Spousteni testu

### Vsechny testy

```bash
dotnet test
```

### Pouze unit testy

```bash
dotnet test --filter "Category!=Integration"
```

### Pouze integracni testy

```bash
dotnet test --filter "Category=Integration"
```

### Konkretni testovy projekt

```bash
dotnet test tests/LexiQuest.Core.Tests
dotnet test tests/LexiQuest.Api.Tests
dotnet test tests/LexiQuest.Blazor.Tests
dotnet test tests/LexiQuest.Infrastructure.Tests
```

### Testy s detailnim vystupem

```bash
dotnet test --verbosity normal --logger "console;verbosity=detailed"
```

---

## Struktura projektu

```
LexiQuest/
├── src/
│   ├── LexiQuest.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/            # REST API controllery
│   │   ├── Endpoints/              # Minimal API endpointy
│   │   ├── Hubs/                   # SignalR huby (MatchHub)
│   │   ├── Validators/             # Request validatory
│   │   └── Resources/              # Lokalizacni soubory API
│   │
│   ├── LexiQuest.Blazor/           # Blazor WebAssembly frontend
│   │   ├── Pages/                  # Stranky (routovatelne komponenty)
│   │   ├── Components/             # Znovupouzitelne komponenty
│   │   ├── Layout/                 # Layout komponenty
│   │   ├── Services/               # Frontend sluzby (HTTP klienti)
│   │   ├── Resources/              # Lokalizacni soubory
│   │   └── wwwroot/                # Staticke soubory (CSS, JS, images)
│   │
│   ├── LexiQuest.Core/             # Domenova vrstva (jadro aplikace)
│   │   ├── Domain/
│   │   │   ├── Entities/           # Domenove entity
│   │   │   ├── Enums/              # Vycty
│   │   │   └── ValueObjects/       # Value objekty
│   │   ├── Interfaces/
│   │   │   ├── Repositories/       # Repository rozhrani
│   │   │   └── Services/           # Service rozhrani
│   │   ├── Services/               # Domenove sluzby (implementace)
│   │   ├── Validators/             # Domenove validatory
│   │   └── Jobs/                   # Background joby
│   │
│   ├── LexiQuest.Infrastructure/   # Infrastrukturni vrstva
│   │   ├── Persistence/
│   │   │   ├── Configurations/     # EF Core entity konfigurace
│   │   │   └── Repositories/       # Repository implementace
│   │   ├── Auth/                   # JWT token service
│   │   ├── Services/               # Externi sluzby (Stripe, Email)
│   │   └── Caching/                # Cache implementace
│   │
│   └── LexiQuest.Shared/           # Sdilene typy
│       ├── DTOs/                   # Data Transfer Objects
│       ├── Enums/                  # Sdilene vycty
│       └── Resources/              # Sdilene lokalizacni soubory
│
├── tests/
│   ├── LexiQuest.Core.Tests/       # Unit testy domeny
│   ├── LexiQuest.Api.Tests/        # API integracni testy
│   ├── LexiQuest.Blazor.Tests/     # bUnit testy komponent
│   └── LexiQuest.Infrastructure.Tests/ # Infrastrukturni testy
│
├── docs/                           # Dokumentace
│   ├── uc/                         # Use case specifikace
│   ├── ui-ux/                      # UI/UX specifikace
│   ├── architecture/               # Architektura
│   └── deployment/                 # Nasazeni a troubleshooting
│
└── todo/                           # Implementacni plan a sledovani pokroku
```

---

## Architektura a konvence

### Clean Architecture

Projekt sleduje principy Clean Architecture:

```
Core (bez zavislosti)
  ↑
Infrastructure (zavisi na Core)
  ↑
Api / Blazor (zavisi na Infrastructure a Core)
```

- **Core** nesmi referencovat zadny jiny projekt
- **Infrastructure** referencuje pouze Core
- **Api** referencuje Core, Infrastructure a Shared
- **Blazor** referencuje pouze Shared
- **Shared** je bez zavislosti (DTOs, enums)

### DDD entity pattern

Vsechny domenove entity pouzivaji nasledujici vzor:

```csharp
public class Entity
{
    // Privatni konstruktor - EF Core
    private Entity() { }

    // Staticka tovarni metoda
    public static Entity Create(string param1, int param2)
    {
        // Validace vstupu
        // Inicializace properties
        return new Entity { ... };
    }

    // Privatni settery
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    // Domenove metody pro zmenu stavu
    public void UpdateName(string newName) { ... }
}
```

### FluentValidation

Vsechny vstupy od uzivatele musi byt validovany pomoci FluentValidation:

```csharp
public class CreateEntityValidator : AbstractValidator<CreateEntityRequest>
{
    public CreateEntityValidator(IStringLocalizer<CreateEntityValidator> localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["NameRequired"])
            .MaximumLength(100).WithMessage(localizer["NameTooLong"]);
    }
}
```

### Lokalizace

- **Zadne hardcoded retezce** v UI ani v chybovych hlasenich
- Vsechny texty v `.resx` souborech
- Struktura: `Resources/Pages/NazevStranky.resx`, `Resources/Components/NazevKomponenty.resx`
- Pouziti v komponentach:
  ```razor
  @inject IStringLocalizer<NazevKomponenty> L
  <h1>@L["Nadpis"]</h1>
  ```

### Scoped CSS

Kazda Blazor komponenta s vlastnimi styly pouziva scoped CSS:

```
Components/
  MyComponent.razor
  MyComponent.razor.css    # Scoped styly
```

### TDD pristup

Doporuceny pracovni postup:

1. Napsat test (cerveny)
2. Napsat minimalni implementaci (zeleny)
3. Refaktorovat
4. Opakovat

---

## API endpointy

### REST controllery

| Controller | Zakladni cesta | Popis |
|---|---|---|
| `UsersController` | `/api/users` | Registrace, prihlaseni, profil |
| `DictionaryController` | `/api/dictionaries` | Vlastni slovniky |
| `AchievementsController` | `/api/achievements` | Achievementy |
| `DailyChallengeController` | `/api/daily-challenge` | Denni vyzvy |
| `LeaguesController` | `/api/leagues` | Ligy a zebrickky |
| `MultiplayerController` | `/api/multiplayer` | Multiplayer funkce |
| `PremiumController` | `/api/premium` | Premium predplatne |
| `ShopController` | `/api/shop` | Obchod s predmety |
| `StreakProtectionController` | `/api/streak-protection` | Streak shield/freeze |
| `TeamsController` | `/api/teams` | Tymy a klany |
| `WebhookController` | `/api/webhook` | Stripe webhooky |
| `NotificationsController` | `/api/notifications` | Push notifikace |
| `AdminWordsController` | `/api/admin/words` | Administrace slov |
| `AdminUsersController` | `/api/admin/users` | Administrace uzivatelu |
| `AIChallengeController` | `/api/ai-challenge` | AI generovane vyzvy |

### Minimal API endpointy

| Soubor | Cesta | Popis |
|---|---|---|
| `GameEndpoints` | `/api/game/*` | Herni smycka |
| `UserEndpoints` | `/api/users/*` | Doplnkove user endpointy |
| `GuestEndpoints` | `/api/guest/*` | Testovaci hra bez registrace |

### SignalR huby

| Hub | Cesta | Popis |
|---|---|---|
| `MatchHub` | `/hubs/match` | Realtime multiplayer |

### Swagger UI

Swagger je dostupny v Development rezimu na: `https://localhost:5000/swagger`

---

## Git workflow

### Pojmenovani vetvi

- `feature/nazev-funkce` - nova funkcionalita
- `fix/popis-opravy` - oprava chyby
- `docs/popis` - dokumentace
- `refactor/popis` - refaktoring bez zmeny funkcionality
- `test/popis` - doplneni testu

### Pull Request

Kazdy PR musi obsahovat:

1. **Popis** - co se meni a proc
2. **Test plan** - jak otestovat zmeny
3. **Vsechny testy musi prochazet** - CI pipeline bezi automaticky

### CI pipeline

Na kazdem PR se automaticky spusti:
- `dotnet restore`
- `dotnet build --no-restore`
- `dotnet test --no-build`

---

## Uzitecne prikazy

```bash
# Build celeho solution
dotnet build

# Spusteni API s hot reload
dotnet watch --project src/LexiQuest.Api

# Pridani nove EF Core migrace
dotnet ef migrations add NazevMigrace \
  --project src/LexiQuest.Infrastructure \
  --startup-project src/LexiQuest.Api

# Smazani posledni migrace (pokud neni aplikovana)
dotnet ef migrations remove \
  --project src/LexiQuest.Infrastructure \
  --startup-project src/LexiQuest.Api

# Generovani SQL scriptu
dotnet ef migrations script \
  --project src/LexiQuest.Infrastructure \
  --startup-project src/LexiQuest.Api \
  --idempotent

# Cisteni build artefaktu
dotnet clean

# Aktualizace NuGet balicku
dotnet outdated  # vyzaduje dotnet-outdated tool
```

---

## Dalsi dokumentace

- [Architektura](architecture/Architecture.md)
- [Use Case specifikace](uc/)
- [UI/UX specifikace](ui-ux/)
- [Deployment Guide](deployment/DeploymentGuide.md)
- [Troubleshooting Guide](deployment/TroubleshootingGuide.md)
- [Resource struktura](resources/ResourceStructure.md)
