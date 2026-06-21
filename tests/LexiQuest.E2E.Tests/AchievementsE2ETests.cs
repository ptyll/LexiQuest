using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Shared.DTOs.Achievements;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class AchievementsE2ETests : E2ETestBase
{
    public AchievementsE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Achievements_ApiAuthenticated_ReturnsSeededCatalog()
    {
        await RunScenarioAsync("achievements", "api-authenticated-catalog", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("achapi");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            using var response = await apiClient.GetAsync("api/v1/achievements");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var achievements = await response.Content.ReadFromJsonAsync<List<AchievementDto>>();
            achievements.Should().NotBeNull();
            achievements.Should().Contain(a => a.Key == "first_word" && a.Category == AchievementCategory.Performance);
            achievements.Should().Contain(a => a.Key == "streak_7" && a.Category == AchievementCategory.Streak);
        });
    }

    [Fact]
    public async Task Achievements_Page_ShowsProgressFiltersAndCardStates()
    {
        await RunScenarioAsync("achievements", "overview-filter-card-states", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("achpage");
            var unlockedAt = new DateTime(2026, 6, 17, 8, 0, 0, DateTimeKind.Utc);

            await Fixture.SeedUserAchievementAsync(
                user.Email,
                "first_word",
                progress: 1,
                isUnlocked: true,
                unlockedAtUtc: unlockedAt);
            await Fixture.SeedUserAchievementAsync(
                user.Email,
                "100_words",
                progress: 50,
                isUnlocked: false);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/achievements");

            await Expect(page.GetByTestId(Selectors.Achievements.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Achievements.Progress)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Achievements.UnlockedCount)).ToContainTextAsync("1 /");
            await Expect(page.GetByTestId(Selectors.Achievements.TotalProgress)).ToBeVisibleAsync();

            await Expect(page.GetByTestId(Selectors.Achievements.CardFirstWord)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Achievements.CardFirstWord)).ToContainTextAsync("První slovo");
            await Expect(page.GetByTestId(Selectors.Achievements.CardFirstWord)).ToContainTextAsync("Odemčeno 17.06.2026");
            await Expect(page.GetByTestId(Selectors.Achievements.CardFirstWord)).ToContainTextAsync("+10 XP");

            await Expect(page.GetByTestId(Selectors.Achievements.CardHundredWords)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Achievements.CardHundredWords)).ToContainTextAsync("50 / 100");
            await Expect(page.GetByTestId(Selectors.Achievements.CardStreakSeven)).ToBeVisibleAsync();

            await Expect(page.GetByTestId(Selectors.Achievements.UnlockedCard)).ToHaveCountAsync(1);
            await Expect(page.GetByTestId(Selectors.Achievements.InProgressCard)).ToHaveCountAsync(1);
            var lockedCount = await page.GetByTestId(Selectors.Achievements.LockedCard).CountAsync();
            lockedCount.Should().BeGreaterThan(0);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "achievements",
                scenario: "overview-filter-card-states",
                state: "all-states",
                viewport: "1366x900",
                theme: "light",
                persona: "achievementProgressUser");

            await page.GetByTestId(Selectors.Achievements.TabPerformance).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Achievements.CardFirstWord)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Achievements.CardHundredWords)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Achievements.CardStreakSeven)).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "achievements",
                scenario: "overview-filter-card-states",
                state: "performance-filter",
                viewport: "1366x900",
                theme: "light",
                persona: "achievementProgressUser");

            await page.GetByTestId(Selectors.Achievements.TabStreak).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Achievements.CardStreakSeven)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Achievements.CardFirstWord)).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "achievements",
                scenario: "overview-filter-card-states",
                state: "streak-filter",
                viewport: "1366x900",
                theme: "light",
                persona: "achievementProgressUser");
        });
    }

    [Fact]
    public async Task Achievements_FirstWordUnlock_ShowsModalAndDoesNotDuplicate()
    {
        await RunScenarioAsync("achievements", "first-word-unlock-no-duplicate", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("achunlock");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");
            await page.GetByTestId(Selectors.Game.ModeTraining).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var scrambled = NormalizeLetters(await page.GetByTestId(Selectors.Game.ScrambledWord).TextContentAsync());
            var answer = await Fixture.GetBeginnerOriginalForScrambledWordAsync(scrambled);

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(answer);
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Achievements.UnlockModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Achievements.UnlockModal)).ToContainTextAsync("První slovo");
            await Expect(page.GetByTestId(Selectors.Achievements.UnlockModal)).ToContainTextAsync("+10 XP");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "achievements",
                scenario: "first-word-unlock-no-duplicate",
                state: "unlock-modal",
                viewport: "1366x900",
                theme: "light",
                persona: "firstWordUser");

            (await Fixture.GetUserAchievementCountAsync(user.Email, "first_word")).Should().Be(1);

            var secondGame = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.Training, DifficultyLevel.Beginner));
            var secondAnswer = await Fixture.GetActiveRoundAnswerAsync(secondGame.SessionId);
            await Fixture.SubmitAnswerViaApiAsync(apiClient, secondGame.SessionId, secondAnswer, 1_000);

            (await Fixture.GetUserAchievementCountAsync(user.Email, "first_word")).Should().Be(1);
        });
    }

    private static string NormalizeLetters(string? value)
    {
        return new string((value ?? string.Empty)
            .Where(char.IsLetter)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }
}
