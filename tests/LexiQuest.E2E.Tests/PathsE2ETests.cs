using FluentAssertions;
using LexiQuest.Core.Services;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class PathsE2ETests : E2ETestBase
{
    public PathsE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Paths_NewUser_ShowsFourPathsAndOnlyBeginnerUnlocked()
    {
        await RunScenarioAsync("paths", "new-user-lock-state", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("pathsnew");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/paths");

            await Expect(page.GetByTestId(Selectors.Paths.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Paths.Card)).ToHaveCountAsync(4);
            await Expect(page.GetByTestId(Selectors.Paths.BeginnerCard)).ToContainTextAsync("Začátečník");
            await Expect(page.GetByTestId(Selectors.Paths.BeginnerCard).GetByRole(AriaRole.Button)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Paths.IntermediateCard).GetByTestId(Selectors.Paths.LockedBadge)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Paths.AdvancedCard).GetByTestId(Selectors.Paths.LockedBadge)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Paths.ExpertCard).GetByTestId(Selectors.Paths.LockedBadge)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "paths",
                scenario: "new-user-lock-state",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "newUser");
        });
    }

    [Fact]
    public async Task Paths_LevelFiveUser_UnlocksIntermediatePath()
    {
        await RunScenarioAsync("paths", "level-five-unlocks-intermediate", async page =>
        {
            var levelCalculator = new LevelCalculator();
            var levelFiveXp = levelCalculator.GetCumulativeXpForLevel(5);
            var user = await Fixture.RegisterUniqueUserAsync("pathslevel5");
            await Fixture.ForceUserStatsAsync(user.Email, totalXp: levelFiveXp, level: 5);
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/paths");

            await Expect(page.GetByTestId(Selectors.Paths.IntermediateCard)).ToContainTextAsync("Cesta pro pokročilé");
            await Expect(page.GetByTestId(Selectors.Paths.IntermediateCard).GetByTestId(Selectors.Paths.LockedBadge)).Not.ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Paths.IntermediateCard).GetByRole(AriaRole.Button)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "paths",
                scenario: "level-five-unlocks-intermediate",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "levelFiveUser");
        });
    }

    [Fact]
    public async Task Paths_BeginnerDetail_ShowsMapLevelModalAndStartsPathGame()
    {
        await RunScenarioAsync("paths", "beginner-detail-starts-path-game", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("pathdetail");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/paths");
            await page.GetByTestId(Selectors.Paths.BeginnerCard).GetByRole(AriaRole.Button).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Paths.DetailPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Paths.Map)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Paths.Level)).ToHaveCountAsync(20);

            var firstLevel = page.Locator("[data-testid='path-level'][data-level-number='1']");
            var secondLevel = page.Locator("[data-testid='path-level'][data-level-number='2']");
            var bossLevel = page.Locator("[data-testid='path-level'][data-level-number='5']");

            (await firstLevel.GetAttributeAsync("class")).Should().Contain("level-current");
            await Expect(firstLevel).ToBeEnabledAsync();
            (await secondLevel.GetAttributeAsync("class")).Should().Contain("level-locked");
            await Expect(secondLevel).ToBeDisabledAsync();
            (await bossLevel.GetAttributeAsync("class")).Should().Contain("level-boss");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "paths",
                scenario: "beginner-detail-starts-path-game",
                state: "map",
                viewport: "1366x900",
                theme: "light",
                persona: "newUser");

            await firstLevel.ClickAsync();
            await Expect(page.GetByTestId(Selectors.Paths.LevelDetail)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Paths.LevelDetailStatus)).ToContainTextAsync("Aktuální");
            await Expect(page.GetByTestId(Selectors.Paths.LevelDetailWordCount)).ToContainTextAsync("10");
            await Expect(page.GetByTestId(Selectors.Paths.LevelDetailTime)).ToContainTextAsync("30");
            await Expect(page.GetByTestId(Selectors.Paths.LevelDetailHints)).ToContainTextAsync("3");
            await Expect(page.GetByTestId(Selectors.Paths.LevelDetailLives)).ToContainTextAsync("5");
            await Expect(page.GetByTestId(Selectors.Paths.LevelDetailReward)).ToContainTextAsync("100 XP");
            await Expect(page.GetByTestId(Selectors.Paths.LevelStart)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "paths",
                scenario: "beginner-detail-starts-path-game",
                state: "level-modal",
                viewport: "1366x900",
                theme: "light",
                persona: "newUser");

            await page.GetByTestId(Selectors.Paths.LevelStart).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Game.Timer)).ToBeVisibleAsync();
            page.Url.Should().Contain("/game/");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "paths",
                scenario: "beginner-detail-starts-path-game",
                state: "path-game",
                viewport: "1366x900",
                theme: "light",
                persona: "newUser");
        });
    }

    [Fact]
    public async Task Paths_CompleteLevel_UpdatesProgressAndShowsPerfectState()
    {
        await RunScenarioAsync("paths", "complete-level-perfect-progress", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("pathcomplete");
            await Fixture.SeedUserAchievementAsync(user.Email, "first_word", progress: 1, isUnlocked: true);
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/paths");
            await page.GetByTestId(Selectors.Paths.BeginnerCard).GetByRole(AriaRole.Button).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Paths.DetailPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var pathId = ExtractPathId(page.Url);

            await page.Locator("[data-testid='path-level'][data-level-number='1']").ClickAsync();
            await Expect(page.GetByTestId(Selectors.Paths.LevelDetail)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Paths.LevelStart).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var sessionId = ExtractSessionId(page.Url);

            for (var round = 1; round <= 10; round++)
            {
                var answer = await Fixture.GetActiveRoundAnswerAsync(sessionId);
                await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(answer);
                await page.GetByTestId(Selectors.Game.Submit).ClickAsync();
                await ContinueLevelUpIfVisibleAsync(page);

                if (round < 10)
                {
                    await Expect(page.GetByText($"Kolo {round + 1}")).ToBeVisibleAsync(new() { Timeout = 10_000 });
                    await ContinueLevelUpIfVisibleAsync(page);
                }
            }

            await ContinueLevelUpIfVisibleAsync(page);
            await Expect(page.GetByTestId(Selectors.Game.LevelComplete)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/paths/{pathId}");
            await Expect(page.GetByTestId(Selectors.Paths.Map)).ToBeVisibleAsync();

            var firstLevelClass = await page.Locator("[data-testid='path-level'][data-level-number='1']").GetAttributeAsync("class");
            var secondLevelClass = await page.Locator("[data-testid='path-level'][data-level-number='2']").GetAttributeAsync("class");

            firstLevelClass.Should().Contain("level-perfect");
            secondLevelClass.Should().Contain("level-current");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "paths",
                scenario: "complete-level-perfect-progress",
                state: "perfect-progress",
                viewport: "1366x900",
                theme: "light",
                persona: "newUser");
        });
    }

    [Fact]
    public async Task Paths_SeededCompletedLevel_ShowsCompletedStateAndCurrentNextLevel()
    {
        await RunScenarioAsync("paths", "completed-progress-state", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("pathcompleted");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/paths");
            await page.GetByTestId(Selectors.Paths.BeginnerCard).GetByRole(AriaRole.Button).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Paths.DetailPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var pathId = ExtractPathId(page.Url);
            await Fixture.ForcePathLevelProgressAsync(user.Email, pathId, levelNumber: 1, isPerfect: false);

            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/paths/{pathId}");
            await Expect(page.GetByTestId(Selectors.Paths.Map)).ToBeVisibleAsync();

            var firstLevelClass = await page.Locator("[data-testid='path-level'][data-level-number='1']").GetAttributeAsync("class");
            var secondLevelClass = await page.Locator("[data-testid='path-level'][data-level-number='2']").GetAttributeAsync("class");

            firstLevelClass.Should().Contain("level-completed");
            firstLevelClass.Should().NotContain("level-perfect");
            secondLevelClass.Should().Contain("level-current");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "paths",
                scenario: "completed-progress-state",
                state: "completed-progress",
                viewport: "1366x900",
                theme: "light",
                persona: "seededCompletedUser");
        });
    }

    private static Guid ExtractPathId(string url)
    {
        var lastSegment = new Uri(url).Segments.Last().Trim('/');
        return Guid.Parse(lastSegment);
    }

    private static Guid ExtractSessionId(string url)
    {
        var lastSegment = new Uri(url).Segments.Last().Trim('/');
        return Guid.Parse(lastSegment);
    }

    private static async Task ContinueLevelUpIfVisibleAsync(IPage page)
    {
        var modal = page.GetByTestId(Selectors.Game.LevelUpModal);
        if (await modal.IsVisibleAsync())
        {
            await page.GetByTestId(Selectors.Game.LevelUpContinue).ClickAsync();
            await Expect(modal).Not.ToBeVisibleAsync(new() { Timeout = 5_000 });
        }
    }
}
