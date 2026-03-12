using FluentAssertions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
public class ResponsiveE2ETests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public ResponsiveE2ETests(PlaywrightFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData(375, 812, "mobile")]
    [InlineData(768, 1024, "tablet")]
    [InlineData(1280, 720, "desktop")]
    public async Task LandingPage_AtViewport_LoadsWithoutErrors(int width, int height, string device)
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.SetViewportSizeAsync(width, height);

        var errors = new List<string>();
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error") errors.Add(msg.Text);
        };

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        errors.Should().BeEmpty($"landing page on {device} ({width}x{height}) should have no console errors");
    }

    [Theory]
    [InlineData(375, 812, "mobile")]
    [InlineData(768, 1024, "tablet")]
    [InlineData(1280, 720, "desktop")]
    public async Task LoginPage_AtViewport_IsUsable(int width, int height, string device)
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.SetViewportSizeAsync(width, height);
        await page.GotoAsync($"{_fixture.BaseUrl}/login");

        // Login form should be visible and usable at all viewport sizes
        var emailInput = page.Locator("[data-testid='email'], input[type='email']");
        await Expect(emailInput.First).ToBeVisibleAsync(new() { Timeout = 5000 });

        var passwordInput = page.Locator("[data-testid='password'], input[type='password']");
        await Expect(passwordInput.First).ToBeVisibleAsync();

        var loginButton = page.Locator("[data-testid='login-button'], button[type='submit']");
        await Expect(loginButton.First).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData(375, 812, "mobile")]
    [InlineData(768, 1024, "tablet")]
    [InlineData(1280, 720, "desktop")]
    public async Task RegisterPage_AtViewport_IsUsable(int width, int height, string device)
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.SetViewportSizeAsync(width, height);
        await page.GotoAsync($"{_fixture.BaseUrl}/register");

        var form = page.Locator("[data-testid='register-form'], form");
        await Expect(form.First).ToBeVisibleAsync(new() { Timeout = 5000 });

        var registerButton = page.Locator("[data-testid='register-button'], button[type='submit']");
        await Expect(registerButton.First).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData(375, 812, "mobile")]
    [InlineData(768, 1024, "tablet")]
    public async Task MobileNavigation_HasMenuToggle(int width, int height, string device)
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.SetViewportSizeAsync(width, height);
        await page.GotoAsync(_fixture.BaseUrl);

        // Mobile/tablet should have hamburger menu or similar toggle
        var menuToggle = page.Locator("[data-testid='menu-toggle'], .navbar-toggler, .hamburger, [data-testid='mobile-menu']");
        var isVisible = await menuToggle.First.IsVisibleAsync();

        // On mobile viewports, a menu toggle is expected
        isVisible.Should().BeTrue($"{device} viewport should have a navigation menu toggle");
    }

    [Fact]
    public async Task DesktopNavigation_HasFullMenu()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.SetViewportSizeAsync(1280, 720);
        await page.GotoAsync(_fixture.BaseUrl);

        // Desktop should have visible navigation links
        var nav = page.Locator("nav, [data-testid='navigation']");
        await Expect(nav.First).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Theory]
    [InlineData(375, 812, "mobile")]
    [InlineData(768, 1024, "tablet")]
    [InlineData(1280, 720, "desktop")]
    public async Task GuestGame_AtViewport_IsPlayable(int width, int height, string device)
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.SetViewportSizeAsync(width, height);
        await page.GotoAsync($"{_fixture.BaseUrl}/guest-game");

        var errors = new List<string>();
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error") errors.Add(msg.Text);
        };

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        errors.Should().BeEmpty($"guest game on {device} should have no console errors");
    }
}
