using FluentAssertions;
using LexiQuest.Core.Validators;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Core.Tests.Validators;

public class SubmitAnswerRequestValidatorTests
{
    private readonly SubmitAnswerRequestValidator _validator;

    public SubmitAnswerRequestValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ValidationMessages>>();
        localizer["Validation.SessionId.Required"].Returns(new LocalizedString("Validation.SessionId.Required", "Session ID is required"));
        localizer["Validation.Answer.Required"].Returns(new LocalizedString("Validation.Answer.Required", "Answer is required"));
        localizer["Validation.Answer.MaxLength"].Returns(new LocalizedString("Validation.Answer.MaxLength", "Answer cannot exceed 50 characters"));
        localizer["Validation.TimeSpent.NonNegative"].Returns(new LocalizedString("Validation.TimeSpent.NonNegative", "Time spent cannot be negative"));

        _validator = new SubmitAnswerRequestValidator(localizer);
    }

    [Fact]
    public void SubmitAnswerValidator_EmptySessionId_ReturnsError()
    {
        // Arrange
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.Empty,
            Answer = "test",
            TimeSpentMs = 1000
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SessionId");
    }

    [Fact]
    public void SubmitAnswerValidator_EmptyAnswer_ReturnsError()
    {
        // Arrange
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = "",
            TimeSpentMs = 1000
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Answer");
    }

    [Fact]
    public void SubmitAnswerValidator_AnswerTooLong_ReturnsError()
    {
        // Arrange
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = new string('a', 51), // 51 characters
            TimeSpentMs = 1000
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Answer");
    }

    [Fact]
    public void SubmitAnswerValidator_NegativeTime_ReturnsError()
    {
        // Arrange
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = "test",
            TimeSpentMs = -1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TimeSpentMs");
    }

    [Fact]
    public void SubmitAnswerValidator_ValidRequest_NoErrors()
    {
        // Arrange
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = "JABLKO",
            TimeSpentMs = 5000
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void SubmitAnswerValidator_AnswerExactly50Characters_IsValid()
    {
        // Arrange
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = new string('a', 50), // exactly 50 characters
            TimeSpentMs = 1000
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SubmitAnswerValidator_ZeroTime_IsValid()
    {
        // Arrange
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = "test",
            TimeSpentMs = 0
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // ── Unicode/Diacritics in Answer ──

    [Fact]
    public void SubmitAnswerValidator_CzechCharacters_ShouldPass()
    {
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = "příliš žluťoučký kůň",
            TimeSpentMs = 1000
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SubmitAnswerValidator_MixedLatinCyrillic_ShouldPass()
    {
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = "helloПривет",
            TimeSpentMs = 1000
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SubmitAnswerValidator_Emoji_ShouldPass()
    {
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = "answer 🎮",
            TimeSpentMs = 1000
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    // ── SQL injection strings in Answer ──

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("admin'--")]
    public void SubmitAnswerValidator_SqlInjectionInAnswer_PassesIfWithinLength(string answer)
    {
        // Validator only checks length and not-empty, so SQL injection strings pass if within 50 chars
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = answer,
            TimeSpentMs = 1000
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    // ── XSS attempt strings in Answer ──

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img onerror=alert(1) src=x>")]
    [InlineData("javascript:alert(1)")]
    public void SubmitAnswerValidator_XssInAnswer_PassesIfWithinLength(string answer)
    {
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = answer,
            TimeSpentMs = 1000
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    // ── Extremely long strings ──

    [Fact]
    public void SubmitAnswerValidator_ExtremelyLongAnswer_ShouldFail()
    {
        var request = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = new string('a', 10001),
            TimeSpentMs = 1000
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Answer");
    }
}
