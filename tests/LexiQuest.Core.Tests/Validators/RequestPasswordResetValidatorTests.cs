using FluentValidation.TestHelper;
using LexiQuest.Core.Validators;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Core.Tests.Validators;

public class RequestPasswordResetValidatorTests
{
    private readonly RequestPasswordResetValidator _validator;

    public RequestPasswordResetValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<RequestPasswordResetValidator>>();
        localizer["Validation.Email.Required"].Returns(new LocalizedString("Validation.Email.Required", "Email je povinný"));
        localizer["Validation.Email.Invalid"].Returns(new LocalizedString("Validation.Email.Invalid", "Neplatný formát emailu"));

        _validator = new RequestPasswordResetValidator(localizer);
    }

    [Fact]
    public void RequestPasswordResetValidator_EmptyEmail_ReturnsError()
    {
        // Arrange
        var request = new RequestPasswordResetDto { Email = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    public void RequestPasswordResetValidator_InvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var request = new RequestPasswordResetDto { Email = email };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void RequestPasswordResetValidator_ValidEmail_NoErrors()
    {
        // Arrange
        var request = new RequestPasswordResetDto { Email = "test@example.com" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
