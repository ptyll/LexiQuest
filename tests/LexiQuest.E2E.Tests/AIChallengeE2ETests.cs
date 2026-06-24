using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class AIChallengeE2ETests : E2ETestBase
{
    private const string Area = "ai-challenge";
    private const string Viewport = "1366x900";
    private const string Theme = "light";

    public AIChallengeE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AIChallenge_NoHistory_ShowsEmptyAnalysisAndChallengeCards()
    {
        await RunScenarioAsync(Area, "no-history-empty-state", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("aino");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/ai-challenge");

            await Expect(page.GetByTestId(Selectors.AIChallenge.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AIChallenge.WeakLettersEmpty)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.AIChallenge.CategoryPerformanceEmpty)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.AIChallenge.Tips)).ToContainTextAsync("Zatím nemáme dost dat");
            await Expect(page.GetByTestId(Selectors.AIChallenge.Tips)).Not.ToContainTextAsync("Daří se");
            await Expect(page.GetByTestId(Selectors.AIChallenge.ChallengeCard)).ToHaveCountAsync(4);
            await Expect(page.GetByTestId(Selectors.AIChallenge.PreviewWord).First).ToBeVisibleAsync(new() { Timeout = 30_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                Area,
                "no-history-empty-state",
                "empty-data",
                Viewport,
                Theme,
                user.Username);
        });
    }

    [Fact]
    public async Task AIChallenge_Analysis_ShowsWeakLettersSlowCategoriesAndCzechTips()
    {
        await RunScenarioAsync(Area, "analysis-weakness-and-slow-category", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("aianalysis");
            await SeedAiRoundAsync(user, "almanach", isCorrect: false, timeSpentMs: 12_000, DifficultyLevel.Advanced);
            await SeedAiRoundAsync(user, "almanach", isCorrect: false, timeSpentMs: 13_000, DifficultyLevel.Advanced);
            await SeedAiRoundAsync(user, "almanach", isCorrect: false, timeSpentMs: 14_000, DifficultyLevel.Advanced);
            await SeedAiRoundAsync(user, "almanach", isCorrect: false, timeSpentMs: 15_000, DifficultyLevel.Advanced);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/ai-challenge");

            await Expect(page.GetByTestId(Selectors.AIChallenge.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AIChallenge.WeakLetter).Filter(new() { HasText = "A" })).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.AIChallenge.CategoryRow).Filter(new() { HasText = "Expert" })).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.AIChallenge.CategoryRow).Filter(new() { HasText = "Průměrný čas" })).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.AIChallenge.Tips)).ToContainTextAsync("Trénujte");
            await Expect(page.GetByTestId(Selectors.AIChallenge.Tips)).Not.ToContainTextAsync("Focus");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                Area,
                "analysis-weakness-and-slow-category",
                "analysis",
                Viewport,
                Theme,
                user.Username);
        });
    }

    [Theory]
    [InlineData(AIChallengeType.WeaknessFocus, Selectors.AIChallenge.StartWeaknessFocus, "weakness-focus")]
    [InlineData(AIChallengeType.SpeedTraining, Selectors.AIChallenge.StartSpeedTraining, "speed-training")]
    [InlineData(AIChallengeType.MemoryGame, Selectors.AIChallenge.StartMemoryGame, "memory-game")]
    [InlineData(AIChallengeType.PatternRecognition, Selectors.AIChallenge.StartPatternRecognition, "pattern-recognition")]
    public async Task AIChallenge_StartType_ReusesGameArenaAndShowsWhyThisWordTooltip(
        AIChallengeType type,
        string startSelector,
        string scenario)
    {
        var expectedType = type.ToString();

        await RunScenarioAsync(Area, $"start-{scenario}", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync($"ai{scenario.Replace("-", string.Empty)}");
            await Fixture.EnsureWordAsync("xylofon", DifficultyLevel.Intermediate, WordCategory.Music);
            await Fixture.EnsureWordAsync("xenon", DifficultyLevel.Intermediate, WordCategory.Science);
            await SeedAiRoundAsync(user, "xylofon", isCorrect: false, timeSpentMs: 9_000, DifficultyLevel.Intermediate);
            await SeedAiRoundAsync(user, "xylofon", isCorrect: false, timeSpentMs: 9_500, DifficultyLevel.Intermediate);
            await SeedAiRoundAsync(user, "xylofon", isCorrect: false, timeSpentMs: 10_000, DifficultyLevel.Intermediate);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/ai-challenge");

            await Expect(page.GetByTestId(Selectors.AIChallenge.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AIChallenge.ChallengeCard).Filter(new() { HasText = expectedType })).ToHaveCountAsync(0);
            await Expect(page.GetByTestId(Selectors.AIChallenge.PreviewWord).First).ToBeVisibleAsync(new() { Timeout = 30_000 });

            await page.GetByTestId(Selectors.AIChallenge.ChallengeGrid).EvaluateAsync(
                """
                element => new Promise(resolve => {
                    const rect = element.getBoundingClientRect();
                    window.scrollBy({ top: rect.top - 120, left: 0, behavior: 'instant' });
                    requestAnimationFrame(() => requestAnimationFrame(resolve));
                })
                """);
            await page.GetByTestId(Selectors.AIChallenge.PreviewReasonToggle).First.ClickAsync();
            await Expect(page.GetByTestId(Selectors.AIChallenge.PreviewReasonTooltip).First).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.AIChallenge.PreviewReasonTooltip).First).ToContainTextAsync("Proč toto slovo");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                Area,
                $"start-{scenario}",
                "challenge-cards-with-tooltip",
                Viewport,
                Theme,
                user.Username,
                fullPage: false,
                scrollToTop: false);

            await page.GetByTestId(startSelector).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Game.ScrambledWord)).ToBeVisibleAsync();
            var sessionId = GetSessionIdFromGameUrl(page.Url);
            sessionId.Should().NotBeEmpty();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                Area,
                $"start-{scenario}",
                "game-arena",
                Viewport,
                Theme,
                user.Username);

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync("spatna-odpoved");
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Game.Feedback)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                Area,
                $"start-{scenario}",
                "session-feedback",
                Viewport,
                Theme,
                user.Username);
        });
    }

    [Fact]
    public async Task AIChallenge_WeaknessFocus_ChangesWordReasonsAfterHistory()
    {
        await RunScenarioAsync(Area, "personalized-selection-changes-with-history", async _ =>
        {
            await Fixture.EnsureWordAsync("xylofon", DifficultyLevel.Intermediate, WordCategory.Music);
            await Fixture.EnsureWordAsync("xenon", DifficultyLevel.Intermediate, WordCategory.Science);

            var noHistoryUser = await Fixture.RegisterUniqueUserAsync("ainohistory");
            var weakUser = await Fixture.RegisterUniqueUserAsync("aiweakx");
            await SeedAiRoundAsync(weakUser, "xylofon", isCorrect: false, timeSpentMs: 8_000, DifficultyLevel.Intermediate);
            await SeedAiRoundAsync(weakUser, "xylofon", isCorrect: false, timeSpentMs: 9_000, DifficultyLevel.Intermediate);
            await SeedAiRoundAsync(weakUser, "xylofon", isCorrect: false, timeSpentMs: 10_000, DifficultyLevel.Intermediate);

            var noHistoryChallenge = await GenerateChallengeViaApiAsync(noHistoryUser, AIChallengeType.WeaknessFocus);
            var weakChallenge = await GenerateChallengeViaApiAsync(weakUser, AIChallengeType.WeaknessFocus);

            noHistoryChallenge.Words.Should().NotBeEmpty();
            weakChallenge.Words.Should().NotBeEmpty();
            noHistoryChallenge.Words.Should().Contain(word => word.Reason.Contains("Obecný trénink", StringComparison.Ordinal));
            weakChallenge.Words.Should().Contain(word =>
                word.Reason.Contains("slabé písmeno", StringComparison.Ordinal));
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    private async Task<AIChallengeDto> GenerateChallengeViaApiAsync(TestUser user, AIChallengeType type)
    {
        using var httpClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
        using var response = await httpClient.PostAsJsonAsync("api/v1/challenges/ai/start", new AIChallengeRequest(type));
        response.EnsureSuccessStatusCode();

        var challenge = await response.Content.ReadFromJsonAsync<AIChallengeDto>();
        challenge.Should().NotBeNull();
        return challenge!;
    }

    private async Task SeedAiRoundAsync(
        TestUser user,
        string correctAnswer,
        bool isCorrect,
        int timeSpentMs,
        DifficultyLevel difficulty)
    {
        using var httpClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
        var game = await Fixture.StartGameViaApiAsync(user, new StartGameRequest(GameMode.Training, difficulty));
        await Fixture.ForceCurrentRoundAsync(game.SessionId, correctAnswer, ReverseForScramble(correctAnswer), 30);
        await Fixture.ForceSessionTotalRoundsAsync(game.SessionId, 1);
        await Fixture.SubmitAnswerViaApiAsync(
            httpClient,
            game.SessionId,
            isCorrect ? correctAnswer : $"spatne-{correctAnswer}",
            timeSpentMs);
    }

    private static string ReverseForScramble(string value)
    {
        var reversed = new string(value.Reverse().ToArray());
        return string.Equals(reversed, value, StringComparison.OrdinalIgnoreCase)
            ? $"{value}x"
            : reversed;
    }

    private static Guid GetSessionIdFromGameUrl(string url)
    {
        var path = new Uri(url).AbsolutePath;
        path.Should().StartWith("/game/");
        var idText = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        Guid.TryParse(idText, out var sessionId).Should().BeTrue();
        return sessionId;
    }
}
