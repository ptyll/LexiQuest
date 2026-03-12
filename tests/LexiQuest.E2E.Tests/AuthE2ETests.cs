using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
public class AuthE2ETests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public AuthE2ETests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Register_Login_DashboardVisible()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/register");

        // Fill registration form
        var uniqueId = Guid.NewGuid().ToString("N");
        await page.FillAsync("[data-testid='email']", $"test{uniqueId}@test.com");
        await page.FillAsync("[data-testid='username']", $"user{uniqueId}"[..15]);
        await page.FillAsync("[data-testid='password']", "TestPass123!");
        await page.FillAsync("[data-testid='confirm-password']", "TestPass123!");
        await page.CheckAsync("[data-testid='accept-terms']");
        await page.ClickAsync("[data-testid='register-button']");

        // Should redirect to login or dashboard
        await page.WaitForURLAsync(url => url.Contains("/login") || url.Contains("/dashboard"));

        // If on login, log in
        if (page.Url.Contains("/login"))
        {
            await page.FillAsync("[data-testid='email']", $"test{uniqueId}@test.com");
            await page.FillAsync("[data-testid='password']", "TestPass123!");
            await page.ClickAsync("[data-testid='login-button']");
            await page.WaitForURLAsync("**/dashboard");
        }

        // Dashboard should be visible
        var dashboard = page.Locator(".dashboard-page, [data-testid='dashboard']");
        await Expect(dashboard).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShowsError()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/login");

        await page.FillAsync("[data-testid='email']", "nonexistent@test.com");
        await page.FillAsync("[data-testid='password']", "WrongPassword123!");
        await page.ClickAsync("[data-testid='login-button']");

        var errorMessage = page.Locator("[data-testid='error-message'], .error-message, .alert-danger");
        await Expect(errorMessage).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShowsError()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/register");

        // Try to register with a known existing email
        await page.FillAsync("[data-testid='email']", "existing@test.com");
        await page.FillAsync("[data-testid='username']", "existinguser");
        await page.FillAsync("[data-testid='password']", "TestPass123!");
        await page.FillAsync("[data-testid='confirm-password']", "TestPass123!");
        await page.CheckAsync("[data-testid='accept-terms']");
        await page.ClickAsync("[data-testid='register-button']");

        // Should show error or stay on register page
        await page.WaitForTimeoutAsync(2000);
        var errorVisible = await page.Locator("[data-testid='error-message'], .error-message, .alert-danger").IsVisibleAsync();
        var stillOnRegister = page.Url.Contains("/register");
        Assert.True(errorVisible || stillOnRegister, "Should show error or remain on register page");
    }
}
