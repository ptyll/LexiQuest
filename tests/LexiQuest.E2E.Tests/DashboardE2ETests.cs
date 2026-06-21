using FluentAssertions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class DashboardE2ETests : E2ETestBase
{
    public DashboardE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Dashboard_NewUser_ShowsZeroProgressAndPrimaryActions()
    {
        await RunScenarioAsync("dashboard", "new-user-empty-progress", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("dashnew");
            await Fixture.LoginAsAsync(page, user);

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.TotalXP.Should().Be(0);
            stats.TotalWordsSolved.Should().Be(0);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByTestId(Selectors.Dashboard.Page)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Dashboard.StatsGrid)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Dashboard.TotalXpStat)).ToContainTextAsync("Celkové XP");
            await Expect(page.GetByTestId(Selectors.Dashboard.TotalXpStat)).ToContainTextAsync("0");
            await Expect(page.GetByTestId(Selectors.Dashboard.TotalXpStat)).ToContainTextAsync("Úroveň 1");
            await Expect(page.GetByTestId(Selectors.Dashboard.AccuracyStat)).ToContainTextAsync("0%");
            await Expect(page.GetByTestId(Selectors.Dashboard.AccuracyStat)).ToContainTextAsync("0 slov");
            await Expect(page.GetByTestId(Selectors.Dashboard.XpBarText)).ToContainTextAsync("0/");
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Hrát" })).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Trénink" })).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Cesty" })).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dashboard",
                scenario: "new-user-empty-progress",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "newUser");
        });
    }

    [Fact]
    public async Task Dashboard_PopulatedUser_ShowsStatsXpStreakAndActions()
    {
        await RunScenarioAsync("dashboard", "populated-user-stats", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("dashpop");
            await Fixture.ForceUserStatsAsync(
                user.Email,
                totalXp: 375,
                level: 3,
                totalWordsSolved: 8,
                accuracy: 75);
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 5,
                longestDays: 7,
                lastActivityUtc: DateTime.UtcNow);
            await Fixture.LoginAsAsync(page, user);

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.TotalXP.Should().Be(375);
            stats.TotalWordsSolved.Should().Be(8);
            stats.XpProgress.Should().NotBeNull();

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByTestId(Selectors.Dashboard.TotalXpStat)).ToContainTextAsync("375");
            await Expect(page.GetByTestId(Selectors.Dashboard.TotalXpStat)).ToContainTextAsync("Úroveň 3");
            await Expect(page.GetByTestId(Selectors.Dashboard.AccuracyStat)).ToContainTextAsync("75%");
            await Expect(page.GetByTestId(Selectors.Dashboard.AccuracyStat)).ToContainTextAsync("8 slov");
            await Expect(page.GetByTestId(Selectors.Dashboard.StreakIndicator)).ToContainTextAsync("5");
            await Expect(page.GetByTestId(Selectors.Dashboard.XpBarLevel)).ToContainTextAsync(stats.XpProgress!.CurrentLevel.ToString());
            await Expect(page.GetByTestId(Selectors.Dashboard.XpBarText))
                .ToHaveTextAsync($"{stats.XpProgress.XPInCurrentLevel}/{stats.XpProgress.XPRequiredForNextLevel} XP");

            var fillStyle = await page.GetByTestId(Selectors.Dashboard.XpBarFill).GetAttributeAsync("style");
            fillStyle.Should().Contain($"{stats.XpProgress.ProgressPercentage}%");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dashboard",
                scenario: "populated-user-stats",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Dashboard_LoadingSkeleton_IsVisibleWhileStatsRequestIsPending()
    {
        await RunScenarioAsync("dashboard", "loading-skeleton", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("dashload");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.DelayNextUserStatsRequestAsync();

            await page.GotoAsync($"{Fixture.WebBaseUrl}/dashboard", new()
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            await Expect(page.GetByTestId(Selectors.Dashboard.Skeleton)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dashboard.Skeleton)).ToHaveAttributeAsync("aria-busy", "true");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dashboard",
                scenario: "loading-skeleton",
                state: "loading",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");

            await Fixture.ReleaseDelayedUserStatsRequestAsync();
            await Expect(page.GetByTestId(Selectors.Dashboard.XpProgress)).ToBeVisibleAsync(new() { Timeout = 10_000 });
        });
    }

    [Fact]
    public async Task Dashboard_ErrorRetry_RecoversAfterStatsFailure()
    {
        await RunScenarioAsync("dashboard", "error-retry", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("dashretry");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.FailNextUserStatsRequestAsync();

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByTestId(Selectors.Dashboard.Error)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByText("Nepodařilo se načíst data")).ToBeVisibleAsync();
            await Expect(page.GetByText("Zkuste to prosím znovu později.")).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dashboard",
                scenario: "error-retry",
                state: "error",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");

            await page.GetByRole(AriaRole.Button, new() { Name = "Zkusit znovu" }).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dashboard.XpProgress)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dashboard.Error)).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dashboard",
                scenario: "error-retry",
                state: "recovered",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        }, assertNoConsoleErrors: false);
    }
}
