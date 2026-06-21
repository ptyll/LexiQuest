using System.Collections.Concurrent;
using FluentAssertions;
using LexiQuest.Shared.Enums;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class SelectorAuditE2ETests : E2ETestBase
{
    private const string Area = "selector-audit";

    public SelectorAuditE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task DataTestIds_MainRoutesAndPrimaryComponents_ExposeStableSelectors()
    {
        await RunScenarioAsync(Area, "main-routes-primary-components", async page =>
        {
            var httpErrors = new ConcurrentBag<string>();
            page.Response += (_, response) =>
            {
                if (response.Status >= 400)
                {
                    httpErrors.Add($"{response.Status} {response.Url}");
                }
            };

            var publicRoutes = new[]
            {
                Audit(
                    "/",
                    Selectors.Landing.Page,
                    Selectors.Landing.Hero,
                    Selectors.Landing.Features,
                    Selectors.Landing.Paths,
                    Selectors.Landing.Cta,
                    Selectors.Landing.Footer),
                Audit(
                    "/login",
                    Selectors.Auth.LoginPage,
                    Selectors.Auth.LoginForm,
                    Selectors.Auth.LoginSubmit,
                    Selectors.Auth.LoginForgotPassword),
                Audit(
                    "/register",
                    Selectors.Auth.RegisterPage,
                    Selectors.Auth.RegisterForm,
                    Selectors.Auth.RegisterSubmit),
                Audit(
                    "/password-reset",
                    Selectors.Auth.PasswordResetRequestPage,
                    Selectors.Auth.PasswordResetRequestForm,
                    Selectors.Auth.PasswordResetRequestSubmit),
                Audit(
                    "/password-reset/selector-audit-token",
                    Selectors.Auth.PasswordResetConfirmPage,
                    Selectors.Auth.PasswordResetConfirmForm,
                    Selectors.Auth.PasswordResetConfirmSubmit),
                Audit(
                    "/play",
                    Selectors.Guest.Page,
                    Selectors.Guest.Welcome,
                    Selectors.Guest.StartButton)
            };

            foreach (var route in publicRoutes)
            {
                await AssertRouteSelectorsAsync(page, route);
            }

            var admin = await Fixture.RegisterUniqueUserAsync("selectoraudit");
            await Fixture.ForceAdminRoleAsync(admin.Email, AdminRole.Admin);
            await Fixture.LoginAsAsync(page, admin);

            var protectedRoutes = new[]
            {
                Audit(
                    "/dashboard",
                    Selectors.Dashboard.Page,
                    Selectors.Dashboard.XpProgress,
                    Selectors.Dashboard.StreakIndicator),
                Audit(
                    "/game",
                    Selectors.Game.StartScreen,
                    Selectors.Game.ModeTraining,
                    Selectors.Game.ModeTimeAttack),
                Audit(
                    "/paths",
                    Selectors.Paths.Page,
                    Selectors.Paths.Card,
                    Selectors.Paths.BeginnerCard),
                Audit(
                    "/leagues",
                    Selectors.Leagues.Page,
                    Selectors.Leagues.Tier,
                    Selectors.Leagues.UserPosition,
                    Selectors.Leagues.Leaderboard),
                Audit(
                    "/daily-challenge",
                    Selectors.Daily.Page,
                    Selectors.Daily.ChallengeCard,
                    Selectors.Daily.Leaderboard),
                Audit(
                    "/achievements",
                    Selectors.Achievements.Page,
                    Selectors.Achievements.Progress,
                    Selectors.Achievements.Tabs,
                    Selectors.Achievements.Grid),
                Audit(
                    "/settings",
                    Selectors.Settings.Page,
                    Selectors.Settings.ProfileSection,
                    Selectors.Settings.PasswordSection,
                    Selectors.Settings.PreferencesSection,
                    Selectors.Settings.PrivacySection,
                    Selectors.Settings.DangerZone),
                Audit(
                    "/dictionaries",
                    Selectors.Dictionaries.Page,
                    Selectors.Dictionaries.PremiumGate),
                Audit(
                    "/team",
                    Selectors.Teams.Page,
                    Selectors.Teams.EmptyState,
                    Selectors.Teams.CreateTeam,
                    Selectors.Teams.SearchTeam),
                Audit(
                    "/premium",
                    Selectors.Premium.Page,
                    Selectors.Premium.FeatureAvailability,
                    Selectors.Premium.PricingGrid),
                Audit(
                    "/shop",
                    Selectors.Shop.Page,
                    Selectors.Shop.CoinBalance,
                    Selectors.Shop.Tabs),
                Audit(
                    "/ai-challenge",
                    Selectors.AIChallenge.Page),
                Audit(
                    "/multiplayer",
                    Selectors.Multiplayer.Page,
                    Selectors.Multiplayer.QuickMatchCard,
                    Selectors.Multiplayer.PrivateRoomCard,
                    Selectors.Multiplayer.MatchHistory),
                Audit(
                    "/multiplayer/quick-match",
                    Selectors.Multiplayer.QuickMatchPage,
                    Selectors.Multiplayer.Searching),
                Audit(
                    "/multiplayer/history",
                    Selectors.Multiplayer.HistoryPage),
                Audit(
                    "/admin",
                    Selectors.Admin.DashboardPage,
                    Selectors.Admin.DashboardStats,
                    Selectors.Admin.LinkWords,
                    Selectors.Admin.LinkUsers),
                Audit(
                    "/admin/words",
                    Selectors.AdminWords.Page,
                    Selectors.AdminWords.Filters,
                    Selectors.AdminWords.Table,
                    Selectors.AdminWords.CreateOpen,
                    Selectors.AdminWords.ImportOpen),
                Audit(
                    "/admin/users",
                    Selectors.AdminUsers.Page,
                    Selectors.AdminUsers.Filters,
                    Selectors.AdminUsers.Table)
            };

            foreach (var route in protectedRoutes)
            {
                await AssertRouteSelectorsAsync(page, route);
            }

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/paths");
            await page.GetByTestId(Selectors.Paths.BeginnerCard).GetByRole(AriaRole.Button).ClickAsync();
            await page.WaitForURLAsync("**/paths/*");
            await Expect(page.GetByTestId(Selectors.Paths.DetailPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Paths.Map)).ToBeVisibleAsync();

            httpErrors.Should().BeEmpty("main selector audit routes should not request missing or rejected resources");
        });
    }

    [Fact]
    public async Task DataTestIds_NotFoundPage_ExposesStableSelector()
    {
        await RunScenarioAsync(Area, "not-found-page-selector", async page =>
        {
            await AssertRouteSelectorsAsync(
                page,
                Audit($"/neexistujici-stranka-{Guid.NewGuid():N}", Selectors.Errors.NotFoundPage));
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    private static RouteSelectorAudit Audit(string route, string rootTestId, params string[] componentTestIds) =>
        new(route, rootTestId, componentTestIds);

    private async Task AssertRouteSelectorsAsync(IPage page, RouteSelectorAudit route)
    {
        await Fixture.GoToAndWaitForAppReadyAsync(page, route.Route);
        await Expect(page.GetByTestId(route.RootTestId)).ToBeVisibleAsync(new() { Timeout = 10_000 });

        foreach (var componentTestId in route.ComponentTestIds)
        {
            await Expect(page.GetByTestId(componentTestId).First).ToBeVisibleAsync(new() { Timeout = 10_000 });
        }
    }

    private sealed record RouteSelectorAudit(string Route, string RootTestId, string[] ComponentTestIds);
}
