using FluentAssertions;
using FluentValidation.TestHelper;
using LexiQuest.Api.Validators;
using LexiQuest.Shared.DTOs.Users;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Api.Tests.Validators;

public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _validator;

    public ChangePasswordValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ValidationMessages>>();
        localizer[Arg.Any<string>()].Returns(x => new LocalizedString(x.Arg<string>(), x.Arg<string>()));
        _validator = new ChangePasswordValidator(localizer);
    }

    private static ChangePasswordRequest ValidRequest() =>
        new()
        {
            CurrentPassword = "OldPass1!",
            NewPassword = "NewStr0ng!Pass",
            ConfirmPassword = "NewStr0ng!Pass"
        };

    // ── CurrentPassword tests ──

    [Fact]
    public void CurrentPassword_Empty_ShouldFail()
    {
        var model = ValidRequest();
        model.CurrentPassword = "";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void CurrentPassword_NonEmpty_ShouldPass()
    {
        var model = ValidRequest();
        model.CurrentPassword = "anything";
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.CurrentPassword);
    }

    // ── NewPassword length boundary tests ──

    [Fact]
    public void NewPassword_Empty_ShouldFail()
    {
        var model = ValidRequest();
        model.NewPassword = "";
        model.ConfirmPassword = "";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_ExactMinLength8_ShouldPass()
    {
        var model = ValidRequest();
        model.NewPassword = "Abcde1!x";  // exactly 8 chars with all rules met
        model.ConfirmPassword = "Abcde1!x";
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_BelowMin7Chars_ShouldFail()
    {
        var model = ValidRequest();
        model.NewPassword = "Abcd1!x";  // 7 chars
        model.ConfirmPassword = "Abcd1!x";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    // ── NewPassword complexity rules ──

    [Fact]
    public void NewPassword_MissingUppercase_ShouldFail()
    {
        var model = ValidRequest();
        model.NewPassword = "lowercase1!";
        model.ConfirmPassword = "lowercase1!";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_MissingLowercase_ShouldFail()
    {
        var model = ValidRequest();
        model.NewPassword = "UPPERCASE1!";
        model.ConfirmPassword = "UPPERCASE1!";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_MissingDigit_ShouldFail()
    {
        var model = ValidRequest();
        model.NewPassword = "NoDigits!!Aa";
        model.ConfirmPassword = "NoDigits!!Aa";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_MissingSpecialChar_ShouldFail()
    {
        var model = ValidRequest();
        model.NewPassword = "NoSpecial1Aa";
        model.ConfirmPassword = "NoSpecial1Aa";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_AllRulesMet_ShouldPass()
    {
        var model = ValidRequest();
        model.NewPassword = "Str0ng!Pass";
        model.ConfirmPassword = "Str0ng!Pass";
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    // ── ConfirmPassword tests ──

    [Fact]
    public void ConfirmPassword_Mismatch_ShouldFail()
    {
        var model = ValidRequest();
        model.NewPassword = "Str0ng!Pass";
        model.ConfirmPassword = "Different1!Pass";
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void ConfirmPassword_Matches_ShouldPass()
    {
        var model = ValidRequest();
        model.NewPassword = "Str0ng!Pass";
        model.ConfirmPassword = "Str0ng!Pass";
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    // ── Valid request ──

    [Fact]
    public void ValidRequest_ShouldPass()
    {
        var model = ValidRequest();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Security strings in passwords ──

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("admin'--")]
    public void NewPassword_SqlInjectionStrings_FailBecauseMissingRules(string password)
    {
        // These strings lack uppercase/lowercase/digit/special combos
        var model = ValidRequest();
        model.NewPassword = password;
        model.ConfirmPassword = password;
        var result = _validator.TestValidate(model);
        // At least one rule should fail (likely missing digit or uppercase etc.)
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img onerror=alert(1) src=x>")]
    public void NewPassword_XssStrings_FailBecauseMissingRules(string password)
    {
        var model = ValidRequest();
        model.NewPassword = password;
        model.ConfirmPassword = password;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_SqlInjectionMeetingAllRules_ShouldPass()
    {
        // A SQL injection string that actually meets all password rules
        var password = "'; DROP1a!";
        var model = ValidRequest();
        model.NewPassword = password;
        model.ConfirmPassword = password;
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_CzechCharacters_MeetingRules_ShouldPass()
    {
        // Czech chars count as "special" (non-alphanumeric) + has uppercase, lowercase, digit
        var password = "Příliš1žl";
        var model = ValidRequest();
        model.NewPassword = password;
        model.ConfirmPassword = password;
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NewPassword_ExtremelyLongString_MeetingRules_ShouldPass()
    {
        // No max length rule on password, so a very long password meeting all rules should pass
        var password = "Aa1!" + new string('x', 9997);
        var model = ValidRequest();
        model.NewPassword = password;
        model.ConfirmPassword = password;
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }
}
