using FluentAssertions;
using FluentValidation.TestHelper;
using LexiQuest.Core.Validators;
using LexiQuest.Shared.DTOs.Teams;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Core.Tests.Validators;

public class CreateTeamValidatorTests
{
    private readonly CreateTeamValidator _validator;

    public CreateTeamValidatorTests()
    {
        var localizer = Substitute.For<IStringLocalizer<ValidationMessages>>();
        localizer[Arg.Any<string>()].Returns(x => new LocalizedString(x.Arg<string>(), x.Arg<string>()));
        _validator = new CreateTeamValidator(localizer);
    }

    private static CreateTeamRequest ValidRequest() =>
        new("ValidTeam", "AB", "A valid team", null);

    // ── Name boundary tests ──

    [Fact]
    public void Name_ExactMinLength3_ShouldPass()
    {
        var model = ValidRequest() with { Name = new string('a', 3) };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_ExactMaxLength30_ShouldPass()
    {
        var model = ValidRequest() with { Name = new string('a', 30) };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_BelowMin2Chars_ShouldFail()
    {
        var model = ValidRequest() with { Name = new string('a', 2) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_AboveMax31Chars_ShouldFail()
    {
        var model = ValidRequest() with { Name = new string('a', 31) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_Empty_ShouldFail()
    {
        var model = ValidRequest() with { Name = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    // ── Tag boundary tests ──

    [Fact]
    public void Tag_ExactMinLength2_ShouldPass()
    {
        var model = ValidRequest() with { Tag = "AB" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Tag);
    }

    [Fact]
    public void Tag_ExactMaxLength4_ShouldPass()
    {
        var model = ValidRequest() with { Tag = "ABCD" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Tag);
    }

    [Fact]
    public void Tag_BelowMin1Char_ShouldFail()
    {
        var model = ValidRequest() with { Tag = "A" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Tag);
    }

    [Fact]
    public void Tag_AboveMax5Chars_ShouldFail()
    {
        var model = ValidRequest() with { Tag = "ABCDE" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Tag);
    }

    [Fact]
    public void Tag_Empty_ShouldFail()
    {
        var model = ValidRequest() with { Tag = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Tag);
    }

    // ── Tag format tests ──

    [Theory]
    [InlineData("ab")]
    [InlineData("Ab")]
    [InlineData("aB")]
    public void Tag_LowercaseLetters_ShouldFail(string tag)
    {
        var model = ValidRequest() with { Tag = tag };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Tag);
    }

    [Theory]
    [InlineData("A!")]
    [InlineData("A@")]
    [InlineData("A#")]
    [InlineData("A-")]
    [InlineData("A_")]
    [InlineData("A ")]
    public void Tag_SpecialCharacters_ShouldFail(string tag)
    {
        var model = ValidRequest() with { Tag = tag };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Tag);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("A1")]
    [InlineData("99")]
    [InlineData("XY99")]
    public void Tag_UppercaseAndDigitsOnly_ShouldPass(string tag)
    {
        var model = ValidRequest() with { Tag = tag };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Tag);
    }

    // ── Description tests ──

    [Fact]
    public void Description_Exactly500Chars_ShouldPass()
    {
        var model = ValidRequest() with { Description = new string('a', 500) };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_501Chars_ShouldFail()
    {
        var model = ValidRequest() with { Description = new string('a', 501) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_Null_ShouldPass()
    {
        var model = ValidRequest() with { Description = null };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_Empty_ShouldPass()
    {
        var model = ValidRequest() with { Description = "" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    // ── Valid request ──

    [Fact]
    public void ValidRequest_ShouldPass()
    {
        var model = ValidRequest();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Security strings in Name ──

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("admin'--")]
    public void Name_SqlInjection_PassesValidationIfWithinLength(string name)
    {
        // SQL injection strings should pass name validation if within length bounds,
        // since the validator only checks length, not content
        var model = ValidRequest() with { Name = name };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img onerror=alert(1) src=x>")]
    [InlineData("javascript:alert(1)")]
    public void Name_XssAttempt_PassesValidationIfWithinLength(string name)
    {
        var model = ValidRequest() with { Name = name };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_CzechCharacters_PassesValidationIfWithinLength()
    {
        var model = ValidRequest() with { Name = "příliš žluťoučký kůň" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_MixedLatinCyrillic_PassesValidationIfWithinLength()
    {
        var model = ValidRequest() with { Name = "TeamПривет" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_Emoji_PassesValidationIfWithinLength()
    {
        var model = ValidRequest() with { Name = "Team 🎮🏆" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_ExtremelyLongString_ShouldFail()
    {
        var model = ValidRequest() with { Name = new string('a', 10001) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    // ── Security strings in Tag ──

    [Theory]
    [InlineData("'OR")]
    [InlineData("<SC")]
    public void Tag_SecurityStrings_RejectedByFormatRegex(string tag)
    {
        var model = ValidRequest() with { Tag = tag };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Tag);
    }

    // ── Security strings in Description ──

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("admin'--")]
    public void Description_SqlInjection_PassesIfWithinLength(string desc)
    {
        var model = ValidRequest() with { Description = desc };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img onerror=alert(1) src=x>")]
    [InlineData("javascript:alert(1)")]
    public void Description_XssAttempt_PassesIfWithinLength(string desc)
    {
        var model = ValidRequest() with { Description = desc };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_ExtremelyLongString_ShouldFail()
    {
        var model = ValidRequest() with { Description = new string('a', 10001) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_CzechCharacters_ShouldPass()
    {
        var model = ValidRequest() with { Description = "příliš žluťoučký kůň úpěl ďábelské ódy" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_Emoji_ShouldPass()
    {
        var model = ValidRequest() with { Description = "Best team ever! 🎮🏆🔥" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
