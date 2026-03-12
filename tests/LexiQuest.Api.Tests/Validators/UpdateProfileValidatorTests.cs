using FluentAssertions;
using FluentValidation.TestHelper;
using LexiQuest.Api.Validators;
using LexiQuest.Shared.DTOs.Users;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Api.Tests.Validators;

public class UpdateProfileValidatorTests
{
    private readonly UpdateProfileValidator _validator;

    public UpdateProfileValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ValidationMessages>>();
        localizer[Arg.Any<string>()].Returns(x => new LocalizedString(x.Arg<string>(), x.Arg<string>()));
        _validator = new UpdateProfileValidator(localizer);
    }

    private static UpdateProfileRequest ValidRequest() =>
        new() { Username = "validuser", Email = "test@example.com" };

    // ── Username boundary tests ──

    [Fact]
    public void Username_ExactMinLength3_ShouldPass()
    {
        var model = ValidRequest();
        model.Username = new string('a', 3);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Username_ExactMaxLength50_ShouldPass()
    {
        var model = ValidRequest();
        model.Username = new string('a', 50);
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Username_BelowMin2Chars_ShouldFail()
    {
        var model = ValidRequest();
        model.Username = new string('a', 2);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Username_AboveMax51Chars_ShouldFail()
    {
        var model = ValidRequest();
        model.Username = new string('a', 51);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Username_Empty_ShouldFail()
    {
        var model = ValidRequest();
        model.Username = "";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    // ── Username format tests ──

    [Fact]
    public void Username_UnderscoreAllowed_ShouldPass()
    {
        var model = ValidRequest();
        model.Username = "valid_user_123";
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("user name")]
    [InlineData("user-name")]
    [InlineData("user@name")]
    [InlineData("user.name")]
    [InlineData("user!name")]
    [InlineData("user#name")]
    public void Username_SpecialChars_ShouldFail(string username)
    {
        var model = ValidRequest();
        model.Username = username;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    // ── Username unicode rejection ──

    [Theory]
    [InlineData("příliš")]
    [InlineData("žluťoučký")]
    [InlineData("пользователь")]
    [InlineData("用户名")]
    public void Username_UnicodeCharacters_ShouldFail(string username)
    {
        var model = ValidRequest();
        model.Username = username;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Username_Emoji_ShouldFail()
    {
        var model = ValidRequest();
        model.Username = "user🎮";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Username_MixedLatinCyrillic_ShouldFail()
    {
        var model = ValidRequest();
        model.Username = "userПривет";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    // ── Email tests ──

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user@domain.org")]
    [InlineData("name.surname@company.co.uk")]
    public void Email_ValidFormat_ShouldPass(string email)
    {
        var model = ValidRequest();
        model.Email = email;
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    [InlineData("test.com")]
    [InlineData("")]
    public void Email_InvalidFormat_ShouldFail(string email)
    {
        var model = ValidRequest();
        model.Email = email;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // ── Valid request ──

    [Fact]
    public void ValidRequest_ShouldPass()
    {
        var model = ValidRequest();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Security strings in Username ──

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("admin'--")]
    public void Username_SqlInjection_ShouldFail(string username)
    {
        var model = ValidRequest();
        model.Username = username;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img onerror=alert(1) src=x>")]
    [InlineData("javascript:alert(1)")]
    public void Username_XssAttempt_ShouldFail(string username)
    {
        var model = ValidRequest();
        model.Username = username;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Username_ExtremelyLongString_ShouldFail()
    {
        var model = ValidRequest();
        model.Username = new string('a', 10001);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }
}
