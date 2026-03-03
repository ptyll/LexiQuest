using FluentAssertions;
using LexiQuest.Blazor.Models;
using LexiQuest.Blazor.Validators;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Models;

public class LoginModelValidatorTests
{
    private readonly IStringLocalizer<LoginModelValidator> _localizer;
    private readonly LoginModelValidator _validator;

    public LoginModelValidatorTests()
    {
        _localizer = Substitute.For<IStringLocalizer<LoginModelValidator>>();
        
        // Setup localized strings
        _localizer["Validation.Email.Required"].Returns(new LocalizedString("Validation.Email.Required", "Email je povinný"));
        _localizer["Validation.Email.Invalid"].Returns(new LocalizedString("Validation.Email.Invalid", "Neplatný formát emailu"));
        _localizer["Validation.Password.Required"].Returns(new LocalizedString("Validation.Password.Required", "Heslo je povinné"));
        
        _validator = new LoginModelValidator(_localizer);
    }

    [Fact]
    public void LoginModelValidator_EmptyEmail_ShowsError()
    {
        // Arrange
        var model = new LoginModel
        {
            Email = "",
            Password = "Password123!",
            RememberMe = false
        };

        // Act
        var result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void LoginModelValidator_InvalidEmail_ShowsError()
    {
        // Arrange
        var model = new LoginModel
        {
            Email = "invalid-email",
            Password = "Password123!",
            RememberMe = false
        };

        // Act
        var result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void LoginModelValidator_EmptyPassword_ShowsError()
    {
        // Arrange
        var model = new LoginModel
        {
            Email = "test@example.com",
            Password = "",
            RememberMe = false
        };

        // Act
        var result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void LoginModelValidator_ValidModel_NoErrors()
    {
        // Arrange
        var model = new LoginModel
        {
            Email = "test@example.com",
            Password = "Password123!",
            RememberMe = true
        };

        // Act
        var result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
