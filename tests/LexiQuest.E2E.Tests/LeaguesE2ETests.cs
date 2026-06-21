using System.Net;
using FluentAssertions;
using LexiQuest.Shared.Enums;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class LeaguesE2ETests : E2ETestBase
{
    public LeaguesE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Leagues_NewUser_IsAssignedToBronzeLeague()
    {
        await RunScenarioAsync("leagues", "new-user-bronze", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("leaguebronze");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/leagues");

            await Expect(page.GetByTestId(Selectors.Leagues.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Leagues.Tier)).ToContainTextAsync("Bronzová liga");
            await Expect(page.GetByTestId(Selectors.Leagues.UserPosition)).ToContainTextAsync("#1");
            await Expect(page.GetByTestId(Selectors.Leagues.Leaderboard)).ToBeVisibleAsync();

            var currentUserRow = page.GetByTestId(Selectors.Leagues.CurrentUserRow);
            await Expect(currentUserRow).ToBeVisibleAsync();
            await Expect(currentUserRow).ToContainTextAsync(user.Username);
            await Expect(currentUserRow).ToContainTextAsync("0 XP");

            await Expect(page.GetByTestId(Selectors.Leagues.Rewards)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Leagues.Rewards)).ToContainTextAsync("Bronzová liga");
            await Expect(page.GetByTestId(Selectors.Leagues.Rewards)).ToContainTextAsync("Legendární liga");
            await Expect(page.GetByTestId(Selectors.Leagues.Rewards)).ToContainTextAsync("+1000 XP");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "leagues",
                scenario: "new-user-bronze",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "newUser");

            page.Url.Should().Contain("/leagues");
        });
    }

    [Fact]
    public async Task Leagues_Leaderboard_SortsHighlightsAndMarksPromotionDemotionZones()
    {
        await RunScenarioAsync("leagues", "leaderboard-zones", async page =>
        {
            var users = new List<TestUser>();
            for (var i = 0; i < 12; i++)
            {
                users.Add(await Fixture.RegisterUniqueUserAsync($"league{i:00}"));
            }

            var xpEntries = users
                .Select((user, index) => (user.Email, WeeklyXp: (12 - index) * 100))
                .ToArray();
            await Fixture.ForceLeagueWeeklyXpAsync(xpEntries);

            var currentUser = users[5];
            await Fixture.LoginAsAsync(page, currentUser);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/leagues");

            await Expect(page.GetByTestId(Selectors.Leagues.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Leagues.Leaderboard)).ToBeVisibleAsync();
            await Expect(page.Locator(".leaderboard-row")).ToHaveCountAsync(12);

            var rowTexts = await page.Locator(".leaderboard-row").AllTextContentsAsync();
            rowTexts[0].Should().Contain(users[0].Username);
            rowTexts[0].Should().Contain("1200 XP");
            rowTexts[5].Should().Contain(currentUser.Username);
            rowTexts[5].Should().Contain("700 XP");
            rowTexts[^1].Should().Contain(users[^1].Username);
            rowTexts[^1].Should().Contain("100 XP");

            await Expect(page.GetByTestId(Selectors.Leagues.PromotionZone)).ToHaveCountAsync(5);
            await Expect(page.GetByTestId(Selectors.Leagues.DemotionZone)).ToHaveCountAsync(5);

            var currentUserRow = page.GetByTestId(Selectors.Leagues.CurrentUserRow);
            await Expect(currentUserRow).ToBeVisibleAsync();
            await Expect(currentUserRow).ToContainTextAsync(currentUser.Username);
            (await currentUserRow.GetAttributeAsync("class")).Should().Contain("is-current-user");
            await AssertCurrentUserRowHasVisibleOutlineAsync(currentUserRow);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "leagues",
                scenario: "leaderboard-zones",
                state: "ranked",
                viewport: "1366x900",
                theme: "light",
                persona: "rankedUser");
        });
    }

    [Theory]
    [InlineData("countdown-warning", 23, "warning", "23h")]
    [InlineData("countdown-critical", 5, "critical", "5h")]
    public async Task Leagues_Countdown_UnderThresholds_UsesVisualState(
        string scenario,
        int hoursRemaining,
        string expectedState,
        string expectedText)
    {
        await RunScenarioAsync("leagues", scenario, async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync($"league{expectedState}");
            await Fixture.ForceLeagueWeekEndAsync(user.Email, DateTime.UtcNow.AddHours(hoursRemaining).AddMinutes(50));
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/leagues");

            var countdown = page.GetByTestId(Selectors.Leagues.Countdown);
            await Expect(countdown).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(countdown).ToContainTextAsync(expectedText);
            await Expect(countdown).ToHaveAttributeAsync("data-state", expectedState);

            var className = await countdown.GetAttributeAsync("class");
            className.Should().Contain($"is-{expectedState}");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "leagues",
                scenario: scenario,
                state: expectedState,
                viewport: "1366x900",
                theme: "light",
                persona: "countdownUser");
        });
    }

    [Fact]
    public async Task Leagues_WeeklyReset_MovesPromotedAndDemotedUsers()
    {
        await RunScenarioAsync("leagues", "weekly-reset-tier-moves", async page =>
        {
            var bronzeUsers = new List<TestUser>();
            var silverUsers = new List<TestUser>();

            for (var i = 0; i < 12; i++)
            {
                bronzeUsers.Add(await Fixture.RegisterUniqueUserAsync($"resetbronze{i:00}"));
                silverUsers.Add(await Fixture.RegisterUniqueUserAsync($"resetsilver{i:00}"));
            }

            await Fixture.ForceUsersIntoActiveLeagueTierAsync(
                LeagueTier.Silver,
                silverUsers.Select(user => user.Email).ToArray());

            var xpEntries = bronzeUsers
                .Select((user, index) => (user.Email, WeeklyXp: (12 - index) * 100))
                .Concat(silverUsers.Select((user, index) => (user.Email, WeeklyXp: (12 - index) * 100)))
                .ToArray();
            await Fixture.ForceLeagueWeeklyXpAsync(xpEntries);
            await Fixture.ForceActiveLeaguesToPreviousWeekAsync();

            var promotedUser = bronzeUsers[0];
            var demotedUser = silverUsers[^1];

            await Fixture.RunLeagueResetAsync();

            await Fixture.LoginAsAsync(page, promotedUser);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/leagues");

            await Expect(page.GetByTestId(Selectors.Leagues.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Leagues.Tier)).ToContainTextAsync("Stříbrná liga");
            await Expect(page.GetByTestId(Selectors.Leagues.CurrentUserRow)).ToContainTextAsync(promotedUser.Username);
            await Expect(page.GetByTestId(Selectors.Leagues.CurrentUserRow)).ToContainTextAsync("0 XP");
            await AssertCurrentUserRowHasVisibleOutlineAsync(page.GetByTestId(Selectors.Leagues.CurrentUserRow));
            await Expect(page.GetByTestId(Selectors.Leagues.History)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Leagues.History)).ToContainTextAsync("Bronzová liga");
            await Expect(page.GetByTestId(Selectors.Leagues.History)).ToContainTextAsync("#1");
            await Expect(page.GetByTestId(Selectors.Leagues.History)).ToContainTextAsync("1200 XP");
            await Expect(page.GetByTestId(Selectors.Leagues.History)).ToContainTextAsync("Postup");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "leagues",
                scenario: "weekly-reset-tier-moves",
                state: "promoted-silver",
                viewport: "1366x900",
                theme: "light",
                persona: "promotedUser");

            await page.EvaluateAsync("() => window.localStorage.clear()");
            await Fixture.LoginAsAsync(page, demotedUser);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/leagues");

            await Expect(page.GetByTestId(Selectors.Leagues.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Leagues.Tier)).ToContainTextAsync("Bronzová liga");
            await Expect(page.GetByTestId(Selectors.Leagues.CurrentUserRow)).ToContainTextAsync(demotedUser.Username);
            await Expect(page.GetByTestId(Selectors.Leagues.CurrentUserRow)).ToContainTextAsync("0 XP");
            await AssertCurrentUserRowHasVisibleOutlineAsync(page.GetByTestId(Selectors.Leagues.CurrentUserRow));
            await Expect(page.GetByTestId(Selectors.Leagues.History)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Leagues.History)).ToContainTextAsync("Stříbrná liga");
            await Expect(page.GetByTestId(Selectors.Leagues.History)).ToContainTextAsync("#12");
            await Expect(page.GetByTestId(Selectors.Leagues.History)).ToContainTextAsync("100 XP");
            await Expect(page.GetByTestId(Selectors.Leagues.History)).ToContainTextAsync("Sestup");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "leagues",
                scenario: "weekly-reset-tier-moves",
                state: "demoted-bronze",
                viewport: "1366x900",
                theme: "light",
                persona: "demotedUser");
        });
    }

    [Fact]
    public async Task Leagues_UnauthenticatedUser_IsRejected()
    {
        await RunScenarioAsync("leagues", "unauthenticated-rejected", async page =>
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri($"{Fixture.ApiBaseUrl}/") };
            using var apiResponse = await httpClient.GetAsync("api/v1/leagues/current");
            apiResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/leagues");
            await page.WaitForURLAsync("**/login");

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přihlášení" })).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Leagues.Page)).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "leagues",
                scenario: "unauthenticated-rejected",
                state: "login-required",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymousUser");
        });
    }

    private static async Task AssertCurrentUserRowHasVisibleOutlineAsync(ILocator row)
    {
        var boxShadow = await row.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow");
        boxShadow.Should().Contain("59, 130, 246");
    }
}
