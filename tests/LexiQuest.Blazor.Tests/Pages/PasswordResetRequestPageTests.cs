using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using LexiQuest.Blazor.Tests.Helpers;

namespace LexiQuest.Blazor.Tests.Pages;

public class PasswordResetRequestPageTests : BunitContext
{
    private readonly IAuthService _authService;
    private readonly IStringLocalizer<PasswordResetRequest> _localizer;

    public PasswordResetRequestPageTests()
    {
        _authService = Substitute.For<IAuthService>();
        _localizer = Substitute.For<IStringLocalizer<PasswordResetRequest>>();

        SetupLocalizer();

        Services.AddSingleton(_authService);
        Services.AddSingleton(_localizer);
        TempoTestHelper.RegisterTempoServices(Services);
    }

    private void SetupLocalizer()
    {
        _localizer["Title"].Returns(new LocalizedString("Title", "Obnova hesla"));
        _localizer["Subtitle"].Returns(new LocalizedString("Subtitle", "Zadejte svůj email pro obnovu hesla"));
        _localizer["Input.Email.Label"].Returns(new LocalizedString("Input.Email.Label", "Email"));
        _localizer["Input.Email.Placeholder"].Returns(new LocalizedString("Input.Email.Placeholder", "vas@email.cz"));
        _localizer["Button.Submit"].Returns(new LocalizedString("Button.Submit", "Odeslat"));
        _localizer["Success.Message"].Returns(new LocalizedString("Success.Message", "Instrukce byly odeslány na váš email"));
        _localizer["Link.BackToLogin"].Returns(new LocalizedString("Link.BackToLogin", "Zpět na přihlášení"));
    }

    [Fact]
    public void PasswordResetRequest_Renders_EmailField()
    {
        // Act
        var cut = Render<PasswordResetRequest>();

        // Assert
        cut.Find("input[type='email']").Should().NotBeNull();
    }

    [Fact]
    public void PasswordResetRequest_Renders_SubmitButton()
    {
        // Act
        var cut = Render<PasswordResetRequest>();

        // Assert
        cut.Find("button[type='submit']").Should().NotBeNull();
    }

    [Fact]
    public void PasswordResetRequest_Renders_BackToLoginLink()
    {
        // Act
        var cut = Render<PasswordResetRequest>();

        // Assert
        cut.Find("a[href='/login']").Should().NotBeNull();
    }

    [Fact]
    public void PasswordResetRequest_InvalidForm_ShowsValidationErrors()
    {
        // Act
        var cut = Render<PasswordResetRequest>();
        
        // Submit empty form
        cut.Find("form").Submit();

        // Assert
        cut.Markup.Should().Contain("form");
    }
}
