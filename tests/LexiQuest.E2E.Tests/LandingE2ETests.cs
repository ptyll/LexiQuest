using FluentAssertions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Smoke")]
[Trait("Category", "Visual")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class LandingE2ETests : E2ETestBase
{
    public LandingE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task LandingPage_LoadsAllPrimarySections_AndStoresUxCheckpoint()
    {
        await RunScenarioAsync("landing", "primary-sections", async page =>
        {
            var response = await Fixture.GoToAndWaitForAppReadyAsync(page);

            response.Should().NotBeNull();
            response!.Ok.Should().BeTrue($"landing page returned {response.Status}");

            await Expect(page.GetByTestId(Selectors.Landing.Hero)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Landing.HowItWorks)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Landing.Features)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Landing.Paths)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Landing.Testimonials)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Landing.Cta)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Landing.Footer)).ToBeVisibleAsync();
            await Expect(page.GetByText("Rozlušti slova, získej moc")).ToBeVisibleAsync();
            await Expect(page.GetByText("Jak to funguje?")).ToBeVisibleAsync();
            await Expect(page.GetByText("Proč hrát LexiQuest?")).ToBeVisibleAsync();
            await Expect(page.GetByText("Připraven začít?")).ToBeVisibleAsync();
            await Expect(page.GetByText("Hero.Tagline")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("HowItWorks.Title")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("CTA.Button")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("Footer.About")).Not.ToBeVisibleAsync();

            await Fixture.RunA11yCheckAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "landing",
                scenario: "primary-sections",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous");
        }, resetDatabase: false);
    }

    [Fact]
    public async Task LandingPage_RegisterCta_NavigatesToRegister()
    {
        await RunScenarioAsync("landing", "register-cta", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page);

            await page.GetByTestId(Selectors.Landing.RegisterCta).Locator("button").ClickAsync();

            await page.WaitForURLAsync("**/register");
            page.Url.Should().Contain("/register");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Vytvořit účet" })).ToBeVisibleAsync();
        }, resetDatabase: false);
    }

    [Fact]
    public async Task LandingPage_FeatureTabs_RenderAllPanels_AndStoreUxCheckpoints()
    {
        await RunScenarioAsync("landing", "feature-tabs", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page);
            await page.GetByTestId(Selectors.Landing.Features).ScrollIntoViewIfNeededAsync();

            await Expect(page.GetByTestId(Selectors.Landing.FeatureTabRpg)).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Tab, new() { Name = "RPG postup" })).ToBeVisibleAsync();
            await Expect(page.GetByText("RPG Progress")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("Boss Battles")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("Competitions")).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "landing",
                scenario: "feature-tabs",
                state: "rpg",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous",
                fullPage: false,
                scrollToTop: false);

            await page.GetByRole(AriaRole.Tab, new() { Name = "Souboje" }).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Landing.FeatureTabBattles)).ToBeVisibleAsync();
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "landing",
                scenario: "feature-tabs",
                state: "souboje",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous",
                fullPage: false,
                scrollToTop: false);

            await page.GetByRole(AriaRole.Tab, new() { Name = "Soutěže" }).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Landing.FeatureTabCompetitions)).ToBeVisibleAsync();
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "landing",
                scenario: "feature-tabs",
                state: "souteze",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous",
                fullPage: false,
                scrollToTop: false);
        }, resetDatabase: false);
    }

    [Fact]
    public async Task LandingPage_GuestCta_NavigatesToGuestPlay()
    {
        await RunScenarioAsync("landing", "guest-cta", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page);

            await page.GetByTestId(Selectors.Landing.GuestCta).Locator("button").ClickAsync();

            await Expect(page.GetByTestId(Selectors.Guest.Welcome)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            page.Url.Should().Contain("/play");
        });
    }

    [Theory]
    [InlineData("footer-about", "/about", "O LexiQuest", "About_Title")]
    [InlineData("footer-terms", "/terms", "Podmínky používání", "Terms_Title")]
    [InlineData("footer-privacy", "/privacy", "Ochrana soukromí", "Privacy_Title")]
    [InlineData("footer-contact", "/contact", "Kontakt", "Contact_Title")]
    public async Task LandingPage_FooterLink_NavigatesToExpectedRoute(
        string testId,
        string expectedRoute,
        string expectedHeading,
        string unexpectedLocalizationKey)
    {
        await RunScenarioAsync("landing", $"footer-{testId}", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page);

            var navigation = page.WaitForURLAsync($"**{expectedRoute}");
            await page.GetByTestId(testId).ClickAsync();

            await navigation;
            page.Url.Should().Contain(expectedRoute);
            page.Url.Should().NotContain("/login");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = expectedHeading })).ToBeVisibleAsync();
            await Expect(page.GetByText(unexpectedLocalizationKey)).Not.ToBeVisibleAsync();
        }, resetDatabase: false);
    }

    [Fact]
    public async Task LandingPage_SeoMetadata_IsAvailable()
    {
        await RunScenarioAsync("landing", "seo-metadata", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page);

            var description = await page.Locator("meta[name='description']").GetAttributeAsync("content");
            var ogTitle = await page.Locator("meta[property='og:title']").GetAttributeAsync("content");
            var jsonLdCount = await page.Locator("script[type='application/ld+json']").CountAsync();

            description.Should().NotBeNullOrWhiteSpace();
            description.Should().Contain("LexiQuest");
            ogTitle.Should().Contain("LexiQuest");
            jsonLdCount.Should().BeGreaterThan(0);
        }, resetDatabase: false);
    }

    [Fact]
    public async Task UnknownPublicRoute_ShowsLocalizedNotFoundPage()
    {
        await RunScenarioAsync("routing", "not-found", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/neexistujici-stranka-{Guid.NewGuid():N}");

            page.Url.Should().NotContain("/login");
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Stránka nenalezena" })).ToBeVisibleAsync();
            await Expect(page.GetByText("Hledaná stránka neexistuje.")).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Zpět na úvod" })).ToBeVisibleAsync();
            await Expect(page.GetByText("Stránka nebyla nalezena.")).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "routing",
                scenario: "not-found",
                state: "localized-404",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous");
        }, resetDatabase: false, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }
}
