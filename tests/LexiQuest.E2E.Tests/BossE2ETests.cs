using System.Net;
using System.Net.Http.Json;
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
[Collection(E2ECollection.Name)]
public class BossE2ETests : E2ETestBase
{
    public BossE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Boss_Marathon_StartsWithTwentyWordsThreeLivesAndNoRegen()
    {
        await RunScenarioAsync("boss", "marathon-start-no-regen", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("bossmarathon");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            using var startResponse = await apiClient.PostAsJsonAsync(
                "api/v1/boss/start",
                new { BossType = BossType.Marathon, Difficulty = DifficultyLevel.Intermediate });
            startResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            using var startJson = JsonDocument.Parse(await startResponse.Content.ReadAsStringAsync());
            var sessionId = startJson.RootElement.GetProperty("id").GetGuid();
            startJson.RootElement.GetProperty("bossType").GetString().Should().Be(nameof(BossType.Marathon));
            startJson.RootElement.GetProperty("totalRounds").GetInt32().Should().Be(20);
            startJson.RootElement.GetProperty("livesRemaining").GetInt32().Should().Be(3);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/boss/marathon/{sessionId}");

            await Expect(page.GetByTestId(Selectors.Boss.MarathonHeader)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Boss.ProgressText)).ToContainTextAsync("1/20");
            await Expect(page.GetByTestId(Selectors.Boss.LivesCount)).ToHaveTextAsync("3");
            var livesDisplay = await page.GetByTestId(Selectors.Boss.LivesDisplay).TextContentAsync();
            (livesDisplay ?? string.Empty).ToLowerInvariant().Should().NotContain("regen");
            await Expect(page.GetByText("MarathonBoss_Title")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("Maratonský boss")).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "boss",
                scenario: "marathon-start-no-regen",
                state: "start",
                viewport: "1366x900",
                theme: "light",
                persona: "bossUser");
        });
    }

    [Fact]
    public async Task Boss_Marathon_VictoryModal_ShowsPerfectAndSpeedBonuses()
    {
        await RunScenarioAsync("boss", "marathon-victory-perfect-speed", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("bossvictory");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var session = await StartBossAsync(apiClient, BossType.Marathon);

            await Fixture.ForceSessionTotalRoundsAsync(session.Id, 1);
            var answer = await Fixture.GetActiveRoundAnswerAsync(session.Id);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/boss/marathon/{session.Id}");

            await FillBossAnswerAsync(page, answer);
            await SubmitBossAnswerAsync(page);

            await Expect(page.GetByTestId(Selectors.Boss.VictoryModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByText("Vítězství!")).ToBeVisibleAsync();
            await Expect(page.GetByText("Bonus za perfektní hru")).ToBeVisibleAsync();
            await Expect(page.GetByText("+200 XP")).ToBeVisibleAsync();
            await Expect(page.GetByText("Bonus za rychlost")).ToBeVisibleAsync();
            await Expect(page.GetByText("+50 XP")).ToBeVisibleAsync();
            var completedState = await GetBossStateAsync(apiClient, session.Id);
            completedState.TotalXP.Should().BeGreaterThanOrEqualTo(250);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "boss",
                scenario: "marathon-victory-perfect-speed",
                state: "victory-modal",
                viewport: "1366x900",
                theme: "light",
                persona: "bossUser");
        });
    }

    [Fact]
    public async Task Boss_Marathon_DefeatModal_ShowsAfterLastLifeLostWithoutRegen()
    {
        await RunScenarioAsync("boss", "marathon-defeat-no-regen", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("bossdefeat");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var session = await StartBossAsync(apiClient, BossType.Marathon);

            await Fixture.ForceSessionLivesAsync(session.Id, 1);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/boss/marathon/{session.Id}");

            await FillBossAnswerAsync(page, "spatnaodpoved");
            await SubmitBossAnswerAsync(page);

            await Expect(page.GetByTestId(Selectors.Boss.DefeatModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByText("Prohra")).ToBeVisibleAsync();
            await Expect(page.GetByText("Konec hry")).ToBeVisibleAsync();
            var body = await page.GetByTestId(Selectors.Boss.DefeatModal).TextContentAsync();
            (body ?? string.Empty).ToLowerInvariant().Should().NotContain("regen");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "boss",
                scenario: "marathon-defeat-no-regen",
                state: "defeat-modal",
                viewport: "1366x900",
                theme: "light",
                persona: "bossUser");
        });
    }

    [Fact]
    public async Task Boss_Condition_EveryThirdRoundHasForbiddenLettersAndPenaltyRules()
    {
        await RunScenarioAsync("boss", "condition-forbidden-penalty", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("bosscondition");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var session = await StartBossAsync(apiClient, BossType.Condition);

            session.TotalRounds.Should().Be(15);
            session.ForbiddenLetters.Should().BeNull("the first Condition round should not carry the every-third-word rule");

            await AdvanceToConditionThirdRoundAsync(apiClient, session.Id);
            var thirdRound = await GetBossStateAsync(apiClient, session.Id);
            thirdRound.CurrentRound.Should().Be(3);
            thirdRound.ForbiddenLetters.Should().NotBeNullOrWhiteSpace();
            thirdRound.ForbiddenLetters!.Length.Should().Be(2);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/boss/condition/{session.Id}");

            await Expect(page.GetByTestId(Selectors.Boss.ConditionHeader)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Boss.ProgressText)).ToContainTextAsync("3/15");
            await Expect(page.GetByTestId(Selectors.Boss.ForbiddenWarning)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Boss.ForbiddenLetters)).ToContainTextAsync(thirdRound.ForbiddenLetters);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "boss",
                scenario: "condition-forbidden-penalty",
                state: "third-round-warning",
                viewport: "1366x900",
                theme: "light",
                persona: "bossUser");

            var forbiddenAnswer = $"{char.ToLowerInvariant(thirdRound.ForbiddenLetters[0])}slovo";
            await Fixture.ForceCurrentRoundAsync(session.Id, forbiddenAnswer, new string(forbiddenAnswer.Reverse().ToArray()));
            var penalized = await SubmitBossAnswerViaApiAsync(apiClient, session.Id, forbiddenAnswer);
            penalized.IsCorrect.Should().BeTrue();
            penalized.ForbiddenLetterPenaltyXP.Should().Be(5);
            penalized.ForbiddenLetterPenalty.Should().Be("-5 XP");

            var cleanSession = await StartBossAsync(apiClient, BossType.Condition);
            await AdvanceToConditionThirdRoundAsync(apiClient, cleanSession.Id);
            await Fixture.ForceCurrentRoundAsync(cleanSession.Id, "krk", "rkk");
            var clean = await SubmitBossAnswerViaApiAsync(apiClient, cleanSession.Id, "krk");
            clean.IsCorrect.Should().BeTrue();
            clean.ForbiddenLetterPenaltyXP.Should().Be(0);
            clean.ForbiddenLetterPenalty.Should().BeNull();
        });
    }

    [Fact]
    public async Task Boss_Twist_StartsWithRevealedLettersAndRevealsAfterThreeSeconds()
    {
        await RunScenarioAsync("boss", "twist-reveal-after-three-seconds", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("bosstwist");
            await Fixture.LoginAsAsync(page, user);

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var session = await StartBossAsync(apiClient, BossType.Twist);

            session.TotalRounds.Should().Be(12);
            session.RevealedPositions.Should().HaveCount(2);
            session.RevealedLetters.Should().HaveCount(2);
            session.CurrentEarlyGuessBonus.Should().Be(10);

            await Fixture.ForceActiveRoundStartedAtAsync(session.Id, DateTime.UtcNow.AddMilliseconds(2_900));
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/boss/twist/{session.Id}");

            await Expect(page.GetByTestId(Selectors.Boss.TwistHeader)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Boss.ProgressText)).ToContainTextAsync("1/12");
            var initialRevealedCount = await page.GetByTestId(Selectors.Boss.LetterRevealed).CountAsync();
            initialRevealedCount.Should().BeGreaterThanOrEqualTo(2);
            initialRevealedCount.Should().BeLessThan(session.WordLength, "the reveal checkpoint needs at least one hidden letter left");
            await Expect(page.GetByTestId(Selectors.Boss.EarlyGuessBonus)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "boss",
                scenario: "twist-reveal-after-three-seconds",
                state: "loaded-reveal-state",
                viewport: "1366x900",
                theme: "light",
                persona: "bossUser");

            await page.WaitForFunctionAsync(
                "initial => document.querySelectorAll('[data-testid=\"letter-revealed\"]').length > initial",
                initialRevealedCount,
                new PageWaitForFunctionOptions { Timeout = 10_000 });

            var revealedCount = await page.GetByTestId(Selectors.Boss.LetterRevealed).CountAsync();
            revealedCount.Should().BeGreaterThan(initialRevealedCount);
            var bonusText = await page.GetByTestId(Selectors.Boss.EarlyGuessBonus).TextContentAsync();
            bonusText.Should().NotContain("+10 XP");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "boss",
                scenario: "twist-reveal-after-three-seconds",
                state: "after-three-seconds",
                viewport: "1366x900",
                theme: "light",
                persona: "bossUser");
        });
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(3, 7)]
    [InlineData(6, 5)]
    [InlineData(9, 2)]
    public async Task Boss_Twist_EarlyGuessBonus_MatchesRevealedLetterCount(int elapsedSeconds, int expectedBonus)
    {
        await RunScenarioAsync("boss", $"twist-early-bonus-{elapsedSeconds}s", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync($"twistbonus{elapsedSeconds}");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var session = await StartBossAsync(apiClient, BossType.Twist);
            var answer = await Fixture.GetActiveRoundAnswerAsync(session.Id);

            if (elapsedSeconds > 0)
            {
                await Fixture.AdvanceE2ETimeAsync(TimeSpan.FromSeconds(elapsedSeconds));
            }

            var result = await SubmitBossAnswerViaApiAsync(apiClient, session.Id, answer);
            result.IsCorrect.Should().BeTrue();
            result.EarlyGuessBonus.Should().Be(expectedBonus);
        });
    }

    private static async Task FillBossAnswerAsync(IPage page, string answer)
    {
        var input = page.GetByTestId(Selectors.Boss.AnswerInput);
        if (await input.CountAsync() > 0)
        {
            await input.FillAsync(answer);
            return;
        }

        await page.GetByRole(AriaRole.Textbox).FillAsync(answer);
    }

    private static async Task SubmitBossAnswerAsync(IPage page)
    {
        var submit = page.GetByTestId(Selectors.Boss.Submit);
        if (await submit.CountAsync() > 0)
        {
            await submit.ClickAsync();
            return;
        }

        await page.GetByRole(AriaRole.Button, new() { Name = "Odeslat odpověď" }).ClickAsync();
    }

    private static async Task<BossSessionDto> StartBossAsync(
        HttpClient apiClient,
        BossType bossType,
        DifficultyLevel difficulty = DifficultyLevel.Intermediate)
    {
        using var response = await apiClient.PostAsJsonAsync(
            "api/v1/boss/start",
            new BossStartRequest(bossType, difficulty));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var session = await response.Content.ReadFromJsonAsync<BossSessionDto>();
        session.Should().NotBeNull();
        session!.BossType.Should().Be(bossType);
        return session;
    }

    private static async Task<BossSessionDto> GetBossStateAsync(HttpClient apiClient, Guid sessionId)
    {
        var session = await apiClient.GetFromJsonAsync<BossSessionDto>($"api/v1/boss/{sessionId}");
        session.Should().NotBeNull();
        return session!;
    }

    private async Task AdvanceToConditionThirdRoundAsync(HttpClient apiClient, Guid sessionId)
    {
        for (var i = 0; i < 2; i++)
        {
            var answer = await Fixture.GetActiveRoundAnswerAsync(sessionId);
            var result = await SubmitBossAnswerViaApiAsync(apiClient, sessionId, answer);
            result.IsCorrect.Should().BeTrue();
        }
    }

    private static async Task<BossRoundResultDto> SubmitBossAnswerViaApiAsync(
        HttpClient apiClient,
        Guid sessionId,
        string answer,
        int timeSpentMs = 1000)
    {
        using var response = await apiClient.PostAsJsonAsync(
            $"api/v1/boss/{sessionId}/answer",
            new BossAnswerRequest
            {
                Answer = answer,
                TimeSpentMs = timeSpentMs
            });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BossRoundResultDto>();
        result.Should().NotBeNull();
        return result!;
    }
}
