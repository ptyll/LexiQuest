# UC-002: Přihlášení uživatele

## Popis
Umožňuje existujícímu uživateli přihlásit se do aplikace pomocí emailu/hesla nebo OAuth providerů.

## Aktéři
- **Primary Actor:** Registrovaný uživatel
- **Secondary Actor:** Systém, OAuth provider

## Předpoklady
- Uživatel má vytvořený účet
- Účet není zablokován/suspendován

## Post-conditions
**Úspěch:**
- Uživatel je přihlášen (JWT token vydán)
- Aktualizován LastLoginAt
- Zobrazen dashboard s aktuálním stavem

**Neúspěch:**
- Přihlášení zamítnuto s chybovou hláškou
- Případně zvýšen failed attempts counter

## Hlavní tok

| Krok | Akce | Data | Validation |
|------|------|------|------------|
| 1 | Uživatel otevře přihlašovací stránku | - | - |
| 2 | Systém zobrazí přihlašovací formulář | - | - |
| 3 | Uživatel zadá email | email: string | NotEmpty, EmailAddress |
| 4 | Uživatel zadá heslo | password: string | NotEmpty |
| 5 | Uživatel klikne "Přihlásit se" | - | - |
| 6 | FE validuje vstupy | - | FluentValidation |
| 7 | FE odešle POST /api/v1/users/login | LoginRequest | - |
| 8 | BE validuje vstupy | - | FluentValidation |
| 9 | BE vyhledá uživatele podle emailu | - | NotFound → error |
| 10 | BE ověří hash hesla | - | Invalid → error |
| 11 | BE kontroluje zda účet není locked | - | Locked → error |
| 12 | BE vygeneruje JWT + Refresh token | - | - |
| 13 | BE aktualizuje LastLoginAt | - | - |
| 14 | BE resetuje FailedAttempts | - | - |
| 15 | BE vrátí AuthResponse | - | - |
| 16 | FE uloží tokeny | - | Secure storage |
| 17 | FE přesměruje na Dashboard | - | - |
| 18 | Zobrazí se notifikace "Vítej zpět! Streak: X dní" | - | - |

## Alternativní toky

### A1: Přihlášení přes Google
- Po kroku 2 uživatel klikne "Přihlásit přes Google"
- OAuth flow → po úspěchu vyhledání uživatele podle GoogleId nebo emailu
- Pokud email neexistuje → nabídka registrace

### A2: Nesprávné přihlašovací údaje
- Krok 9 nebo 10 selže
- BE vrátí generickou chybu (neprozrazovat zda email existuje)
- FE zobrazí: "Nesprávný email nebo heslo"
- Increment FailedAttempts

### A3: Účet je uzamčen
- Krok 11 detekuje FailedAttempts >= 5
- BE vrátí chybu "Account.Locked" s časem odemčení
- FE zobrazí: "Účet je dočasně zablokován. Zkus to za X minut."

### A4: Neaktivní streak reminder
- Po kroku 17, pokud streak končí do 6 hodin:
- Zobrazí se speciální modal: "⚠️ Tvůj streak končí za 5 hodin!"

## DTOs

```csharp
public record LoginRequest(string Email, string Password, bool RememberMe);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User,
    StreakWarningDto? StreakWarning
);

public record StreakWarningDto(
    int HoursRemaining,
    bool IsCritical // < 6 hodin
);
```

## Validátory

```csharp
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IStringLocalizer<Resources> L)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithLocalizedMessage("Validation.Email.Required")
            .EmailAddress().WithLocalizedMessage("Validation.Email.Invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithLocalizedMessage("Validation.Password.Required");
    }
}
```

## Resource klíče

```
Login.Title
Login.Subtitle
Login.Input.Email.Label
Login.Input.Email.Placeholder
Login.Input.Password.Label
Login.Input.Password.Placeholder
Login.Input.RememberMe.Label
Login.Button.Submit
Login.Button.Google
Login.Link.ForgotPassword
Login.Link.Register
Login.Link.Register.Text
Login.Error.InvalidCredentials
Login.Error.AccountLocked
Login.Error.AccountLocked.Time
Login.Warning.StreakEnding
Login.Warning.StreakEnding.Action
```

## Odhad
| Část | Hodiny |
|------|--------|
| Backend | 3h |
| Frontend | 4h |
| Testy | 3h |
| **Celkem** | **10h** |
