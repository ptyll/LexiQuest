# UC-001: Registrace uživatele

## Popis
Umožňuje novému uživateli vytvořit účet v aplikaci LexiQuest pomocí emailu a hesla nebo přes OAuth (Google).

## Aktéři
- **Primary Actor:** Nový uživatel (Host)
- **Secondary Actor:** Systém, Email service

## Předpoklady
- Uživatel není přihlášen
- Email service je dostupný

## Post-conditions
**Úspěch:**
- Uživatel má vytvořený účet
- Uživatel je automaticky přihlášen
- Odeslán uvítací email
- Vytvořen záznam v tabulce Users s výchozími hodnotami

**Neúspěch:**
- Účet není vytvořen
- Zobrazena chybová hláška s důvodem

## Hlavní tok (Happy Path)

| Krok | Akce | Data | Validation |
|------|------|------|------------|
| 1 | Uživatel otevře stránku registrace | - | - |
| 2 | Systém zobrazí registrační formulář | - | - |
| 3 | Uživatel zadá email | email: string | FluentValidation: NotEmpty, EmailAddress |
| 4 | Uživatel zadá uživatelské jméno | username: string | FluentValidation: NotEmpty, MinLength(3), MaxLength(30), Regex(\"^[a-zA-Z0-9_]+$\") |
| 5 | Uživatel zadá heslo | password: string | FluentValidation: NotEmpty, MinLength(8), Must contain uppercase, lowercase, digit, special char |
| 6 | Uživatel potvrdí heslo | confirmPassword: string | FluentValidation: Equal(Password) |
| 7 | Uživatel zaškrtne souhlas s podmínkami | termsAccepted: bool | Must be true |
| 8 | Uživatel klikne na "Registrovat" | - | - |
| 9 | FE validuje vstupy FluentValidation | - | - |
| 10 | FE odešle POST /api/v1/users/register | RegisterRequest | - |
| 11 | BE validuje vstupy FluentValidation | - | - |
| 12 | BE kontroluje unikátnost emailu | - | Error pokud existuje |
| 13 | BE kontroluje unikátnost username | - | Error pokud existuje |
| 14 | BE hashuje heslo (bcrypt/Argon2) | - | - |
| 15 | BE vytvoří záznam User v databázi | - | - |
| 16 | BE vytvoří výchozí UserStats (0 XP, Bronze league) | - | - |
| 17 | BE vytvoří výchozí Streak (0 dní) | - | - |
| 18 | BE vygeneruje JWT token | - | - |
| 19 | BE odešle uvítací email | - | async background |
| 20 | BE vrátí 200 OK s tokenem a uživatelskými daty | AuthResponse | - |
| 21 | FE uloží token do localStorage/sessionStorage | - | - |
| 22 | FE přesměruje na Dashboard | - | - |
| 23 | Zobrazí se toast notifikace "Vítej v LexiQuest!" | - | - |

## Alternativní toky

### A1: Registrace přes Google OAuth
- Po kroku 2 uživatel klikne "Registrovat přes Google"
- Otevře se Google OAuth popup
- Po úspěšném OAuth:
  - Pokud email neexistuje → vytvořit účet (náhodné username z emailu)
  - Pokud email existuje → přihlásit existujícího uživatele

### A2: Email již existuje
- Krok 12 selže
- BE vrátí 400 BadRequest s kódem "Email.AlreadyExists"
- FE zobrazí chybu pod email inputem: "Tento email je již registrován"
- Nabídne link "Přihlásit se"

### A3: Username již existuje
- Krok 13 selže
- BE vrátí 400 BadRequest s kódem "Username.AlreadyExists"
- FE zobrazí chybu a nabídne alternativní návrhy username

### A4: Slabé heslo (FE validace)
- Krok 9 selže
- FE zobrazí real-time indikátor síly hesla
- Chybová hláška specifikuje co chybí: "Heslo musí obsahovat velké písmeno"

## Business pravidla

| ID | Pravidlo | Priorita |
|----|----------|----------|
| BR-001 | Email musí být unikátní | Critical |
| BR-002 | Username musí být unikátní | Critical |
| BR-003 | Heslo min. 8 znaků, obsahovat: velké, malé, číslo, speciál | Critical |
| BR-004 | Nový uživatel startuje v Bronze lize | High |
| BR-005 | Nový uživatel má 0 XP a 0 dní streak | High |
| BR-006 | Souhlas s podmínkami je povinný | Critical |

## Data Transfer Objects

```csharp
// Request
public record RegisterRequest(
    string Email,
    string Username, 
    string Password,
    string ConfirmPassword,
    bool TermsAccepted
);

// Response
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Email,
    string Username,
    int CurrentStreak,
    int TotalXp,
    string League
);
```

## Validátory

### Backend (FluentValidation)
```csharp
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IUserRepository userRepo, IStringLocalizer<Resources> L)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithLocalizedMessage("Validation.Email.Required")
            .EmailAddress().WithLocalizedMessage("Validation.Email.Invalid")
            .MustAsync(async (email, ct) => !await userRepo.ExistsByEmailAsync(email, ct))
            .WithLocalizedMessage("Validation.Email.AlreadyExists");

        RuleFor(x => x.Username)
            .NotEmpty().WithLocalizedMessage("Validation.Username.Required")
            .MinimumLength(3).WithLocalizedMessage("Validation.Username.MinLength")
            .MaximumLength(30).WithLocalizedMessage("Validation.Username.MaxLength")
            .Matches("^[a-zA-Z0-9_]+$").WithLocalizedMessage("Validation.Username.InvalidChars")
            .MustAsync(async (username, ct) => !await userRepo.ExistsByUsernameAsync(username, ct))
            .WithLocalizedMessage("Validation.Username.AlreadyExists");

        RuleFor(x => x.Password)
            .NotEmpty().WithLocalizedMessage("Validation.Password.Required")
            .MinimumLength(8).WithLocalizedMessage("Validation.Password.MinLength")
            .Matches("[A-Z]").WithLocalizedMessage("Validation.Password.Uppercase")
            .Matches("[a-z]").WithLocalizedMessage("Validation.Password.Lowercase")
            .Matches("[0-9]").WithLocalizedMessage("Validation.Password.Digit")
            .Matches("[^a-zA-Z0-9]").WithLocalizedMessage("Validation.Password.Special");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithLocalizedMessage("Validation.Password.Mismatch");
            
        RuleFor(x => x.TermsAccepted)
            .Equal(true).WithLocalizedMessage("Validation.Terms.Required");
    }
}
```

### Frontend (FluentValidation)
```csharp
public class RegisterModelValidator : FluentValidator<RegisterModel>
{
    public RegisterModelValidator(IStringLocalizer<Resources> L) : base(L)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
            
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 30)
            .Matches("^[a-zA-Z0-9_]+$");
            
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Must(ContainUppercase)
            .Must(ContainLowercase)
            .Must(ContainDigit)
            .Must(ContainSpecial);
    }
}
```

## Test Cases (TDD)

```csharp
public class RegisterUserTests
{
    [Theory]
    [InlineData("test@test.cz", "user123", "Strong1!Pass", "Strong1!Pass")]
    public async Task Register_ValidData_CreatesUser(string email, string username, string pass, string confirm)
    {
        // Arrange
        var request = new RegisterRequest(email, username, pass, confirm, true);
        
        // Act
        var result = await _sut.Register(request);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsError()
    {
        // Arrange
        _userRepoMock.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        // Act & Assert
        var result = await _sut.Register(new RegisterRequest("exists@test.cz", "user", "Pass1!", "Pass1!", true));
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.AlreadyExists");
    }
    
    [Theory]
    [InlineData("weak", "Password.MinLength")]
    [InlineData("noupper1!", "Password.Uppercase")]
    [InlineData("NOLOWER1!", "Password.Lowercase")]
    [InlineData("NoDigit!!", "Password.Digit")]
    [InlineData("NoSpecial1", "Password.Special")]
    public void Register_WeakPassword_ValidationFails(string password, string expectedError)
    {
        // Arrange
        var request = new RegisterRequest("test@test.cz", "user", password, password, true);
        
        // Act
        var result = _validator.Validate(request);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == expectedError);
    }
}
```

## Resource klíče

```
Register.Title
Register.Subtitle
Register.Input.Email.Label
Register.Input.Email.Placeholder
Register.Input.Username.Label
Register.Input.Username.Placeholder
Register.Input.Password.Label
Register.Input.Password.Placeholder
Register.Input.ConfirmPassword.Label
Register.Input.Terms.Label
Register.Input.Terms.Link
Register.Button.Submit
Register.Button.Google
Register.Link.Login
Register.Link.Login.Text
Register.Error.EmailExists
Register.Error.UsernameExists
Register.Success.Welcome
```

## UI/UX Reference
- Viz `docs/ui-ux/UI-UX-003-RegistracePrihlaseni.md`

## Odhad implementace
| Část | Hodiny |
|------|--------|
| Backend API + Validace | 4h |
| Database model | 2h |
| Frontend Blazor | 6h |
| Unit/Integration testy | 4h |
| **Celkem** | **16h** |
