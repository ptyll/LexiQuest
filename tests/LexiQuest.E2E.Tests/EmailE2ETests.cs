using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.Data.SqlClient;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Email")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class EmailE2ETests : E2ETestBase
{
    public EmailE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task PasswordReset_ExistingUser_SendsCzechResetEmailToSmtp4Dev()
    {
        await RunScenarioAsync("email", "password-reset-existing-user", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("reset");

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/password-reset");
            await page.GetByLabel("Email").FillAsync(user.Email);
            await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Odeslat odkaz" }).ClickAsync();

            await Expect(page.GetByText("Pokud je email registrován, poslali jsme vám odkaz pro obnovení hesla."))
                .ToBeVisibleAsync();

            var message = await Fixture.Smtp4Dev.WaitForMessageTextAsync(raw =>
                raw.Contains(user.Email, StringComparison.OrdinalIgnoreCase)
                && raw.Contains("Obnovení hesla - LexiQuest", StringComparison.OrdinalIgnoreCase)
                && raw.Contains("/password-reset/", StringComparison.OrdinalIgnoreCase));

            message.Should().Contain(user.Email);
            message.Should().Contain("noreply@lexiquest.test");
            message.Should().Contain("Obnovení hesla - LexiQuest");
            message.Should().Contain("Obnovit heslo");
            message.Should().Contain(Fixture.WebBaseUrl);
            message.Should().Contain("/password-reset/");
            message.Should().NotContain("Password Reset");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "email",
                scenario: "password-reset-existing-user",
                state: "success",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Register_NewUser_SendsCzechWelcomeEmailToSmtp4Dev()
    {
        await RunScenarioAsync("email", "register-welcome-email", async page =>
        {
            var unique = Guid.NewGuid().ToString("N")[..10];
            var email = $"welcome.{unique}@lexiquest.test";
            var username = $"welcome{unique}";

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/register");
            await page.GetByLabel("Email").FillAsync(email);
            await page.GetByLabel("Uživatelské jméno").FillAsync(username);
            await page.GetByPlaceholder("Min. 8 znaků, 1 velké písmeno, 1 číslo").FillAsync("TestPass123!");
            await page.GetByLabel("Potvrdit heslo").FillAsync("TestPass123!");
            await page.GetByLabel("Souhlasím s podmínkami použití").CheckAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Vytvořit účet" }).ClickAsync();

            await page.WaitForURLAsync("**/dashboard");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přehled" })).ToBeVisibleAsync();

            var message = await Fixture.Smtp4Dev.WaitForMessageTextAsync(raw =>
                raw.Contains(email, StringComparison.OrdinalIgnoreCase)
                && raw.Contains("Vítej v LexiQuestu!", StringComparison.OrdinalIgnoreCase)
                && raw.Contains(username, StringComparison.OrdinalIgnoreCase));

            message.Should().Contain(email);
            message.Should().Contain("noreply@lexiquest.test");
            message.Should().Contain("Vítej v LexiQuestu!");
            message.Should().Contain($"Ahoj {username}!");
            WebUtility.HtmlDecode(message).Should().Contain("Začít hrát");
            message.Should().Contain(Fixture.WebBaseUrl);
            message.Should().NotContain("Welcome.Subject");
            message.Should().NotContain("Welcome.Body");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "email",
                scenario: "register-welcome-email",
                state: "dashboard-after-register",
                viewport: "1366x900",
                theme: "light",
                persona: "newRegisteredUser");
        });
    }

    [Fact]
    public async Task PasswordReset_LinkChangesPasswordAndOldPasswordStopsWorking()
    {
        await RunScenarioAsync("email", "password-reset-complete-flow", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("resetflow");
            var newPassword = "NoveHeslo123!";

            var resetUrl = await RequestPasswordResetUrlAsync(page, user);

            await SubmitNewPasswordAsync(page, resetUrl, newPassword);
            await ExpectPasswordResetSuccessAsync(page);

            await AssertLoginFailsAsync(user.Email, user.Password);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/login");
            await page.GetByLabel("Email").FillAsync(user.Email);
            await page.GetByLabel("Heslo").FillAsync(newPassword);
            await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Přihlásit se" }).ClickAsync();

            await page.WaitForURLAsync("**/dashboard");
            await Expect(page.GetByRole(Microsoft.Playwright.AriaRole.Heading, new() { Name = "Přehled" }))
                .ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Dashboard.TotalXpStat)).ToContainTextAsync("Celkové XP");
            await Expect(page.GetByTestId(Selectors.Dashboard.XpBarLevel)).ToHaveTextAsync("Úroveň 1");
            await Expect(page.GetByText("Denní výzva")).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "email",
                scenario: "password-reset-complete-flow",
                state: "dashboard-after-reset",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task PasswordReset_UsedToken_ShowsLocalizedError()
    {
        await RunScenarioAsync("email", "password-reset-used-token", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("usedtoken");
            var resetUrl = await RequestPasswordResetUrlAsync(page, user);

            await SubmitNewPasswordAsync(page, resetUrl, "PouziteHeslo123!");
            await ExpectPasswordResetSuccessAsync(page);

            await SubmitNewPasswordAsync(page, resetUrl, "DalsiHeslo123!");

            await Expect(page.GetByText("Odkaz pro obnovení hesla již byl použit.")).ToBeVisibleAsync();
            await Expect(page.GetByText(new Regex("Error\\.UsedToken|Token\\.Used|PasswordResetService"))).Not.ToBeVisibleAsync();
        }, assertNoConsoleErrors: false);
    }

    [Fact]
    public async Task PasswordReset_InvalidToken_ShowsLocalizedError()
    {
        await RunScenarioAsync("email", "password-reset-invalid-token", async page =>
        {
            await SubmitNewPasswordAsync(page, "/password-reset/neplatny-token-123", "NoveHeslo123!");

            await Expect(page.GetByText("Odkaz pro obnovení hesla je neplatný nebo vypršel.")).ToBeVisibleAsync();
            await Expect(page.GetByText(new Regex("Error\\.InvalidToken|Token\\.Invalid|PasswordResetService"))).Not.ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task PasswordReset_ExpiredToken_ShowsLocalizedError()
    {
        await RunScenarioAsync("email", "password-reset-expired-token", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("expiredtoken");
            var resetUrl = await RequestPasswordResetUrlAsync(page, user);
            await ExpirePasswordResetTokenAsync(ExtractToken(resetUrl));

            await SubmitNewPasswordAsync(page, resetUrl, "NoveHeslo123!");

            await Expect(page.GetByText("Odkaz pro obnovení hesla vypršel.")).ToBeVisibleAsync();
            await Expect(page.GetByText(new Regex("Error\\.ExpiredToken|Token\\.Expired|PasswordResetService"))).Not.ToBeVisibleAsync();
        }, assertNoConsoleErrors: false);
    }

    [Fact]
    public async Task PasswordReset_NewPasswordSameAsOld_ShowsSpecificLocalizedError()
    {
        await RunScenarioAsync("email", "password-reset-same-as-old", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("sameold");
            var resetUrl = await RequestPasswordResetUrlAsync(page, user);

            await SubmitNewPasswordAsync(page, resetUrl, user.Password);

            await Expect(page.GetByText("Nové heslo nesmí být stejné jako staré.")).ToBeVisibleAsync();
            await Expect(page.GetByText(new Regex("Error\\.SamePassword|Password\\.SameAsOld|PasswordResetService"))).Not.ToBeVisibleAsync();
        }, assertNoConsoleErrors: false);
    }

    private async Task<string> RequestPasswordResetUrlAsync(IPage page, TestUser user)
    {
        await Fixture.GoToAndWaitForAppReadyAsync(page, "/password-reset");
        await page.GetByLabel("Email").FillAsync(user.Email);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Odeslat odkaz" }).ClickAsync();

        await Expect(page.GetByText("Pokud je email registrován, poslali jsme vám odkaz pro obnovení hesla."))
            .ToBeVisibleAsync();

        var message = await Fixture.Smtp4Dev.WaitForMessageTextAsync(raw =>
            raw.Contains(user.Email, StringComparison.OrdinalIgnoreCase)
            && raw.Contains("/password-reset/", StringComparison.OrdinalIgnoreCase));

        return ExtractResetUrl(message);
    }

    private async Task SubmitNewPasswordAsync(IPage page, string resetUrl, string password)
    {
        await Fixture.GoToAndWaitForAppReadyAsync(page, resetUrl);
        await Expect(page.GetByRole(Microsoft.Playwright.AriaRole.Heading, new() { Name = "Nové heslo" }))
            .ToBeVisibleAsync();

        await page.GetByLabel("Nové heslo").FillAsync(password);
        await page.GetByLabel("Potvrzení hesla").FillAsync(password);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Nastavit nové heslo" }).ClickAsync();
    }

    private static async Task ExpectPasswordResetSuccessAsync(IPage page)
    {
        await Expect(page.GetByText("Heslo bylo úspěšně změněno. Nyní se můžete přihlásit."))
            .ToBeVisibleAsync();
    }

    private string ExtractResetUrl(string message)
    {
        var match = Regex.Match(
            message,
            $@"{Regex.Escape(Fixture.WebBaseUrl)}/password-reset/[a-f0-9]{{64}}",
            RegexOptions.IgnoreCase);

        match.Success.Should().BeTrue("password reset email must contain a reset URL for the E2E web host");
        return match.Value;
    }

    private static string ExtractToken(string resetUrl)
    {
        var token = new Uri(resetUrl).Segments.Last().Trim('/');
        token.Should().MatchRegex("^[a-f0-9]{64}$");
        return token;
    }

    private async Task ExpirePasswordResetTokenAsync(string token)
    {
        await using var connection = new SqlConnection(Fixture.DatabaseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE PasswordResetTokens
            SET ExpiresAt = DATEADD(minute, -5, SYSUTCDATETIME())
            WHERE Token = @token
            """;
        command.Parameters.AddWithValue("@token", token);

        var affectedRows = await command.ExecuteNonQueryAsync();
        affectedRows.Should().Be(1);
    }

    private async Task AssertLoginFailsAsync(string email, string password)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{Fixture.ApiBaseUrl}/") };
        using var response = await httpClient.PostAsJsonAsync("api/v1/users/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
