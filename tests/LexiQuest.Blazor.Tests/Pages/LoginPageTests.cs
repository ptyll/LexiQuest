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
using LexiQuest.Blazor.Tests.Helpers;

namespace LexiQuest.Blazor.Tests.Pages;

public class LoginPageTests : BunitContext
{
    private readonly IAuthService _authService;
    private readonly IStringLocalizer<Login> _localizer;
    private readonly IStringLocalizer<LoginModelValidator> _validatorLocalizer;

    public LoginPageTests()
    {
        _authService = Substitute.For<IAuthService>();
        _localizer = Substitute.For<IStringLocalizer<Login>>();
        _validatorLocalizer = Substitute.For<IStringLocalizer<LoginModelValidator>>();

        // Setup localized strings
        SetupLocalizer();

        Services.AddSingleton(_authService);
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_validatorLocalizer);
        Services.AddTransient<LoginModelValidator>();
        TempoTestHelper.RegisterTempoServices(Services);
    }

    private void SetupLocalizer()
    {
        _localizer["Title"].Returns(new LocalizedString("Title", "Přihlášení"));
        _localizer["Subtitle"].Returns(new LocalizedString("Subtitle", "Vítejte zpět!"));
        _localizer["Input.Email.Label"].Returns(new LocalizedString("Input.Email.Label", "Email"));
        _localizer["Input.Email.Placeholder"].Returns(new LocalizedString("Input.Email.Placeholder", "vas@email.cz"));
        _localizer["Input.Password.Label"].Returns(new LocalizedString("Input.Password.Label", "Heslo"));
        _localizer["Input.Password.Placeholder"].Returns(new LocalizedString("Input.Password.Placeholder", "Vaše heslo"));
        _localizer["Input.RememberMe.Label"].Returns(new LocalizedString("Input.RememberMe.Label", "Zapamatovat si mě"));
        _localizer["Button.Submit"].Returns(new LocalizedString("Button.Submit", "Přihlásit se"));
        _localizer["Link.ForgotPassword"].Returns(new LocalizedString("Link.ForgotPassword", "Zapomenuté heslo?"));
        _localizer["Link.Register"].Returns(new LocalizedString("Link.Register", "Zaregistrovat se"));
        _localizer["Link.Register.Text"].Returns(new LocalizedString("Link.Register.Text", "Nemáte účet?"));
        _localizer["Button.Google"].Returns(new LocalizedString("Button.Google", "Přihlásit přes Google"));
        _localizer["Error.InvalidCredentials"].Returns(new LocalizedString("Error.InvalidCredentials", "Nesprávný email nebo heslo"));
        
        // Validator localizer
        _validatorLocalizer["Validation.Email.Required"].Returns(new LocalizedString("Validation.Email.Required", "Email je povinný"));
        _validatorLocalizer["Validation.Email.Invalid"].Returns(new LocalizedString("Validation.Email.Invalid", "Neplatný formát emailu"));
        _validatorLocalizer["Validation.Password.Required"].Returns(new LocalizedString("Validation.Password.Required", "Heslo je povinné"));
    }

    [Fact]
    public void LoginPage_Renders_EmailAndPasswordFields()
    {
        // Act
        var cut = Render<Login>();

        // Assert
        cut.Find("input[type='email']").Should().NotBeNull();
        cut.Find("input[type='password']").Should().NotBeNull();
    }

    [Fact]
    public void LoginPage_Renders_RememberMeCheckbox()
    {
        // Act
        var cut = Render<Login>();

        // Assert
        cut.Find("input[type='checkbox']").Should().NotBeNull();
    }

    [Fact]
    public void LoginPage_Renders_SubmitButton()
    {
        // Act
        var cut = Render<Login>();

        // Assert
        cut.Find("button[type='submit']").Should().NotBeNull();
    }

    [Fact]
    public void LoginPage_Renders_RegisterLink()
    {
        // Act
        var cut = Render<Login>();

        // Assert
        cut.Find("a[href='/register']").Should().NotBeNull();
    }

    [Fact]
    public void LoginPage_InvalidForm_ShowsErrorMessage()
    {
        // Arrange
        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(new AuthResult { Success = false, ErrorMessage = "Error" });

        // Act - submit empty form
        var cut = Render<Login>();
        var form = cut.Find("form");
        form.Submit();

        // Assert - Error message should be shown for empty fields
        cut.Find(".alert-error").Should().NotBeNull();
    }

    [Fact]
    public void LoginPage_SubmitValid_CallsAuthService()
    {
        // Arrange
        _authService.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
            .Returns(new AuthResult { Success = true });

        var cut = Render<Login>();
        
        // Fill form - use Change instead of Input for InputText components
        cut.Find("input[type='email']").Change("test@example.com");
        cut.Find("input[type='password']").Change("Password123!");

        // Act
        var form = cut.Find("form");
        form.Submit();

        // Assert
        _authService.Received(1).LoginAsync("test@example.com", "Password123!", Arg.Any<bool>());
    }
}
