using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Trait("Category", "A11y")]
[Collection(E2ECollection.Name)]
public class AccessibilityPerformanceE2ETests : E2ETestBase
{
    private const string Area = "accessibility-performance";

    public AccessibilityPerformanceE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task A11y_MainRoutes_HaveLabelsMetadataAndNoBasicAuditIssues()
    {
        await RunScenarioAsync(Area, "main-routes-basic-a11y-audit", async page =>
        {
            var publicRoutes = new[]
            {
                (Route: "/", ReadySelector: "[data-testid='hero-section']"),
                (Route: "/login", ReadySelector: "main, body:has-text('Přihlášení')"),
                (Route: "/register", ReadySelector: "main, body:has-text('Vytvořit účet')"),
                (Route: "/play", ReadySelector: "[data-testid='guest-welcome']")
            };

            foreach (var route in publicRoutes)
            {
                await Fixture.GoToAndWaitForAppReadyAsync(page, route.Route);
                await Expect(page.Locator(route.ReadySelector).First).ToBeVisibleAsync(new() { Timeout = 10_000 });
                await Fixture.RunA11yCheckAsync(page);
                await AssertNoHorizontalOverflowAsync(page);
            }

            var admin = await Fixture.RegisterUniqueUserAsync("a11yroute");
            await Fixture.ForceAdminRoleAsync(admin.Email, AdminRole.Admin);
            await Fixture.LoginAsAsync(page, admin);

            var protectedRoutes = new[]
            {
                (Route: "/dashboard", ReadySelector: "[data-testid='dashboard-xp-progress'], .dashboard-page"),
                (Route: "/game", ReadySelector: "[data-testid='game-start-screen']"),
                (Route: "/settings", ReadySelector: "[data-testid='settings-page']"),
                (Route: "/admin/words", ReadySelector: "[data-testid='admin-words-page']"),
                (Route: "/multiplayer", ReadySelector: "[data-testid='multiplayer-page']")
            };

            foreach (var route in protectedRoutes)
            {
                await Fixture.GoToAndWaitForAppReadyAsync(page, route.Route);
                await Expect(page.Locator(route.ReadySelector).First).ToBeVisibleAsync(new() { Timeout = 10_000 });
                await Fixture.RunA11yCheckAsync(page);
                await AssertNoHorizontalOverflowAsync(page);
            }
        });
    }

