using FluentAssertions;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Smoke")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class AuthE2ETests : E2ETestBase
{
    public AuthE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Register_ValidNewUser_RedirectsToDashboard()
    {
        await RunScenarioAsync("auth", "register-success", async page =>
        {
            var unique = Guid.NewGuid().ToString("N")[..10];
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/register");

            await page.GetByLabel("Email").FillAsync($"registrace.{unique}@lexiquest.test");
            await page.GetByLabel("Uživatelské jméno").FillAsync($"user{unique}"[..14]);
            await page.GetByPlaceholder("Min. 8 znaků, 1 velké písmeno, 1 číslo").FillAsync("TestPass123!");
            await page.GetByLabel("Potvrdit heslo").FillAsync("TestPass123!");
            await page.GetByLabel("Souhlasím s podmínkami použití").CheckAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Vytvořit účet" }).ClickAsync();

            await page.WaitForURLAsync("**/dashboard");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přehled" })).ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Login_ExistingUser_RedirectsToDashboard()
    {
        await RunScenarioAsync("auth", "login-success", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("login");
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/login");

            await page.GetByLabel("Email").FillAsync(user.Email);
            await page.GetByLabel("Heslo").FillAsync(user.Password);
            await page.GetByRole(AriaRole.Button, new() { Name = "Přihlásit se" }).ClickAsync();

            await page.WaitForURLAsync("**/dashboard");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přehled" })).ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Login_RememberMe_StaysAuthenticatedAfterDashboardReload()
    {
        await RunScenarioAsync("auth", "login-remember-me-reload", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("remember");
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/login");

            await page.GetByLabel("Email").FillAsync(user.Email);
            await page.GetByLabel("Heslo").FillAsync(user.Password);
            await page.GetByText("Zapamatovat si mě").ClickAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Přihlásit se" }).ClickAsync();

            await page.WaitForURLAsync("**/dashboard");
            await Expect(page.GetByText("Celkové XP")).ToBeVisibleAsync();

            await page.ReloadAsync();
            await Fixture.WaitForNoBusyIndicatorsAsync(page);

            page.Url.Should().Contain("/dashboard");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přehled" })).ToBeVisibleAsync();
            await Expect(page.GetByText("Celkové XP")).ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Session_ExpiredAccessToken_RefreshesWithRefreshTokenAndLoadsDashboard()
    {
        await RunScenarioAsync("auth", "session-refresh-expired-access-token", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("refresh");
            var auth = await Fixture.AuthenticateAsync(user, rememberMe: true);
            var expiredAccessToken = Fixture.CreateExpiredAccessToken(auth);

            await Fixture.GoToAndWaitForAppReadyAsync(page);
            await page.EvaluateAsync(
                """
                ([accessToken, refreshToken]) => {
                    window.localStorage.setItem('access_token', accessToken);
                    window.localStorage.setItem('refresh_token', refreshToken);
                }
                """,
                new[] { expiredAccessToken, auth.RefreshToken });

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            page.Url.Should().Contain("/dashboard");
            await Expect(page.GetByText("Celkové XP")).ToBeVisibleAsync();

            var tokens = await page.EvaluateAsync<string[]>(
                """
                () => [
                    window.localStorage.getItem('access_token') || '',
                    window.localStorage.getItem('refresh_token') || ''
                ]
                """);
            tokens[0].Should().NotBe(expiredAccessToken);
            tokens[0].Should().NotBeNullOrWhiteSpace();
            tokens[1].Should().NotBeNullOrWhiteSpace();
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Session_InvalidRefreshToken_LogsOutAndClearsStoredTokens()
    {
        await RunScenarioAsync("auth", "session-invalid-refresh-token", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("badrefresh");
            var auth = await Fixture.AuthenticateAsync(user, rememberMe: true);
            var expiredAccessToken = Fixture.CreateExpiredAccessToken(auth);

            await Fixture.GoToAndWaitForAppReadyAsync(page);
            await page.EvaluateAsync(
                """
                ([accessToken, refreshToken]) => {
                    window.localStorage.setItem('access_token', accessToken);
                    window.localStorage.setItem('refresh_token', refreshToken);
                }
                """,
                new[] { expiredAccessToken, "invalid-refresh-token" });

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");
            await page.WaitForURLAsync("**/login");

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přihlášení" })).ToBeVisibleAsync();

            var tokens = await page.EvaluateAsync<string[]>(
                """
                () => [
                    window.localStorage.getItem('access_token') || '',
                    window.localStorage.getItem('refresh_token') || ''
                ]
                """);
            tokens.Should().OnlyContain(token => string.IsNullOrEmpty(token));
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShowsGenericError()
    {
        await RunScenarioAsync("auth", "login-invalid-credentials", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/login");

            await page.GetByLabel("Email").FillAsync("neexistuje@lexiquest.test");
            await page.GetByLabel("Heslo").FillAsync("WrongPassword123!");
            await page.GetByRole(AriaRole.Button, new() { Name = "Přihlásit se" }).ClickAsync();

            await Expect(page.GetByText("Přihlášení selhalo")).ToBeVisibleAsync();
            await Expect(page.GetByText("Nesprávný email nebo heslo")).ToBeVisibleAsync();
            page.Url.Should().Contain("/login");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "auth",
                scenario: "login-invalid-credentials",
                state: "error",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous");
        });
    }

    [Fact]
    public async Task Login_FiveWrongAttempts_LocksAccountAndShowsLocalizedLockout()
    {
        await RunScenarioAsync("auth", "login-lockout", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("lockout");
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/login");

            for (var attempt = 0; attempt < 5; attempt++)
            {
                await page.GetByLabel("Email").FillAsync(user.Email);
                await page.GetByLabel("Heslo").FillAsync($"SpatneHeslo{attempt}!");
                await page.GetByRole(AriaRole.Button, new() { Name = "Přihlásit se" }).ClickAsync();
                await Expect(page.GetByText("Nesprávný email nebo heslo")).ToBeVisibleAsync();
            }

            await page.GetByLabel("Email").FillAsync(user.Email);
            await page.GetByLabel("Heslo").FillAsync(user.Password);
            await page.GetByRole(AriaRole.Button, new() { Name = "Přihlásit se" }).ClickAsync();

            await Expect(page.GetByText("Účet je dočasně zablokován. Zkuste to znovu za 15 minut.")).ToBeVisibleAsync();
            page.Url.Should().Contain("/login");
        }, assertNoConsoleErrors: false);
    }

    [Fact]
    public async Task Logout_FromTopBarClearsTokensAndProtectedRouteReturnsToLogin()
    {
        await RunScenarioAsync("auth", "logout-clears-session", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("logout");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přehled" })).ToBeVisibleAsync();
            await Expect(page.GetByText("Celkové XP")).ToBeVisibleAsync();

            await page.GetByRole(AriaRole.Button, new() { Name = "Odhlásit se" }).ClickAsync();
            await page.WaitForURLAsync("**/login");

            var tokens = await page.EvaluateAsync<string[]>(
                """
                () => [
                    window.localStorage.getItem('access_token') || '',
                    window.localStorage.getItem('refresh_token') || ''
                ]
                """);
            tokens.Should().OnlyContain(token => string.IsNullOrEmpty(token));

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");
            await page.WaitForURLAsync("**/login");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přihlášení" })).ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Register_DuplicateEmail_ShowsLocalizedError()
    {
        await RunScenarioAsync("auth", "register-duplicate-email", async page =>
        {
            var existing = await Fixture.RegisterUniqueUserAsync("duplicate");
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/register");

            await page.GetByLabel("Email").FillAsync(existing.Email);
            await page.GetByLabel("Uživatelské jméno").FillAsync($"jinak{Guid.NewGuid():N}"[..14]);
            await page.GetByPlaceholder("Min. 8 znaků, 1 velké písmeno, 1 číslo").FillAsync(existing.Password);
            await page.GetByLabel("Potvrdit heslo").FillAsync(existing.Password);
            await page.GetByLabel("Souhlasím s podmínkami použití").CheckAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Vytvořit účet" }).ClickAsync();

            await Expect(page.GetByText("Registrace selhala")).ToBeVisibleAsync();
            page.Url.Should().Contain("/register");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "auth",
                scenario: "register-duplicate-email",
                state: "error",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous");
        });
    }

    [Fact]
    public async Task Register_DuplicateUsername_ShowsLocalizedError()
    {
        await RunScenarioAsync("auth", "register-duplicate-username", async page =>
        {
            var existing = await Fixture.RegisterUniqueUserAsync("dupuser");
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/register");

            await page.GetByLabel("Email").FillAsync($"novy.{Guid.NewGuid():N}@lexiquest.test");
            await page.GetByLabel("Uživatelské jméno").FillAsync(existing.Username);
            await page.GetByPlaceholder("Min. 8 znaků, 1 velké písmeno, 1 číslo").FillAsync(existing.Password);
            await page.GetByLabel("Potvrdit heslo").FillAsync(existing.Password);
            await page.GetByLabel("Souhlasím s podmínkami použití").CheckAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Vytvořit účet" }).ClickAsync();

            await Expect(page.GetByText("Registrace selhala")).ToBeVisibleAsync();
            page.Url.Should().Contain("/register");
        });
    }

    [Fact]
    public async Task Register_InvalidInputs_ShowLocalizedValidationMessagesWithoutRawKeys()
    {
        await RunScenarioAsync("auth", "register-validation-errors", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/register");

            await page.GetByLabel("Email").FillAsync("spatny-email");
            await page.GetByLabel("Uživatelské jméno").FillAsync("č");
            await page.GetByPlaceholder("Min. 8 znaků, 1 velké písmeno, 1 číslo").FillAsync("kratke");
            await page.GetByLabel("Potvrdit heslo").FillAsync("jine-heslo");
            await page.GetByRole(AriaRole.Button, new() { Name = "Vytvořit účet" }).ClickAsync();

            await Expect(page.GetByText("Zadejte platný email")).ToBeVisibleAsync();
            await Expect(page.GetByText("Uživatelské jméno musí mít alespoň 3 znaky")).ToBeVisibleAsync();
            await Expect(page.GetByText("Heslo musí mít alespoň 8 znaků")).ToBeVisibleAsync();
            await Expect(page.GetByText("Hesla se neshodují")).ToBeVisibleAsync();
            await Expect(page.GetByText("Musíte souhlasit s podmínkami")).ToBeVisibleAsync();
            await Expect(page.GetByText(new Regex("Validation\\.|RegisterModelValidator|\\{0\\}"))).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "auth",
                scenario: "register-validation-errors",
                state: "errors",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous");
        });
    }

    [Theory]
    [InlineData("", "Email je povinný")]
    [InlineData("spatny-email", "Zadejte platný email")]
    [InlineData("LONG_VALID_EMAIL", "Email může mít maximálně 256 znaků")]
    public async Task Register_EmailValidationEdgeCases_ShowSpecificLocalizedMessage(
        string email,
        string expectedMessage)
    {
        var scenario = $"register-email-validation-{Regex.Replace(expectedMessage.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-')}";

        await RunScenarioAsync("auth", scenario, async page =>
        {
            var unique = Guid.NewGuid().ToString("N")[..10];
            var testedEmail = email == "LONG_VALID_EMAIL"
                ? $"{new string('a', 250)}@lexiquest.test"
                : email;

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/register");

            if (!string.IsNullOrEmpty(testedEmail))
            {
                await page.GetByLabel("Email").FillAsync(testedEmail);
            }

            await page.GetByLabel("Uživatelské jméno").FillAsync($"email{unique}"[..14]);
            await page.GetByPlaceholder("Min. 8 znaků, 1 velké písmeno, 1 číslo").FillAsync("TestPass123!");
            await page.GetByLabel("Potvrdit heslo").FillAsync("TestPass123!");
            await page.GetByLabel("Souhlasím s podmínkami použití").CheckAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Vytvořit účet" }).ClickAsync();

            await Expect(page.GetByText(expectedMessage)).ToBeVisibleAsync();
            await Expect(page.GetByText(new Regex("Validation\\.|RegisterModelValidator|\\{0\\}"))).Not.ToBeVisibleAsync();
            page.Url.Should().Contain("/register");
        });
    }

    [Theory]
    [InlineData("", "TestPass123!", "Uživatelské jméno je povinné")]
    [InlineData("ččč", "TestPass123!", "Uživatelské jméno může obsahovat pouze písmena, číslice a podtržítka")]
    [InlineData("ab", "TestPass123!", "Uživatelské jméno musí mít alespoň 3 znaky")]
    [InlineData("abcdefghijklmnopqrstuvwxyzabcde", "TestPass123!", "Uživatelské jméno může mít maximálně 30 znaků")]
    [InlineData("validuser", "lowercase1!", "Heslo musí obsahovat velké písmeno")]
    [InlineData("validuser", "UPPERCASE1!", "Heslo musí obsahovat malé písmeno")]
    [InlineData("validuser", "BezCisla!", "Heslo musí obsahovat číslici")]
    [InlineData("validuser", "BezSpecial1", "Heslo musí obsahovat speciální znak")]
    public async Task Register_FieldValidationEdgeCases_ShowSpecificLocalizedMessage(
        string username,
        string password,
        string expectedMessage)
    {
        var scenario = $"register-validation-{Regex.Replace(expectedMessage.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-')}";

        await RunScenarioAsync("auth", scenario, async page =>
        {
            var unique = Guid.NewGuid().ToString("N")[..10];
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/register");

            await page.GetByLabel("Email").FillAsync($"edge.{unique}@lexiquest.test");
            await page.GetByLabel("Uživatelské jméno").FillAsync(username);
            await page.GetByPlaceholder("Min. 8 znaků, 1 velké písmeno, 1 číslo").FillAsync(password);
            await page.GetByLabel("Potvrdit heslo").FillAsync(password);
            await page.GetByLabel("Souhlasím s podmínkami použití").CheckAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Vytvořit účet" }).ClickAsync();

            await Expect(page.GetByText(expectedMessage)).ToBeVisibleAsync();
            await Expect(page.GetByText(new Regex("Validation\\.|RegisterModelValidator|\\{0\\}"))).Not.ToBeVisibleAsync();
            page.Url.Should().Contain("/register");
        });
    }

    [Theory]
    [InlineData("/login")]
    [InlineData("/register")]
    [InlineData("/password-reset")]
    [InlineData("/password-reset/neplatny-token-123")]
    public async Task AuthPages_RenderFocusedLayoutWithoutAppSidebar(string path)
    {
        var scenario = path.Trim('/').Replace("/", "-") + "-focused-layout";

        await RunScenarioAsync("auth", scenario, async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, path);

            await Expect(page.Locator(".app-layout")).Not.ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Přehled" })).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "auth",
                scenario: scenario,
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous");
        });
    }

    [Fact]
    public async Task Register_PasswordStrength_UsesLocalizedCzechTextWithoutKeys()
    {
        await RunScenarioAsync("auth", "register-password-strength-localized", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/register");

            await page.GetByPlaceholder("Min. 8 znaků, 1 velké písmeno, 1 číslo").FillAsync("TestPass123!");

            var strength = page.GetByTestId(Selectors.Auth.PasswordStrength);
            await Expect(strength).ToBeVisibleAsync();
            await Expect(strength).ToContainTextAsync("Síla hesla");
            await Expect(strength).ToContainTextAsync("Silné");
            await Expect(page.GetByText(new Regex("TmPasswordStrength|PasswordStrength_"))).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "auth",
                scenario: "register-password-strength-localized",
                state: "filled-password",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous");
        });
    }
}
