using FluentValidation.TestHelper;
using LexiQuest.Core.Validators;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Core.Tests.Validators;

public class ResetPasswordValidatorTests
{
    private readonly ResetPasswordValidator _validator;

    public ResetPasswordValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ResetPasswordValidator>>();
        localizer["Validation.Token.Required"].Returns(new LocalizedString("Validation.Token.Required", "Token je povinný"));
        localizer["Validation.Password.Required"].Returns(new LocalizedString("Validation.Password.Required", "Heslo je povinné"));
        localizer["Validation.Password.MinLength"].Returns(new LocalizedString("Validation.Password.MinLength", "Heslo musí mít alespoň 8 znaků"));
        localizer["Validation.Password.Uppercase"].Returns(new LocalizedString("Validation.Password.Uppercase", "Heslo musí obsahovat alespoň jedno velké písmeno"));
        localizer["Validation.Password.Lowercase"].Returns(new LocalizedString("Validation.Password.Lowercase", "Heslo musí obsahovat alespoň jedno malé písmeno"));
        localizer["Validation.Password.Digit"].Returns(new LocalizedString("Validation.Password.Digit", "Heslo musí obsahovat alespoň jednu číslici"));
        localizer["Validation.Password.Special"].Returns(new LocalizedString("Validation.Password.Special", "Heslo musí obsahovat alespoň jeden speciální znak"));
        localizer["Validation.ConfirmPassword.Required"].Returns(new LocalizedString("Validation.ConfirmPassword.Required", "Potvrzení hesla je povinné"));
        localizer["Validation.ConfirmPassword.Mismatch"].Returns(new LocalizedString("Validation.ConfirmPassword.Mismatch", "Hesla se neshodují"));

        _validator = new ResetPasswordValidator(localizer);
    }

    [Fact]
    public void ResetPasswordValidator_EmptyToken_ReturnsError()
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "", NewPassword = "Valid1!Pass", ConfirmPassword = "Valid1!Pass" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void ResetPasswordValidator_EmptyPassword_ReturnsError()
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "valid_token", NewPassword = "", ConfirmPassword = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void ResetPasswordValidator_ShortPassword_ReturnsError(string password)
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "valid_token", NewPassword = password, ConfirmPassword = password };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ResetPasswordValidator_MissingUppercase_ReturnsError()
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "valid_token", NewPassword = "lowercase1!", ConfirmPassword = "lowercase1!" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ResetPasswordValidator_MissingLowercase_ReturnsError()
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "valid_token", NewPassword = "UPPERCASE1!", ConfirmPassword = "UPPERCASE1!" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ResetPasswordValidator_MissingDigit_ReturnsError()
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "valid_token", NewPassword = "NoDigits!!", ConfirmPassword = "NoDigits!!" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ResetPasswordValidator_MissingSpecialChar_ReturnsError()
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "valid_token", NewPassword = "NoSpecial1", ConfirmPassword = "NoSpecial1" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ResetPasswordValidator_Mismatch_ReturnsError()
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "valid_token", NewPassword = "Valid1!Pass", ConfirmPassword = "Different1!Pass" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void ResetPasswordValidator_ValidRequest_NoErrors()
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "valid_token", NewPassword = "Valid1!Pass", ConfirmPassword = "Valid1!Pass" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
