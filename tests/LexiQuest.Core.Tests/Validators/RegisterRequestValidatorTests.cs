using FluentValidation.TestHelper;
using LexiQuest.Core.Validators;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Core.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<RegisterRequestValidator>>();
        localizer["Validation.Email.Required"].Returns(new LocalizedString("Validation.Email.Required", "Email je povinný"));
        localizer["Validation.Email.Invalid"].Returns(new LocalizedString("Validation.Email.Invalid", "Neplatný formát emailu"));
        localizer["Validation.Username.Required"].Returns(new LocalizedString("Validation.Username.Required", "Uživatelské jméno je povinné"));
        localizer["Validation.Username.MinLength"].Returns(new LocalizedString("Validation.Username.MinLength", "Uživatelské jméno musí mít alespoň 3 znaky"));
        localizer["Validation.Username.MaxLength"].Returns(new LocalizedString("Validation.Username.MaxLength", "Uživatelské jméno může mít maximálně 30 znaků"));
        localizer["Validation.Username.InvalidChars"].Returns(new LocalizedString("Validation.Username.InvalidChars", "Uživatelské jméno může obsahovat pouze písmena, čísla a podtržítko"));
        localizer["Validation.Password.Required"].Returns(new LocalizedString("Validation.Password.Required", "Heslo je povinné"));
        localizer["Validation.Password.MinLength"].Returns(new LocalizedString("Validation.Password.MinLength", "Heslo musí mít alespoň 8 znaků"));
        localizer["Validation.Password.Uppercase"].Returns(new LocalizedString("Validation.Password.Uppercase", "Heslo musí obsahovat alespoň jedno velké písmeno"));
        localizer["Validation.Password.Lowercase"].Returns(new LocalizedString("Validation.Password.Lowercase", "Heslo musí obsahovat alespoň jedno malé písmeno"));
        localizer["Validation.Password.Digit"].Returns(new LocalizedString("Validation.Password.Digit", "Heslo musí obsahovat alespoň jednu číslici"));
        localizer["Validation.Password.Special"].Returns(new LocalizedString("Validation.Password.Special", "Heslo musí obsahovat alespoň jeden speciální znak"));
        localizer["Validation.Password.Mismatch"].Returns(new LocalizedString("Validation.Password.Mismatch", "Hesla se neshodují"));
        localizer["Validation.Terms.Required"].Returns(new LocalizedString("Validation.Terms.Required", "Musíte souhlasit s podmínkami"));

        _validator = new RegisterRequestValidator(localizer);
    }

    [Fact]
    public void RegisterRequestValidator_EmptyEmail_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "", 
            Username = "testuser", 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    [InlineData("test.com")]
    public void RegisterRequestValidator_InvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = email, 
            Username = "testuser", 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void RegisterRequestValidator_EmptyUsername_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = "", 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void RegisterRequestValidator_UsernameTooShort_ReturnsError(string username)
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = username, 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void RegisterRequestValidator_UsernameTooLong_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = new string('a', 31), 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("user name")]
    [InlineData("user-name")]
    [InlineData("user@name")]
    [InlineData("user.name")]
    public void RegisterRequestValidator_UsernameInvalidChars_ReturnsError(string username)
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = username, 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void RegisterRequestValidator_PasswordTooShort_ReturnsError(string password)
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = "testuser", 
            Password = password, 
            ConfirmPassword = password,
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestValidator_PasswordMissingUppercase_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = "testuser", 
            Password = "lowercase1!", 
            ConfirmPassword = "lowercase1!",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestValidator_PasswordMissingLowercase_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = "testuser", 
            Password = "UPPERCASE1!", 
            ConfirmPassword = "UPPERCASE1!",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestValidator_PasswordMissingDigit_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = "testuser", 
            Password = "NoDigits!!", 
            ConfirmPassword = "NoDigits!!",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestValidator_PasswordMissingSpecialChar_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = "testuser", 
            Password = "NoSpecial1", 
            ConfirmPassword = "NoSpecial1",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequestValidator_PasswordMismatch_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = "testuser", 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Different1!Pass",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void RegisterRequestValidator_TermsNotAccepted_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest 
        { 
            Email = "test@test.com", 
            Username = "testuser", 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = false 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AcceptTerms);
    }

    [Theory]
    [InlineData("test@test.com", "testuser", "Strong1!Pass")]
    [InlineData("user@example.org", "user_123", "MyP@ssw0rd")]
    public void RegisterRequestValidator_ValidRequest_NoErrors(string email, string username, string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = email,
            Username = username,
            Password = password,
            ConfirmPassword = password,
            AcceptTerms = true
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Unicode/Diacritics in Username ──

    [Theory]
    [InlineData("příliš")]
    [InlineData("žluťoučký")]
    [InlineData("kůň")]
    public void RegisterRequestValidator_CzechCharactersInUsername_ShouldFail(string username)
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = username,
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void RegisterRequestValidator_MixedLatinCyrillicUsername_ShouldFail()
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = "userПривет",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void RegisterRequestValidator_EmojiInUsername_ShouldFail()
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = "user🎮",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    // ── SQL injection strings in Username ──

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("admin'--")]
    public void RegisterRequestValidator_SqlInjectionInUsername_ShouldFail(string username)
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = username,
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    // ── XSS attempt strings in Username ──

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img onerror=alert(1) src=x>")]
    [InlineData("javascript:alert(1)")]
    public void RegisterRequestValidator_XssInUsername_ShouldFail(string username)
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = username,
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    // ── Extremely long strings ──

    [Fact]
    public void RegisterRequestValidator_ExtremelyLongUsername_ShouldFail()
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = new string('a', 10001),
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void RegisterRequestValidator_ExtremelyLongEmail_NoMaxLengthRule()
    {
        // The validator has no max length for email, so a long but structurally valid email passes.
        // This documents the current behavior - a max length rule could be added for defense in depth.
        var request = new RegisterRequest
        {
            Email = new string('a', 10001) + "@test.com",
            Username = "testuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void RegisterRequestValidator_ExtremelyLongPassword_MeetingRules_ShouldPass()
    {
        // No max length on password, so extremely long password meeting all rules passes
        var password = "Aa1!" + new string('x', 9997);
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = "testuser",
            Password = password,
            ConfirmPassword = password,
            AcceptTerms = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    // ── SQL injection in Email ──

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("' OR '1'='1")]
    public void RegisterRequestValidator_SqlInjectionInEmail_ShouldFail(string email)
    {
        var request = new RegisterRequest
        {
            Email = email,
            Username = "testuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // ── XSS in Email ──

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("javascript:alert(1)")]
    public void RegisterRequestValidator_XssInEmail_ShouldFail(string email)
    {
        var request = new RegisterRequest
        {
            Email = email,
            Username = "testuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
