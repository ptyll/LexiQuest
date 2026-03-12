using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
public class LandingE2ETests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public LandingE2ETests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task LandingPage_LoadsSuccessfully()
    {
        var page = await _fixture.Browser.NewPageAsync();
        var response = await page.GotoAsync(_fixture.BaseUrl);

        Assert.NotNull(response);
        Assert.True(response.Ok, $"Landing page returned {response.Status}");
    }

    [Fact]
    public async Task LandingPage_HasHeroSection()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);

        var hero = page.Locator("[data-testid='hero-section'], .hero-section, .hero");
        await Expect(hero).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Fact]
    public async Task LandingPage_HasFeaturesSection()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);

        var features = page.Locator("[data-testid='features-section'], .features-section, .features");
        await Expect(features).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Fact]
    public async Task LandingPage_CTANavigatesToRegister()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);

        // Click main CTA button
        var ctaButton = page.Locator("[data-testid='cta-register'], [data-testid='hero-cta'], a[href*='register'].cta, .hero a[href*='register']");
        await Expect(ctaButton.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        await ctaButton.First.ClickAsync();

        await page.WaitForURLAsync("**/register");
        Assert.Contains("/register", page.Url);
    }

    [Fact]
    public async Task LandingPage_HasLoginLink()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);

        var loginLink = page.Locator("a[href*='login'], [data-testid='login-link']");
        await Expect(loginLink.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        await loginLink.First.ClickAsync();

        await page.WaitForURLAsync("**/login");
        Assert.Contains("/login", page.Url);
    }

    [Fact]
    public async Task LandingPage_HasGuestPlayOption()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);

        var guestOption = page.Locator("[data-testid='guest-play'], [data-testid='try-free'], a[href*='guest']");
        await Expect(guestOption.First).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Fact]
    public async Task LandingPage_HasFooter()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);

        var footer = page.Locator("footer, [data-testid='footer'], .footer");
        await Expect(footer).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Fact]
    public async Task LandingPage_NoConsoleErrors()
    {
        var page = await _fixture.Browser.NewPageAsync();
        var consoleErrors = new List<string>();
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                consoleErrors.Add(msg.Text);
        };

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Empty(consoleErrors);
    }
}
