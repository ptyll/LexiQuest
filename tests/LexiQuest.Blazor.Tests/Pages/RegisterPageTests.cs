using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Models;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Blazor.Validators;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Pages;

public class RegisterPageTests : BunitContext
{
    private readonly IAuthService _authService;
    private readonly IStringLocalizer<Register> _localizer;
    private readonly IStringLocalizer<RegisterModelValidator> _validatorLocalizer;

    public RegisterPageTests()
    {
        _authService = Substitute.For<IAuthService>();
        _localizer = Substitute.For<IStringLocalizer<Register>>();
        _validatorLocalizer = Substitute.For<IStringLocalizer<RegisterModelValidator>>();

        SetupLocalizer();

        Services.AddSingleton(_authService);
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_validatorLocalizer);
        Services.AddTransient<RegisterModelValidator>();
    }

    private void SetupLocalizer()
    {
        _localizer["Title"].Returns(new LocalizedString("Title", "Registrace"));
        _localizer["Subtitle"].Returns(new LocalizedString("Subtitle", "Vytvořte si účet"));
        _localizer["Input.Email.Label"].Returns(new LocalizedString("Input.Email.Label", "Email"));
        _localizer["Input.Email.Placeholder"].Returns(new LocalizedString("Input.Email.Placeholder", "vas@email.cz"));
        _localizer["Input.Username.Label"].Returns(new LocalizedString("Input.Username.Label", "Uživatelské jméno"));
        _localizer["Input.Username.Placeholder"].Returns(new LocalizedString("Input.Username.Placeholder", " vase_jmeno"));
        _localizer["Input.Password.Label"].Returns(new LocalizedString("Input.Password.Label", "Heslo"));
        _localizer["Input.Password.Placeholder"].Returns(new LocalizedString("Input.Password.Placeholder", "Min. 8 znaků"));
        _localizer["Input.ConfirmPassword.Label"].Returns(new LocalizedString("Input.ConfirmPassword.Label", "Potvrzení hesla"));
        _localizer["Input.ConfirmPassword.Placeholder"].Returns(new LocalizedString("Input.ConfirmPassword.Placeholder", "Zopakujte heslo"));
        _localizer["Checkbox.Terms"].Returns(new LocalizedString("Checkbox.Terms", "Souhlasím s podmínkami"));
        _localizer["Button.Submit"].Returns(new LocalizedString("Button.Submit", "Zaregistrovat se"));
        _localizer["Button.Google"].Returns(new LocalizedString("Button.Google", "Registrovat přes Google"));
        _localizer["Link.Login.Text"].Returns(new LocalizedString("Link.Login.Text", "Již máte účet?"));
        _localizer["Link.Login"].Returns(new LocalizedString("Link.Login", "Přihlaste se"));
        _localizer["Error.RegistrationFailed"].Returns(new LocalizedString("Error.RegistrationFailed", "Registrace selhala"));
        
        // Validator localizer
        _validatorLocalizer["Validation.Email.Required"].Returns(new LocalizedString("Validation.Email.Required", "Email je povinný"));
        _validatorLocalizer["Validation.Email.Invalid"].Returns(new LocalizedString("Validation.Email.Invalid", "Neplatný formát emailu"));
        _validatorLocalizer["Validation.Username.Required"].Returns(new LocalizedString("Validation.Username.Required", "Uživatelské jméno je povinné"));
        _validatorLocalizer["Validation.Username.MinLength"].Returns(new LocalizedString("Validation.Username.MinLength", "Min. 3 znaky"));
        _validatorLocalizer["Validation.Password.Required"].Returns(new LocalizedString("Validation.Password.Required", "Heslo je povinné"));
        _validatorLocalizer["Validation.Password.MinLength"].Returns(new LocalizedString("Validation.Password.MinLength", "Min. 8 znaků"));
        _validatorLocalizer["Validation.Password.Uppercase"].Returns(new LocalizedString("Validation.Password.Uppercase", "Alespoň 1 velké písmeno"));
        _validatorLocalizer["Validation.Password.Lowercase"].Returns(new LocalizedString("Validation.Password.Lowercase", "Alespoň 1 malé písmeno"));
        _validatorLocalizer["Validation.Password.Digit"].Returns(new LocalizedString("Validation.Password.Digit", "Alespoň 1 číslo"));
        _validatorLocalizer["Validation.Password.Special"].Returns(new LocalizedString("Validation.Password.Special", "Alespoň 1 speciální znak"));
        _validatorLocalizer["Validation.Password.Mismatch"].Returns(new LocalizedString("Validation.Password.Mismatch", "Hesla se neshodují"));
        _validatorLocalizer["Validation.Terms.Required"].Returns(new LocalizedString("Validation.Terms.Required", "Musíte souhlasit s podmínkami"));
    }

    [Fact]
    public void RegisterPage_Renders_AllFormFields()
    {
        // Act
        var cut = Render<Register>();

        // Assert
        cut.Find("input[type='email']").Should().NotBeNull();
        cut.Find("input[type='text']").Should().NotBeNull(); // Username
        cut.FindAll("input[type='password']").Count.Should().Be(2); // Password + Confirm
    }

    [Fact]
    public void RegisterPage_Renders_TermsCheckbox()
    {
        // Act
        var cut = Render<Register>();

        // Assert
        cut.Find("input[type='checkbox']").Should().NotBeNull();
    }

    [Fact]
    public void RegisterPage_Renders_SubmitButton()
    {
        // Act
        var cut = Render<Register>();

        // Assert
        var submitButton = cut.Find("button[type='submit']");
        submitButton.Should().NotBeNull();
        submitButton.TextContent.Should().Contain("Zaregistrovat");
    }

    [Fact]
    public void RegisterPage_Renders_LoginLink()
    {
        // Act
        var cut = Render<Register>();

        // Assert
        cut.Find("a[href='/login']").Should().NotBeNull();
    }

    [Fact]
    public void RegisterPage_Renders_AllInputFields()
    {
        // Act
        var cut = Render<Register>();

        // Assert - all form fields should be present
        var inputs = cut.FindAll("input");
        inputs.Count.Should().Be(5); // email + username + 2 passwords + checkbox
        
        cut.Find("input[type='email']").Should().NotBeNull();
        cut.Find("input[type='text']").Should().NotBeNull();
        cut.FindAll("input[type='password']").Count.Should().Be(2);
        cut.Find("input[type='checkbox']").Should().NotBeNull();
    }

    [Fact]
    public void RegisterPage_Renders_GoogleButton()
    {
        // Act
        var cut = Render<Register>();

        // Assert
        var googleButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Google"));
        googleButton.Should().NotBeNull();
    }

    [Fact]
    public void RegisterPage_Renders_Logo()
    {
        // Act
        var cut = Render<Register>();

        // Assert
        cut.Find(".register-logo").Should().NotBeNull();
        cut.Markup.Should().Contain("LexiQuest");
    }
}
