# UC-024: Admin - Správa slov

## Popis
Administrativní rozhraní pro správu slovníku, moderaci a analýzy.

## Role

| Role | Oprávnění |
|------|-----------|
| Admin | Vše |
| Moderator | Slovník, uživatelé (read) |
| Content Manager | Pouze slovník |

## Funkce

### 1. Správa slov
- Přidat nové slovo
- Upravit existující
- Smazat slovo
- Hromadný import (CSV)
- Export slovníku
- Hledání a filtrování

### 2. Statistiky slovníku
- Počet slov celkem
- Rozdělení podle obtížnosti
- Nejčastěji používaná slova
- Slova s nejvyšší úspěšností
- Slova s nejnižší úspěšností

### 3. Denní výzva management
- Naplánovat výzvy dopředu
- Nastavit modifikátory
- Zobrazit historii

### 4. Uživatelé
- Seznam uživatelů
- Vyhledávání
- Detaily uživatele
- Suspendace/Blokace
- Reset hesla

### 5. Systémové
- Health check
- Logy
- Performance metriky

## DTOs

```csharp
public record AdminWordListRequest(
    string? Search,
    DifficultyLevel? Difficulty,
    int? MinLength,
    int? MaxLength,
    int Page = 1,
    int PageSize = 50
);

public record AdminWordDto(
    Guid Id,
    string Original,
    int Length,
    DifficultyLevel Difficulty,
    int FrequencyRank,
    int TimesUsed,
    double SuccessRate,
    DateTime CreatedAt
);

public record BulkImportResult(
    int TotalProcessed,
    int SuccessCount,
    int ErrorCount,
    List<string> Errors
);

public record AdminUserDto(
    Guid Id,
    string Email,
    string Username,
    DateTime CreatedAt,
    DateTime LastLoginAt,
    bool IsPremium,
    bool IsSuspended,
    int TotalXP,
    int CurrentStreak
);
```

## Resource klíče

```
Admin.Title
Admin.Words.Title
Admin.Words.Add
Admin.Words.Edit
Admin.Words.Delete
Admin.Words.Import
Admin.Words.Export
Admin.Words.Search
Admin.Words.Filter.Difficulty
Admin.Users.Title
Admin.Users.Search
Admin.Users.Suspend
Admin.Users.Unsuspend
Admin.Users.ResetPassword
Admin.Daily.Title
Admin.Daily.Schedule
Admin.Stats.Title
Admin.Stats.TotalWords
Admin.Stats.TotalUsers
Admin.Stats.ActiveToday
```

## Odhad: 14h
