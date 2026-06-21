using FluentAssertions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Visual")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class ResponsiveE2ETests : E2ETestBase
{
    public ResponsiveE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    public static TheoryData<int, int, string> Viewports => new()
    {
        { 375, 812, "mobile" },
        { 768, 1024, "tablet" },
        { 1366, 900, "desktop" },
        { 1920, 1080, "wide-desktop" }
    };

    [Theory]
    [MemberData(nameof(Viewports))]
    public async Task LandingPage_Viewport_HasNoHorizontalOverflow(int width, int height, string viewport)
    {
        await RunScenarioAsync("responsive", $"landing-{viewport}", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page);

            await Expect(page.GetByTestId(Selectors.Landing.Hero)).ToBeVisibleAsync();
            await AssertNoHorizontalOverflowAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "responsive",
                scenario: "landing",
                state: "loaded",
                viewport: $"{width}x{height}",
                theme: "light",
                persona: "anonymous");
        }, width, height, resetDatabase: false);
    }

    [Theory]
    [MemberData(nameof(Viewports))]
    public async Task LoginPage_Viewport_IsUsable(int width, int height, string viewport)
    {
        await RunScenarioAsync("responsive", $"login-{viewport}", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/login");

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přihlášení" })).ToBeVisibleAsync();
            await Expect(page.GetByLabel("Email")).ToBeVisibleAsync();
            await Expect(page.GetByLabel("Heslo")).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Přihlásit se" })).ToBeVisibleAsync();
            await AssertNoHorizontalOverflowAsync(page);
        }, width, height);
    }

    [Theory]
    [MemberData(nameof(Viewports))]
    public async Task RegisterPage_Viewport_IsUsable(int width, int height, string viewport)
    {
        await RunScenarioAsync("responsive", $"register-{viewport}", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/register");

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Vytvořit účet" })).ToBeVisibleAsync();
            await Expect(page.GetByLabel("Email")).ToBeVisibleAsync();
            await Expect(page.GetByLabel("Uživatelské jméno")).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Vytvořit účet" })).ToBeVisibleAsync();
            await AssertNoHorizontalOverflowAsync(page);
        }, width, height);
    }

    [Theory]
    [MemberData(nameof(Viewports))]
    public async Task GuestPlay_Viewport_WelcomeIsUsable(int width, int height, string viewport)
    {
        await RunScenarioAsync("responsive", $"guest-{viewport}", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/play");

            await Expect(page.GetByTestId(Selectors.Guest.Welcome)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Guest.StartButton)).ToBeVisibleAsync();
            await AssertNoHorizontalOverflowAsync(page);
        }, width, height);
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public async Task LandingPage_Theme_LoadsWithoutBrokenLayout(string theme)
    {
        await RunScenarioAsync("responsive", $"landing-theme-{theme}", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page);

            await Expect(page.GetByTestId(Selectors.Landing.Hero)).ToBeVisibleAsync();
            await AssertNoHorizontalOverflowAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "responsive",
                scenario: "landing-theme",
                state: "loaded",
                viewport: "1366x900",
                theme: theme,
                persona: "anonymous");
        }, theme: theme, resetDatabase: false);
    }

    private static async Task AssertNoHorizontalOverflowAsync(IPage page)
    {
        var hasHorizontalOverflow = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");

        hasHorizontalOverflow.Should().BeFalse("viewport should not have unintended horizontal scrolling");
    }
}
