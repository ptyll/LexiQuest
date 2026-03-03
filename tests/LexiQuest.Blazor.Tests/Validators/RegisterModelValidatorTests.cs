using FluentValidation.TestHelper;
using LexiQuest.Blazor.Models;
using LexiQuest.Blazor.Validators;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Validators;

public class RegisterModelValidatorTests
{
    private readonly RegisterModelValidator _validator;

    public RegisterModelValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<RegisterModelValidator>>();
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

        _validator = new RegisterModelValidator(localizer);
    }

    [Fact]
    public void RegisterModelValidator_EmptyEmail_ShowsError()
    {
        // Arrange
        var model = new RegisterModel 
        { 
            Email = "", 
            Username = "testuser", 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("weak")]
    [InlineData("noupper1!")]
    [InlineData("NOLOWER1!")]
    [InlineData("NoDigit!!")]
    [InlineData("NoSpecial1")]
    public void RegisterModelValidator_InvalidPassword_ShowsError(string password)
    {
        // Arrange
        var model = new RegisterModel 
        { 
            Email = "test@test.com", 
            Username = "testuser", 
            Password = password, 
            ConfirmPassword = password,
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterModelValidator_PasswordMismatch_ShowsError()
    {
        // Arrange
        var model = new RegisterModel 
        { 
            Email = "test@test.com", 
            Username = "testuser", 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Different1!Pass",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void RegisterModelValidator_ValidModel_NoErrors()
    {
        // Arrange
        var model = new RegisterModel 
        { 
            Email = "test@test.com", 
            Username = "testuser", 
            Password = "Strong1!Pass", 
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true 
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
