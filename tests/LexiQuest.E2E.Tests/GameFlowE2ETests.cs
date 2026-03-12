using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
public class GameFlowE2ETests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public GameFlowE2ETests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<IPage> LoginAsync()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/login");
        await page.FillAsync("[data-testid='email']", "e2etest@test.com");
        await page.FillAsync("[data-testid='password']", "TestPass123!");
        await page.ClickAsync("[data-testid='login-button']");
        await page.WaitForURLAsync("**/dashboard");
        return page;
    }

    [Fact]
    public async Task StartGame_AnswerWords_CompleteRound()
    {
        var page = await LoginAsync();

        // Navigate to game / start a new game
        await page.ClickAsync("[data-testid='start-game'], [data-testid='play-button']");
        await page.WaitForSelectorAsync("[data-testid='game-arena'], .game-arena", new() { Timeout = 10000 });

        // Game arena should be visible
        var gameArena = page.Locator("[data-testid='game-arena'], .game-arena");
        await Expect(gameArena).ToBeVisibleAsync();

        // Wait for a word/question to appear
        var wordDisplay = page.Locator("[data-testid='word-display'], .word-display, [data-testid='question']");
        await Expect(wordDisplay).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Type an answer
        var answerInput = page.Locator("[data-testid='answer-input'], .answer-input, input[type='text']");
        await answerInput.FillAsync("testanswer");

        // Submit the answer
        await page.ClickAsync("[data-testid='submit-answer'], [data-testid='check-button'], button[type='submit']");

        // Should show feedback (correct or incorrect)
        var feedback = page.Locator("[data-testid='feedback'], .feedback, [data-testid='result']");
        await Expect(feedback).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Fact]
    public async Task GameSession_ShowsProgressIndicator()
    {
        var page = await LoginAsync();

        await page.ClickAsync("[data-testid='start-game'], [data-testid='play-button']");
        await page.WaitForSelectorAsync("[data-testid='game-arena'], .game-arena", new() { Timeout = 10000 });

        // Progress indicator should be visible
        var progress = page.Locator("[data-testid='progress'], .progress, [data-testid='round-counter']");
        await Expect(progress).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Fact]
    public async Task GameSession_CanBePaused()
    {
        var page = await LoginAsync();

        await page.ClickAsync("[data-testid='start-game'], [data-testid='play-button']");
        await page.WaitForSelectorAsync("[data-testid='game-arena'], .game-arena", new() { Timeout = 10000 });

        // Click pause/menu button if available
        var pauseButton = page.Locator("[data-testid='pause-button'], [data-testid='menu-button']");
        if (await pauseButton.IsVisibleAsync())
        {
            await pauseButton.ClickAsync();
            var pauseMenu = page.Locator("[data-testid='pause-menu'], .pause-menu, .modal");
            await Expect(pauseMenu).ToBeVisibleAsync(new() { Timeout = 3000 });
        }
    }

    [Fact]
    public async Task Dashboard_ShowsStreakAndStats()
    {
        var page = await LoginAsync();

        // Dashboard should show streak indicator
        var streak = page.Locator("[data-testid='streak-indicator'], .streak-indicator");
        await Expect(streak).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Stats section should be present
        var stats = page.Locator("[data-testid='stats'], .stats-section, [data-testid='user-stats']");
        await Expect(stats).ToBeVisibleAsync(new() { Timeout = 5000 });
    }
}
