using FluentValidation.TestHelper;
using LexiQuest.Core.Validators;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Core.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<LoginRequestValidator>>();
        localizer["Validation.Email.Required"].Returns(new LocalizedString("Validation.Email.Required", "Email je povinný"));
        localizer["Validation.Email.Invalid"].Returns(new LocalizedString("Validation.Email.Invalid", "Neplatný formát emailu"));
        localizer["Validation.Password.Required"].Returns(new LocalizedString("Validation.Password.Required", "Heslo je povinné"));

        _validator = new LoginRequestValidator(localizer);
    }

    [Fact]
    public void LoginRequestValidator_EmptyEmail_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "",
            Password = "password123",
            RememberMe = false
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void LoginRequestValidator_EmptyPassword_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "",
            RememberMe = false
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    public void LoginRequestValidator_InvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = "password123",
            RememberMe = false
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("test@example.com", "password123")]
    [InlineData("user@test.org", "MyP@ssw0rd")]
    public void LoginRequestValidator_ValidRequest_NoErrors(string email, string password)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = password,
            RememberMe = false
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