    [Fact]
    public async Task A11y_TabOrder_ReachesPrimaryControlsAcrossCoreRoutes()
    {
        await RunScenarioAsync(Area, "tab-order-core-routes", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/");
            await AssertTabReachesAsync(page, "[data-testid='hero-cta-register'] button", "landing register CTA");

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/login");
            await AssertTabReachesAsync(page, "input[type='email']", "login email");

            var admin = await Fixture.RegisterUniqueUserAsync("a11ytab");
            await Fixture.ForceAdminRoleAsync(admin.Email, AdminRole.Admin);
            await Fixture.LoginAsAsync(page, admin);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");
            await UseSkipLinkAsync(page);
            await AssertTabReachesAsync(page, "[data-testid='game-mode-training']", "game training mode", resetFocus: false);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/settings");
            await UseSkipLinkAsync(page);
            await AssertTabReachesAsync(page, "[data-testid='username-input'] input", "settings username", resetFocus: false);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin/words");
            await UseSkipLinkAsync(page);
            await AssertTabReachesAsync(page, "[data-testid='admin-word-stats-open'] button", "admin word stats action", resetFocus: false);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/multiplayer");
            await UseSkipLinkAsync(page);
            await AssertTabReachesAsync(page, "[data-testid='multiplayer-quick-match-start']", "multiplayer quick match", resetFocus: false);
        });
    }

    [Fact]
    public async Task A11y_GameAndNotificationLiveRegions_AreAnnounced()
    {
        await RunScenarioAsync(Area, "live-regions-game-notifications", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("a11ylive");
            await Fixture.SeedNotificationAsync(
                user.Email,
                NotificationType.SystemMessage,
                "A11y upozornění",
                "Kontrola živého regionu",
                NotificationSeverity.Info);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            var badge = page.GetByTestId(Selectors.Notifications.UnreadBadge);
            await Expect(badge).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(badge).ToHaveAttributeAsync("aria-live", "polite");
            await Expect(badge).ToHaveAttributeAsync("aria-atomic", "true");

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");
            await page.GetByTestId(Selectors.Game.ModeTraining).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Expect(page.GetByTestId(Selectors.Game.Timer)).ToHaveAttributeAsync("role", "timer");
            await Expect(page.GetByTestId(Selectors.Game.Timer)).ToHaveAttributeAsync("aria-live", "polite");
            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync("spatnaodpoved");
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Game.Feedback)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Game.Feedback)).ToHaveAttributeAsync("role", "status");
            await Expect(page.GetByTestId(Selectors.Game.Feedback)).ToHaveAttributeAsync("aria-live", "assertive");
        });
    }

    [Fact]
    public async Task A11y_GameKeyboardOnly_AllowsAnswerWithEnter()
    {
        await RunScenarioAsync(Area, "keyboard-only-basic-game", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("a11ykeys");
            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.Training, DifficultyLevel.Beginner));

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/game/{game.SessionId}");

            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId(Selectors.Game.AnswerInput).FocusAsync();
            await page.Keyboard.TypeAsync("spatnaodpoved");
            await page.Keyboard.PressAsync("Enter");

            await Expect(page.GetByTestId(Selectors.Game.Feedback)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Game.Feedback)).ToContainTextAsync("Špatně");
        });
    }

    [Fact]
    public async Task A11y_ModalFocusTrap_KeepsKeyboardInsideDialog()
    {
        await RunScenarioAsync(Area, "modal-focus-trap", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("a11ymodal");
            await Fixture.ForceUserPremiumAsync(user.Email);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dictionaries");

            await page.GetByTestId(Selectors.Dictionaries.CreateButton).ClickAsync();
            var dialog = page.GetByTestId(Selectors.Dictionaries.CreateDialog);
            await Expect(dialog).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await page.GetByTestId(Selectors.Dictionaries.NameInput).FocusAsync();
            await page.Keyboard.PressAsync("Shift+Tab");
            (await IsActiveElementInsideAsync(dialog)).Should().BeTrue("Shift+Tab from the first modal field must stay inside the dialog");

            for (var i = 0; i < 10; i++)
            {
                await page.Keyboard.PressAsync("Tab");
                (await IsActiveElementInsideAsync(dialog)).Should().BeTrue($"Tab step {i + 1} should stay inside the dialog");
            }
        });
    }

    [Fact]
    public async Task A11y_MobileNavigation_MenuIsReachableAndNavigates()
    {
        await RunScenarioAsync(Area, "mobile-navigation-menu", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("a11ynav");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByTestId("mobile-nav-toggle")).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId("mobile-nav-toggle").ClickAsync();

            var menu = page.GetByTestId("mobile-nav-dialog");
            await Expect(menu).ToBeVisibleAsync();
            await Expect(menu.GetByRole(AriaRole.Link, new() { Name = "Nastavení" })).ToBeVisibleAsync();
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                Area,
                "mobile-navigation-menu",
                "menu-open",
                "375x812",
                "light",
                user.Username,
                fullPage: false,
                scrollToTop: false);

            await menu.GetByRole(AriaRole.Link, new() { Name = "Nastavení" }).ClickAsync();
            await page.WaitForURLAsync("**/settings");
            await Expect(page.GetByTestId(Selectors.Settings.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(menu).Not.ToBeVisibleAsync();
            await AssertNoHorizontalOverflowAsync(page);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                Area,
                "mobile-navigation-menu",
                "settings-after-menu-navigation",
                "375x812",
                "light",
                user.Username);
        }, width: 375, height: 812);
    }

    [Fact]
    public async Task Performance_LandingDashboardAndGame_LoadWithinSmokeBudget()
    {
        await RunScenarioAsync(Area, "performance-smoke-core-routes", async page =>
        {
            await AssertRouteLoadWithinAsync(page, "/", "[data-testid='hero-section']", TimeSpan.FromSeconds(8));

            var user = await Fixture.RegisterUniqueUserAsync("perf");
            await Fixture.LoginAsAsync(page, user);

            await AssertRouteLoadWithinAsync(page, "/dashboard", ".dashboard-page", TimeSpan.FromSeconds(10));
            await AssertRouteLoadWithinAsync(page, "/game", "[data-testid='game-start-screen']", TimeSpan.FromSeconds(10));

            var navigationTiming = await page.EvaluateAsync<NavigationTimingProbe>(
                """
                () => {
                    const nav = performance.getEntriesByType('navigation')[0];
                    return {
                        domContentLoadedMs: nav ? nav.domContentLoadedEventEnd : 0,
                        loadEventMs: nav ? nav.loadEventEnd : 0,
                        resourceCount: performance.getEntriesByType('resource').length
                    };
                }
                """);

            navigationTiming.DomContentLoadedMs.Should().BeLessThan(8_000);
            navigationTiming.ResourceCount.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task Pwa_StaticAssets_AreAvailableFromServiceWorkerCache()
    {
        await RunScenarioAsync(Area, "pwa-static-cache-smoke", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/");

            var probeJson = await page.EvaluateAsync<string>(
                """
                async () => {
                    if (!('serviceWorker' in navigator) || !('caches' in window)) {
                        return JSON.stringify({ supported: false, cacheNames: [], cachedAssets: {} });
                    }

                    await Promise.race([
                        navigator.serviceWorker.ready,
                        new Promise(resolve => setTimeout(resolve, 5000))
                    ]);

                    const cacheNames = await caches.keys();
                    const assets = ['/manifest.json', '/css/app.css', '/icon-192.png'];
                    const cachedAssets = {};
                    for (const asset of assets) {
                        cachedAssets[asset] = Boolean(await caches.match(asset));
                    }

                    return JSON.stringify({ supported: true, cacheNames, cachedAssets });
                }
                """);

            var probe = JsonSerializer.Deserialize<CacheProbe>(
                probeJson,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            probe.Should().NotBeNull();
            probe!.Supported.Should().BeTrue();
            probe.CacheNames.Should().Contain(name => name.StartsWith("lexiquest-static-", StringComparison.Ordinal));
            probe.CachedAssets["/manifest.json"].Should().BeTrue();
            probe.CachedAssets["/css/app.css"].Should().BeTrue();
            probe.CachedAssets["/icon-192.png"].Should().BeTrue();
        }, resetDatabase: false);
    }

    private static async Task AssertNoHorizontalOverflowAsync(IPage page)
    {
        var hasHorizontalOverflow = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");

        hasHorizontalOverflow.Should().BeFalse("a11y/responsive route should not create horizontal scrolling");
    }

    private static Task<bool> IsActiveElementInsideAsync(ILocator container)
    {
        return container.EvaluateAsync<bool>(
            "element => element === document.activeElement || element.contains(document.activeElement)");
    }

    private static async Task AssertTabReachesAsync(
        IPage page,
        string selector,
        string description,
        int maxTabs = 48,
        bool resetFocus = true)
    {
        if (resetFocus)
        {
            await page.EvaluateAsync(
                """
                () => {
                    document.body.setAttribute('tabindex', '-1');
                    document.body.focus();
                }
                """);
        }

        var target = page.Locator(selector).First;
        await Expect(target).ToBeVisibleAsync(new() { Timeout = 10_000 });

        for (var i = 0; i <= maxTabs; i++)
        {
            var reached = await target.EvaluateAsync<bool>(
                "target => target === document.activeElement || target.contains(document.activeElement)");

            if (reached)
            {
                return;
            }

            await page.Keyboard.PressAsync("Tab");
        }

        var activeDescription = await page.EvaluateAsync<string>(
            "() => document.activeElement ? `${document.activeElement.tagName} ${document.activeElement.textContent || document.activeElement.getAttribute('aria-label') || document.activeElement.getAttribute('placeholder') || ''}`.trim() : 'none'");
        false.Should().BeTrue($"Tab should reach {description}; active element was {activeDescription}");
    }

    private static async Task UseSkipLinkAsync(IPage page)
    {
        await page.EvaluateAsync(
            """
            () => {
                document.body.setAttribute('tabindex', '-1');
                document.body.focus();
            }
            """);

        await page.Keyboard.PressAsync("Tab");
        var skipLink = page.GetByTestId("skip-to-content");
        await Expect(skipLink).ToBeFocusedAsync();
        await Expect(skipLink).ToContainTextAsync("Přeskočit na obsah");

        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("#main-content")).ToBeFocusedAsync();
    }

    private async Task AssertRouteLoadWithinAsync(
        IPage page,
        string route,
        string readySelector,
        TimeSpan budget)
    {
        var stopwatch = Stopwatch.StartNew();
        await Fixture.GoToAndWaitForAppReadyAsync(page, route);
        await Expect(page.Locator(readySelector).First).ToBeVisibleAsync(new() { Timeout = 15_000 });
        stopwatch.Stop();

        stopwatch.Elapsed.Should().BeLessThan(budget, $"{route} should load within the smoke budget");
    }

    private sealed class NavigationTimingProbe
    {
        public double DomContentLoadedMs { get; set; }

        public double LoadEventMs { get; set; }

        public int ResourceCount { get; set; }
    }

    private sealed class CacheProbe
    {
        public bool Supported { get; set; }

        public List<string> CacheNames { get; set; } = [];

        public Dictionary<string, bool> CachedAssets { get; set; } = [];
    }
}
