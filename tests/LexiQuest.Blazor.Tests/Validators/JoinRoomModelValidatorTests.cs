using FluentAssertions;
using FluentValidation.TestHelper;
using LexiQuest.Blazor.Models;
using LexiQuest.Blazor.Validators;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Blazor.Tests.Validators;

public class JoinRoomModelValidatorTests
{
    private readonly JoinRoomModelValidator _validator;

    public JoinRoomModelValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<LexiQuest.Blazor.Pages.Multiplayer>>();
        localizer["Validation_RoomCode_Required"].Returns(new LocalizedString("Validation_RoomCode_Required", "Kód místnosti je povinný"));
        localizer["Validation_RoomCode_InvalidFormat"].Returns(new LocalizedString("Validation_RoomCode_InvalidFormat", "Kód musí být ve formátu LEXIQ-XXXX"));
        
        _validator = new JoinRoomModelValidator(localizer);
    }

    [Fact]
    public void Validate_EmptyCode_ShouldHaveValidationError()
    {
        // Arrange
        var model = new JoinRoomModel { Code = "" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Kód místnosti je povinný");
    }

    [Fact]
    public void Validate_NullCode_ShouldHaveValidationError()
    {
        // Arrange
        var model = new JoinRoomModel { Code = null! };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("LEXIQ")]
    [InlineData("LEXIQ-")]
    [InlineData("LEXIQ-ABC")]
    [InlineData("LEXIQ-ABCDE")]
    [InlineData("ABCD-1234")]
    [InlineData("lexiq-abcd")]
    public void Validate_InvalidCodeFormat_ShouldHaveValidationError(string code)
    {
        // Arrange
        var model = new JoinRoomModel { Code = code };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Kód musí být ve formátu LEXIQ-XXXX");
    }

    [Theory]
    [InlineData("LEXIQ-ABCD")]
    [InlineData("LEXIQ-1234")]
    [InlineData("LEXIQ-A1B2")]
    [InlineData("LEXIQ-9Z8Y")]
    public void Validate_ValidCode_ShouldNotHaveValidationError(string code)
    {
        // Arrange
        var model = new JoinRoomModel { Code = code };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_ValidCodeWithLowerCase_ShouldHaveValidationError()
    {
        // Arrange - lowercase should fail
        var model = new JoinRoomModel { Code = "lexiq-abcd" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }
}
