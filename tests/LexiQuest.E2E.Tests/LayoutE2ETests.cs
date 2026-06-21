using FluentAssertions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class LayoutE2ETests : E2ETestBase
{
    public LayoutE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Layout_DashboardSidebar_IsStyledAsNavigation()
    {
        await RunScenarioAsync("layout", "dashboard-sidebar-styled", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("layoutnav");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            var sidebar = page.GetByTestId(Selectors.Layout.Sidebar);
            await Expect(sidebar).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var firstItem = sidebar.Locator("a").First;
            await Expect(firstItem).ToBeVisibleAsync();

            var style = await firstItem.EvaluateAsync<SidebarItemStyle>(
                """
                element => {
                    const computed = window.getComputedStyle(element);
                    return {
                        display: computed.display,
                        textDecorationLine: computed.textDecorationLine,
                        borderRadius: computed.borderRadius,
                        color: computed.color
                    };
                }
                """);

            style.Display.Should().Be("flex");
            style.TextDecorationLine.Should().NotContain("underline");
            style.BorderRadius.Should().NotBe("0px");
            style.Color.Should().NotBe("rgb(0, 0, 238)");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "layout",
                scenario: "dashboard-sidebar-styled",
                state: "desktop",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Layout_GlobalErrorBoundary_ShowsCzechFallbackAndRetry()
    {
        await RunScenarioAsync("layout", "global-error-boundary-retry", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("layouterror");
            var token = Guid.NewGuid().ToString("N");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/e2e/client-error?throw=1&token={token}");

            await Expect(page.GetByTestId("global-error-boundary")).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Nastala chyba" })).ToBeVisibleAsync();
            await Expect(page.GetByText("Omlouváme se, ale něco se pokazilo. Zkuste to prosím znovu.")).ToBeVisibleAsync();
            await Expect(page.GetByTestId("global-error-boundary-retry")).ToBeVisibleAsync();
            await Expect(page.GetByText("An unhandled error has occurred.")).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "layout",
                scenario: "global-error-boundary-retry",
                state: "fallback",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");

            await page.GetByTestId("global-error-boundary-retry").ClickAsync();
            await Expect(page.GetByTestId("global-error-boundary")).Not.ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId("e2e-client-error-recovered")).ToHaveCountAsync(1);
        }, assertNoConsoleErrors: false);
    }

    [Fact]
    public async Task Layout_MainPages_ShowSkeletonLoadingStates()
    {
        await RunScenarioAsync("layout", "main-pages-loading-states", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("layoutload");
            await Fixture.LoginAsAsync(page, user);

            await AssertLoadingCheckpointAsync(
                page,
                path: "/dashboard",
                apiPath: "/api/v1/stats/user",
                testId: Selectors.Dashboard.Skeleton,
                state: "dashboard",
                requireAriaBusy: true);

            await AssertLoadingCheckpointAsync(
                page,
                path: "/settings",
                apiPath: "/api/v1/users/me",
                testId: Selectors.Settings.Loading,
                state: "settings",
                requireAriaBusy: true);

            await AssertLoadingCheckpointAsync(
                page,
                path: "/profile",
                apiPath: "/api/v1/users/me",
                testId: Selectors.Profile.Loading,
                state: "profile");

            await AssertLoadingCheckpointAsync(
                page,
                path: "/team",
                apiPath: "/api/v1/teams/my",
                testId: Selectors.Teams.Loading,
                state: "team");

            await AssertLoadingCheckpointAsync(
                page,
                path: "/paths",
                apiPath: "/api/v1/paths",
                testId: "paths-loading",
                state: "paths");

            await AssertLoadingCheckpointAsync(
                page,
                path: "/ai-challenge",
                apiPath: "/api/v1/challenges/ai/analysis",
                testId: Selectors.AIChallenge.Loading,
                state: "ai-challenge");
        });
    }

    [Fact]
    public async Task Layout_Toasts_RenderSuccessErrorWarningAndInfoVariants()
    {
        await RunScenarioAsync("layout", "toast-variants", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("layouttoast");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/e2e/toasts?showAll=true");

            var toasts = page.Locator(".tm-toast");
            await Expect(toasts).ToHaveCountAsync(4, new() { Timeout = 10_000 });
            await Expect(toasts.Filter(new() { HasTextString = "Úspěšně uloženo" })).ToBeVisibleAsync();
            await Expect(toasts.Filter(new() { HasTextString = "Akci se nepodařilo dokončit" })).ToBeVisibleAsync();
            await Expect(toasts.Filter(new() { HasTextString = "Zkontrolujte zadané údaje" })).ToBeVisibleAsync();
            await Expect(toasts.Filter(new() { HasTextString = "Nové informace jsou připravené" })).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "layout",
                scenario: "toast-variants",
                state: "all-variants",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    private sealed class SidebarItemStyle
    {
        public string Display { get; set; } = string.Empty;
        public string TextDecorationLine { get; set; } = string.Empty;
        public string BorderRadius { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    private async Task AssertLoadingCheckpointAsync(
        IPage page,
        string path,
        string apiPath,
        string testId,
        string state,
        bool requireAriaBusy = false)
    {
        await Fixture.DelayNextApiRequestAsync(apiPath);

        try
        {
            await page.GotoAsync($"{Fixture.WebBaseUrl}{path}", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            var loading = page.GetByTestId(testId);
            await Expect(loading).ToBeVisibleAsync(new() { Timeout = 10_000 });
            if (requireAriaBusy)
            {
                await Expect(loading).ToHaveAttributeAsync("aria-busy", "true");
            }

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "layout",
                scenario: "main-pages-loading-states",
                state: state,
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        }
        finally
        {
            await Fixture.ReleaseDelayedApiRequestsAsync();
        }

        await Fixture.WaitForNoBusyIndicatorsAsync(page);
    }
}
