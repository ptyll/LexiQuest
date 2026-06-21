using FluentAssertions;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Smoke")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class GameFlowE2ETests : E2ETestBase
{
    public GameFlowE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task StartGame_LoggedInUser_CanStartTrainingSession()
    {
        await RunScenarioAsync("game", "start-training", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("game");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");

            await Expect(page.GetByTestId(Selectors.Game.StartScreen)).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Vyber režim hry" })).ToBeVisibleAsync();
            await Expect(page.GetByText("Procvičuj bez tlaku")).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Trénink" })).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Na čas" })).ToBeVisibleAsync();
            await Expect(page.GetByText("Welcome")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("SelectMode")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("Mode_Training")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("Mode_TimeAttack")).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "start-screen",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");

            await page.GetByTestId(Selectors.Game.ModeTraining).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            page.Url.Should().Contain("/game/");
            await Expect(page.GetByTestId(Selectors.Game.AnswerInput)).ToBeVisibleAsync();
            await Expect(page.GetByPlaceholder("Napiš slovo...")).ToBeVisibleAsync();
            await Expect(page.GetByText("Kolo 1")).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Potvrdit" })).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Přeskočit" })).ToBeVisibleAsync();
            await Expect(page.GetByText("Button_Back")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("Answer_Placeholder")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("TimeRemaining")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("CURRENT STATE")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("SCRAMBLED WORD")).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "start-training",
                state: "active-game",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Game_NonExistingSession_ShowsErrorState()
    {
        await RunScenarioAsync("game", "missing-session", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("missinggame");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/game/{Guid.NewGuid()}");

            await Expect(page.GetByTestId(Selectors.Game.ErrorState)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "missing-session",
                state: "error",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        }, assertNoConsoleErrors: false);
    }

    [Fact]
    public async Task StartGame_LoggedInUser_CanStartTimeAttackSession()
    {
        await RunScenarioAsync("game", "start-time-attack", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("timeattack");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");

            await page.GetByTestId(Selectors.Game.ModeTimeAttack).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Game.Timer)).ToBeVisibleAsync();
            page.Url.Should().Contain("/game/");
        });
    }

    [Fact]
    public async Task Game_CorrectAnswer_IncreasesXpComboAndMovesToNextRound()
    {
        await RunScenarioAsync("game", "correct-answer-next-round", async page =>
        {
            await StartTrainingAsync(page, "correct");
            var userEmail = await page.EvaluateAsync<string>(
                "() => window.localStorage.getItem('e2e-current-user-email') || ''");
            userEmail.Should().NotBeEmpty();
            await Fixture.SeedUserAchievementAsync(userEmail, "first_word", progress: 1, isUnlocked: true);

            var correctAnswer = await GetCurrentCorrectAnswerAsync(page);
            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(correctAnswer);
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.Feedback)).ToBeVisibleAsync();
            await Expect(page.GetByText("Správně!")).ToBeVisibleAsync();
            await Expect(page.GetByText("x1 COMBO!")).ToBeVisibleAsync();

            await Expect(page.GetByText("Kolo 2")).ToBeVisibleAsync(new() { Timeout = 5_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "correct-answer-next-round",
                state: "round-2",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Game_WrongAnswer_ShowsCorrectAnswerAndStaysOnRound()
    {
        await RunScenarioAsync("game", "wrong-answer-feedback", async page =>
        {
            await StartTrainingAsync(page, "wrong");
            var correctAnswer = await GetCurrentCorrectAnswerAsync(page);

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync("spatnaodpoved");
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.Feedback)).ToBeVisibleAsync();
            await Expect(page.GetByText("Špatně!")).ToBeVisibleAsync();
            await Expect(page.GetByText($"Správná odpověď: {correctAnswer}")).ToBeVisibleAsync();
            await Expect(page.GetByText("Kolo 1")).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "wrong-answer-feedback",
                state: "feedback",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Theory]
    [InlineData("upper")]
    [InlineData("trim")]
    public async Task Game_AnswerMatching_IsCaseInsensitiveAndTrimsWhitespace(string variant)
    {
        await RunScenarioAsync("game", $"answer-matching-{variant}", async page =>
        {
            await StartTrainingAsync(page, $"match{variant}");
            var correctAnswer = await GetCurrentCorrectAnswerAsync(page);
            var submitted = variant == "upper"
                ? correctAnswer.ToUpperInvariant()
                : $"  {correctAnswer}  ";

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(submitted);
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.Feedback)).ToBeVisibleAsync();
            await Expect(page.GetByText("Správně!")).ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Game_DiacriticsMustMatch()
    {
        await RunScenarioAsync("game", "diacritics-must-match", async page =>
        {
            await StartTrainingAsync(page, "diacritics");
            var sessionId = ExtractSessionId(page.Url);
            await Fixture.ForceCurrentRoundAsync(sessionId, "myš", "šym");
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/game/{sessionId}");

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync("mys");
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByText("Špatně!")).ToBeVisibleAsync();
            await Expect(page.GetByText("Správná odpověď: myš")).ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Game_EmptyAndTooLongAnswers_DisableSubmit()
    {
        await RunScenarioAsync("game", "answer-validation-disabled-submit", async page =>
        {
            await StartTrainingAsync(page, "disabled");
            var submit = page.GetByRole(AriaRole.Button, new() { Name = "Potvrdit" });

            await Expect(submit).ToBeDisabledAsync();

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync("   ");
            await Expect(submit).ToBeDisabledAsync();

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(new string('a', 51));
            await Expect(submit).ToBeDisabledAsync();
        });
    }

    [Fact]
    public async Task Game_SkipTreatsRoundAsWrongAndShowsCorrectAnswer()
    {
        await RunScenarioAsync("game", "skip-round-feedback", async page =>
        {
            await StartTrainingAsync(page, "skip");
            var correctAnswer = await GetCurrentCorrectAnswerAsync(page);

            await page.GetByTestId(Selectors.Game.Skip).ClickAsync();

            await Expect(page.GetByText("Špatně!")).ToBeVisibleAsync();
            await Expect(page.GetByText($"Správná odpověď: {correctAnswer}")).ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Game_TimerExpiry_SubmitsEmptyAnswerAndShowsCorrectAnswer()
    {
        await RunScenarioAsync("game", "timer-expiry", async page =>
        {
            await StartTrainingAsync(page, "timer");
            var sessionId = ExtractSessionId(page.Url);
            var scrambled = await GetCurrentScrambledAsync(page);
            var correctAnswer = await Fixture.GetBeginnerOriginalForScrambledWordAsync(scrambled);

            await Fixture.ForceCurrentRoundAsync(sessionId, correctAnswer, scrambled, timeLimitSeconds: 1);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/game/{sessionId}");

            await Expect(page.GetByTestId(Selectors.Game.Timer)).ToBeVisibleAsync();
            await Expect(page.GetByText("Špatně!")).ToBeVisibleAsync(new() { Timeout = 5_000 });
            await Expect(page.GetByText($"Správná odpověď: {correctAnswer}")).ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Game_LowTimer_ShowsWarningBeforeExpiry()
    {
        await RunScenarioAsync("game", "low-timer-warning", async page =>
        {
            await StartTrainingAsync(page, "lowtimer");
            var sessionId = ExtractSessionId(page.Url);
            var scrambled = await GetCurrentScrambledAsync(page);
            var correctAnswer = await Fixture.GetBeginnerOriginalForScrambledWordAsync(scrambled);

            await Fixture.ForceCurrentRoundAsync(sessionId, correctAnswer, scrambled, timeLimitSeconds: 10);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/game/{sessionId}");

            var timer = page.GetByTestId(Selectors.Game.Timer);
            await Expect(timer).ToBeVisibleAsync();
            await Expect(timer).ToContainTextAsync("00:03", new() { Timeout = 10_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "low-timer-warning",
                state: "warning",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Game_LevelComplete_ShowsOverlayAfterFinalRound()
    {
        await RunScenarioAsync("game", "level-complete", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("levelcomplete");
            await Fixture.SeedUserAchievementAsync(user.Email, "first_word", progress: 1, isUnlocked: true);
            await Fixture.LoginAsAsync(page, user);

            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.Training, DifficultyLevel.Beginner));
            await Fixture.ForceSessionTotalRoundsAsync(game.SessionId, totalRounds: 1);

            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/game/{game.SessionId}");
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var correctAnswer = await Fixture.GetActiveRoundAnswerAsync(game.SessionId);
            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(correctAnswer);
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.LevelComplete)).ToBeVisibleAsync(new() { Timeout = 5_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "level-complete",
                state: "visible",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Game_TrainingMode_ShowsInfiniteLivesAndWrongAnswerDoesNotDecrease()
    {
        await RunScenarioAsync("game", "training-infinite-lives", async page =>
        {
            await StartTrainingAsync(page, "livestraining");

            await Expect(page.GetByTestId(Selectors.Game.Lives)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Game.LivesCount)).ToHaveTextAsync("∞");

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync("spatnaodpoved");
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByText("Špatně!")).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Game.LivesCount)).ToHaveTextAsync("∞");
        });
    }

    [Fact]
    public async Task Game_TimeAttackWrongAnswer_DecreasesLives()
    {
        await RunScenarioAsync("game", "time-attack-wrong-answer-loses-life", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("livesattack");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");

            await page.GetByTestId(Selectors.Game.ModeTimeAttack).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Expect(page.GetByTestId(Selectors.Game.LivesCount)).ToHaveTextAsync("5/5");

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync("spatnaodpoved");
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByText("Špatně!")).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Game.LivesCount)).ToHaveTextAsync("4/5");
        });
    }

    [Theory]
    [InlineData(DifficultyLevel.Beginner, "5/5")]
    [InlineData(DifficultyLevel.Intermediate, "4/4")]
    [InlineData(DifficultyLevel.Advanced, "3/3")]
    [InlineData(DifficultyLevel.Expert, "3/3")]
    public async Task Game_TimeAttackDifficulty_StartsWithExpectedLives(DifficultyLevel difficulty, string expectedLives)
    {
        await RunScenarioAsync("game", $"time-attack-lives-{difficulty.ToString().ToLowerInvariant()}", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync($"lives{difficulty.ToString().ToLowerInvariant()}");
            await Fixture.LoginAsAsync(page, user);

            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.TimeAttack, difficulty));

            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/game/{game.SessionId}");

            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Game.LivesCount)).ToHaveTextAsync(expectedLives);
        });
    }

    [Fact]
    public async Task Game_LastLifeLost_ShowsGameOverAndDisablesInput()
    {
        await RunScenarioAsync("game", "last-life-game-over", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("gameover");
            await Fixture.LoginAsAsync(page, user);

            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.TimeAttack, DifficultyLevel.Beginner));
            await Fixture.ForceSessionLivesAsync(game.SessionId, livesRemaining: 1);

            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/game/{game.SessionId}");
            await Expect(page.GetByTestId(Selectors.Game.LivesCount)).ToHaveTextAsync("1/5");

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync("spatnaodpoved");
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.GameOver)).ToBeVisibleAsync(new() { Timeout = 5_000 });
            await Expect(page.GetByTestId(Selectors.Game.AnswerInput)).ToBeDisabledAsync();
            await Expect(page.GetByTestId(Selectors.Game.LivesCount)).ToHaveTextAsync("0/5");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "last-life-game-over",
                state: "no-lives",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Game_LowLives_ShowsWarningAndRegenTimer()
    {
        await RunScenarioAsync("game", "low-lives-warning-regen", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("lowregen");
            await Fixture.LoginAsAsync(page, user);

            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.TimeAttack, DifficultyLevel.Beginner));
            await Fixture.ForceSessionLivesAsync(
                game.SessionId,
                livesRemaining: 1,
                nextLifeRegenAt: DateTime.UtcNow.AddMinutes(20));

            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/game/{game.SessionId}");

            await Expect(page.GetByTestId(Selectors.Game.LivesCount)).ToHaveTextAsync("1/5");
            await Expect(page.GetByTestId(Selectors.Game.LowLivesWarning)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Game.LivesRegen)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Game.LivesRegen)).ToContainTextAsync("Další život za");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "low-lives-warning-regen",
                state: "warning",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Game_MaxLives_DoesNotShowRegenOrOverflow()
    {
        await RunScenarioAsync("game", "max-lives-no-overflow", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("maxlives");
            await Fixture.LoginAsAsync(page, user);

            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.TimeAttack, DifficultyLevel.Beginner));
            await Fixture.ForceSessionLivesAsync(
                game.SessionId,
                livesRemaining: 5,
                nextLifeRegenAt: DateTime.UtcNow.AddMinutes(-5));

            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/game/{game.SessionId}");

            await Expect(page.GetByTestId(Selectors.Game.LivesCount)).ToHaveTextAsync("5/5");
            await Expect(page.GetByTestId(Selectors.Game.LivesRegen)).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("6/5")).Not.ToBeVisibleAsync();
        });
    }

    [Theory]
    [InlineData(2500, 15, 5)]
    [InlineData(4500, 13, 3)]
    [InlineData(8500, 11, 1)]
    [InlineData(10000, 10, 0)]
    public async Task Game_XpSpeedBonusThresholds_AreApplied(int timeSpentMs, int expectedXp, int expectedSpeedBonus)
    {
        await RunScenarioAsync("game", $"xp-speed-{timeSpentMs}", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync($"xpspeed{timeSpentMs}");
            await Fixture.LoginAsAsync(page, user);

            using var api = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.TimeAttack, DifficultyLevel.Beginner));
            var correctAnswer = await Fixture.GetActiveRoundAnswerAsync(game.SessionId);

            var result = await Fixture.SubmitAnswerViaApiAsync(api, game.SessionId, correctAnswer, timeSpentMs);

            result.IsCorrect.Should().BeTrue();
            result.XPEarned.Should().Be(expectedXp);
            result.SpeedBonus.Should().Be(expectedSpeedBonus);
        });
    }

    [Fact]
    public async Task Game_XpComboAndStreakBonuses_AreAppliedAcrossSession()
    {
        await RunScenarioAsync("game", "xp-combo-streak", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("xpcombo");
            await Fixture.LoginAsAsync(page, user);

            using var api = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.Training, DifficultyLevel.Beginner));

            var xpByRound = new List<int>();
            for (var round = 1; round <= 10; round++)
            {
                var correctAnswer = await Fixture.GetActiveRoundAnswerAsync(game.SessionId);
                var result = await Fixture.SubmitAnswerViaApiAsync(api, game.SessionId, correctAnswer, 15_000);
                xpByRound.Add(result.XPEarned);
            }

            xpByRound[0].Should().Be(10);
            xpByRound[2].Should().Be(12, "3+ combo uses 1.2x multiplier");
            xpByRound[4].Should().Be(17, "5+ combo uses 1.5x multiplier plus streak bonus");
            xpByRound[9].Should().Be(22, "10+ combo uses 2x multiplier plus streak bonus");
        });
    }

    [Fact]
    public async Task Game_CorrectAnswer_UpdatesUserStatsXpAndDashboardValues()
    {
        await RunScenarioAsync("game", "xp-updates-user-stats", async page =>
        {
            await StartTrainingAsync(page, "xpstats");
            var userEmail = await page.EvaluateAsync<string>("() => window.localStorage.getItem('e2e-current-user-email') || ''");
            userEmail.Should().NotBeEmpty();

            var correctAnswer = await GetCurrentCorrectAnswerAsync(page);
            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(correctAnswer);
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByText("Správně!")).ToBeVisibleAsync();

            var user = new TestUser(userEmail, string.Empty, "TestPass123!");
            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.TotalXP.Should().BeGreaterThan(0);
            stats.TotalWordsSolved.Should().Be(1);
            stats.Accuracy.Should().Be(100);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");
            await Expect(page.GetByTestId(Selectors.Dashboard.TotalXpStat))
                .ToContainTextAsync(stats.TotalXP.ToString());
        });
    }

    [Fact]
    public async Task Game_LevelUp_ShowsModalWhenXpCrossesThreshold()
    {
        await RunScenarioAsync("game", "level-up-modal", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("levelup");
            await Fixture.ForceUserStatsAsync(user.Email, totalXp: 95, level: 1);
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");

            await page.GetByTestId(Selectors.Game.ModeTraining).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var correctAnswer = await GetCurrentCorrectAnswerAsync(page);
            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(correctAnswer);
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.LevelUpModal)).ToBeVisibleAsync(new() { Timeout = 5_000 });
            await Expect(page.GetByTestId(Selectors.Game.LevelUpModal)).ToContainTextAsync("2");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "level-up-modal",
                state: "visible",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Game_MultiLevelUpFromSingleXpGain_ShowsFinalLevelAndAllUnlocks()
    {
        await RunScenarioAsync("game", "multi-level-up-single-xp-gain", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("multilevel");
            await Fixture.ForceUserStatsAsync(user.Email, totalXp: 50, level: 1);
            await Fixture.SetFixedCorrectAnswerXpAsync(500);
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");

            await page.GetByTestId(Selectors.Game.ModeTraining).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var correctAnswer = await GetCurrentCorrectAnswerAsync(page);
            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(correctAnswer);
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            var modal = page.GetByTestId(Selectors.Game.LevelUpModal);
            await Expect(modal).ToBeVisibleAsync(new() { Timeout = 5_000 });
            await Expect(modal).ToContainTextAsync("4");
            await Expect(modal).ToContainTextAsync("Cesta pro pokročilé");

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.TotalXP.Should().Be(550);
            stats.CurrentLevel.Should().Be(4);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: "multi-level-up-single-xp-gain",
                state: "visible",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Theory]
    [InlineData(3, "Cesta pro pokročilé")]
    [InlineData(5, "Ligy")]
    [InlineData(7, "Pokročilá cesta")]
    [InlineData(10, "Expertní cesta")]
    [InlineData(15, "Multiplayer")]
    public async Task Game_LevelUnlocks_ShowExpectedRewards(int targetLevel, string expectedUnlock)
    {
        await RunScenarioAsync("game", $"level-{targetLevel}-unlock", async page =>
        {
            var levelCalculator = new LevelCalculator();
            var targetThreshold = levelCalculator.GetCumulativeXpForLevel(targetLevel);
            var user = await Fixture.RegisterUniqueUserAsync($"unlock{targetLevel}");
            await Fixture.ForceUserStatsAsync(
                user.Email,
                totalXp: targetThreshold - 10,
                level: targetLevel - 1);
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");

            await page.GetByTestId(Selectors.Game.ModeTraining).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var correctAnswer = await GetCurrentCorrectAnswerAsync(page);
            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(correctAnswer);
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            var modal = page.GetByTestId(Selectors.Game.LevelUpModal);
            await Expect(modal).ToBeVisibleAsync(new() { Timeout = 5_000 });
            await Expect(modal).ToContainTextAsync(targetLevel.ToString());
            await Expect(modal).ToContainTextAsync(expectedUnlock);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "game",
                scenario: $"level-{targetLevel}-unlock",
                state: "visible",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Dashboard_XpBar_MatchesStatsApiProgress()
    {
        await RunScenarioAsync("dashboard", "xp-bar-api-progress", async page =>
        {
            var levelCalculator = new LevelCalculator();
            const int totalXp = 375;
            var user = await Fixture.RegisterUniqueUserAsync("xpbar");
            await Fixture.ForceUserStatsAsync(
                user.Email,
                totalXp: totalXp,
                level: levelCalculator.GetLevelFromXp(totalXp),
                totalWordsSolved: 8,
                accuracy: 75);
            await Fixture.LoginAsAsync(page, user);

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.XpProgress.Should().NotBeNull();
            var progress = stats.XpProgress!;

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByTestId(Selectors.Dashboard.XpProgress)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dashboard.XpBar)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Dashboard.XpBarLevel)).ToContainTextAsync(progress.CurrentLevel.ToString());
            await Expect(page.GetByTestId(Selectors.Dashboard.XpBarText))
                .ToHaveTextAsync($"{progress.XPInCurrentLevel}/{progress.XPRequiredForNextLevel} XP");

            var fillStyle = await page.GetByTestId(Selectors.Dashboard.XpBarFill).GetAttributeAsync("style");
            fillStyle.Should().Contain($"{progress.ProgressPercentage}%");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dashboard",
                scenario: "xp-bar-api-progress",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Game_ReloadActiveSession_RestoresCurrentRound()
    {
        await RunScenarioAsync("game", "reload-active-session", async page =>
        {
            await StartTrainingAsync(page, "reload");
            var url = page.Url;
            var scrambledBeforeReload = await GetCurrentScrambledAsync(page);

            await page.ReloadAsync();
            await Fixture.WaitForNoBusyIndicatorsAsync(page);

            page.Url.Should().Be(url);
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync();
            var scrambledAfterReload = await GetCurrentScrambledAsync(page);
            scrambledAfterReload.Should().Be(scrambledBeforeReload);
        });
    }

    [Fact]
    public async Task Game_OtherUsersSession_IsNotAccessible()
    {
        await RunScenarioAsync("game", "other-user-session-forbidden", async page =>
        {
            await StartTrainingAsync(page, "owner");
            var ownerSessionPath = new Uri(page.Url).PathAndQuery;

            var otherUser = await Fixture.RegisterUniqueUserAsync("intruder");
            await Fixture.LoginAsAsync(page, otherUser);
            await Fixture.GoToAndWaitForAppReadyAsync(page, ownerSessionPath);

            await Expect(page.GetByTestId(Selectors.Game.ErrorState)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Game.Arena)).Not.ToBeVisibleAsync();
        }, assertNoConsoleErrors: false);
    }

    private async Task StartTrainingAsync(IPage page, string userPrefix)
    {
        var user = await Fixture.RegisterUniqueUserAsync(userPrefix);
        await Fixture.LoginAsAsync(page, user);
        await page.EvaluateAsync(
            "email => window.localStorage.setItem('e2e-current-user-email', email)",
            user.Email);
        await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");

        await page.GetByTestId(Selectors.Game.ModeTraining).ClickAsync();
        await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
    }

    private async Task<string> GetCurrentScrambledAsync(IPage page)
    {
        var text = await page.GetByTestId(Selectors.Game.ScrambledWord).TextContentAsync();
        return NormalizeLetters(text);
    }

    private async Task<string> GetCurrentCorrectAnswerAsync(IPage page)
    {
        var scrambled = await GetCurrentScrambledAsync(page);
        return await Fixture.GetBeginnerOriginalForScrambledWordAsync(scrambled);
    }

    private static string NormalizeLetters(string? value)
    {
        return new string((value ?? string.Empty)
            .Where(char.IsLetter)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }

    private static Guid ExtractSessionId(string url)
    {
        var lastSegment = new Uri(url).Segments.Last().Trim('/');
        return Guid.Parse(lastSegment);
    }
}
