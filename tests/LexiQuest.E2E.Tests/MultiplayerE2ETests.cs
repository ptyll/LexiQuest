using FluentAssertions;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "SignalR")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class MultiplayerE2ETests : E2ETestBase
{
    public MultiplayerE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Multiplayer_LandingQuickMatch_SearchCancelAndScreenshot()
    {
        await RunScenarioAsync("multiplayer", "landing-quickmatch-search-cancel", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("mpui");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/multiplayer");
            await Expect(page.GetByTestId(Selectors.Multiplayer.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Multiplayer.QuickMatchCard)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateRoomCard)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "multiplayer",
                scenario: "landing-quickmatch-search-cancel",
                state: "landing",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");

            await page.GetByTestId(Selectors.Multiplayer.QuickMatchStart).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.Searching)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Multiplayer.SearchingStatus)).ToContainTextAsync("Hledání");
            await Expect(page.GetByTestId(Selectors.Multiplayer.CancelSearch)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "multiplayer",
                scenario: "landing-quickmatch-search-cancel",
                state: "searching",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");

            await page.GetByTestId(Selectors.Multiplayer.CancelSearch).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task QuickMatch_SignalR_JwtDuplicateCancelAndTwoPlayerMatch()
    {
        await Fixture.ResetDatabaseAsync();

        var alice = await Fixture.RegisterUniqueUserAsync("mpalice");
        var bob = await Fixture.RegisterUniqueUserAsync("mpbob");
        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 4);
        await Fixture.ForceUserStatsAsync(bob.Email, totalXp: 390, level: 4);

        await using var aliceConnection = await CreateStartedHubConnectionAsync(alice);
        aliceConnection.State.Should().Be(HubConnectionState.Connected);

        var firstJoin = await aliceConnection.InvokeAsync<bool>("JoinMatchmaking");
        firstJoin.Should().BeTrue();

        var duplicateJoin = await aliceConnection.InvokeAsync<bool>("JoinMatchmaking");
        duplicateJoin.Should().BeFalse("the same user cannot be queued twice from one connection");

        var cancel = await aliceConnection.InvokeAsync<bool>("CancelMatchmaking");
        cancel.Should().BeTrue();

        var secondCancel = await aliceConnection.InvokeAsync<bool>("CancelMatchmaking");
        secondCancel.Should().BeFalse("cancelling an empty queue entry should be a no-op signal");

        await using var bobConnection = await CreateStartedHubConnectionAsync(bob);

        var aliceMatchTcs = new TaskCompletionSource<MatchFoundEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bobMatchTcs = new TaskCompletionSource<MatchFoundEvent>(TaskCreationOptions.RunContinuationsAsynchronously);

        aliceConnection.On<MatchFoundEvent>("MatchFound", match => aliceMatchTcs.TrySetResult(match));
        bobConnection.On<MatchFoundEvent>("MatchFound", match => bobMatchTcs.TrySetResult(match));

        (await aliceConnection.InvokeAsync<bool>("JoinMatchmaking")).Should().BeTrue();
        (await bobConnection.InvokeAsync<bool>("JoinMatchmaking")).Should().BeTrue();

        var aliceMatch = await AwaitAsync(aliceMatchTcs.Task, TimeSpan.FromSeconds(10), "Alice should receive MatchFound");
        var bobMatch = await AwaitAsync(bobMatchTcs.Task, TimeSpan.FromSeconds(10), "Bob should receive MatchFound");

        aliceMatch.MatchId.Should().Be(bobMatch.MatchId);
        aliceMatch.OpponentUsername.Should().Be(bob.Username);
        bobMatch.OpponentUsername.Should().Be(alice.Username);
        aliceMatch.OpponentLevel.Should().Be(4);
        bobMatch.OpponentLevel.Should().Be(4);
        aliceMatch.IsPrivateRoom.Should().BeFalse();
        bobMatch.IsPrivateRoom.Should().BeFalse();
    }

    [Fact]
    public async Task QuickMatch_SignalR_PrefersSimilarLevelAndLeavesFarOpponentQueued()
    {
        await Fixture.ResetDatabaseAsync();

        var alice = await Fixture.RegisterUniqueUserAsync("mpsimilaralice");
        var farOpponent = await Fixture.RegisterUniqueUserAsync("mpfarbob");
        var similarOpponent = await Fixture.RegisterUniqueUserAsync("mpsimilarcarol");

        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 5);
        await Fixture.ForceUserStatsAsync(farOpponent.Email, totalXp: 22_000, level: 20);
        await Fixture.ForceUserStatsAsync(similarOpponent.Email, totalXp: 520, level: 7);

        await using var aliceConnection = await CreateStartedHubConnectionAsync(alice);
        await using var farConnection = await CreateStartedHubConnectionAsync(farOpponent);
        await using var similarConnection = await CreateStartedHubConnectionAsync(similarOpponent);

        var aliceMatchTcs = new TaskCompletionSource<MatchFoundEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var farMatchTcs = new TaskCompletionSource<MatchFoundEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var similarMatchTcs = new TaskCompletionSource<MatchFoundEvent>(TaskCreationOptions.RunContinuationsAsynchronously);

        aliceConnection.On<MatchFoundEvent>("MatchFound", match => aliceMatchTcs.TrySetResult(match));
        farConnection.On<MatchFoundEvent>("MatchFound", match => farMatchTcs.TrySetResult(match));
        similarConnection.On<MatchFoundEvent>("MatchFound", match => similarMatchTcs.TrySetResult(match));

        (await aliceConnection.InvokeAsync<bool>("JoinMatchmaking")).Should().BeTrue();
        (await farConnection.InvokeAsync<bool>("JoinMatchmaking")).Should().BeTrue();

        await AssertNoMatchAsync(aliceMatchTcs.Task, TimeSpan.FromMilliseconds(750), "far level difference should not match immediately");
        await AssertNoMatchAsync(farMatchTcs.Task, TimeSpan.FromMilliseconds(750), "far opponent should remain queued");

        (await similarConnection.InvokeAsync<bool>("JoinMatchmaking")).Should().BeTrue();

        var aliceMatch = await AwaitAsync(aliceMatchTcs.Task, TimeSpan.FromSeconds(10), "Alice should match a similar-level opponent");
        var similarMatch = await AwaitAsync(similarMatchTcs.Task, TimeSpan.FromSeconds(10), "similar opponent should receive MatchFound");

        aliceMatch.MatchId.Should().Be(similarMatch.MatchId);
        aliceMatch.OpponentUsername.Should().Be(similarOpponent.Username);
        aliceMatch.OpponentLevel.Should().Be(7);
        similarMatch.OpponentUsername.Should().Be(alice.Username);
        similarMatch.OpponentLevel.Should().Be(5);

        await AssertNoMatchAsync(farMatchTcs.Task, TimeSpan.FromMilliseconds(750), "far opponent should not be included in the similar-level match");
        (await farConnection.InvokeAsync<bool>("CancelMatchmaking")).Should().BeTrue();
    }

    [Fact]
    public async Task QuickMatch_SinglePlayer_TimesOutAfterThirtySecondsAndShowsOptions()
    {
        const string scenario = "quickmatch-single-player-timeout";

        await RunScenarioAsync("multiplayer", scenario, async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("mptimeout");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/multiplayer/quick-match");
            await Expect(page.GetByTestId(Selectors.Multiplayer.Searching)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Expect(page.GetByTestId(Selectors.Multiplayer.Timeout)).ToBeVisibleAsync(new() { Timeout = 40_000 });
            await Expect(page.GetByTestId(Selectors.Multiplayer.TimeoutTitle)).ToContainTextAsync("Soupeř nenalezen");
            await Expect(page.GetByTestId(Selectors.Multiplayer.TimeoutRetry)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.TimeoutPlayVsAi)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.TimeoutBack)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "multiplayer",
                scenario,
                state: "timeout",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserA");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task QuickMatch_TwoBrowserPlayers_CountdownAndNavigateToRealtimeGame()
    {
        const string scenario = "quickmatch-countdown-realtime";
        await Fixture.ResetDatabaseAsync();

        var alice = await Fixture.RegisterUniqueUserAsync("mpuialice");
        var bob = await Fixture.RegisterUniqueUserAsync("mpuibob");
        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 4);
        await Fixture.ForceUserStatsAsync(bob.Email, totalXp: 390, level: 4);

        var alicePage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.alice");
        var bobPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.bob");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(alicePage, alice),
                Fixture.LoginAsAsync(bobPage, bob));

            await Task.WhenAll(
                Fixture.GoToAndWaitForAppReadyAsync(alicePage, "/multiplayer/quick-match"),
                Fixture.GoToAndWaitForAppReadyAsync(bobPage, "/multiplayer/quick-match"));

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.MatchFound)).ToBeVisibleAsync(new() { Timeout = 15_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.MatchFound)).ToBeVisibleAsync(new() { Timeout = 15_000 });
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.Countdown)).ToContainTextAsync(new Regex("[123]"));
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.Countdown)).ToContainTextAsync(new Regex("[123]"));

            await Fixture.TakeCheckpointScreenshotAsync(
                alicePage,
                area: "multiplayer",
                scenario,
                state: "match-found-countdown",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserA");

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 12_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 12_000 });
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimeScrambledWord)).ToBeVisibleAsync();
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeScrambledWord)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                alicePage,
                area: "multiplayer",
                scenario,
                state: "realtime-game",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserA");

            await Fixture.AssertNoConsoleErrorsAsync(alicePage);
            await Fixture.AssertNoConsoleErrorsAsync(bobPage);
            await Fixture.AssertNoFailedRequestsAsync(alicePage);
            await Fixture.AssertNoFailedRequestsAsync(bobPage);
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(alicePage, "multiplayer", $"{scenario}-alice");
            await Fixture.TakeFailureArtifactsAsync(bobPage, "multiplayer", $"{scenario}-bob");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await alicePage.Context.CloseAsync();
            await bobPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task QuickMatch_RealtimeCorrectAnswer_UpdatesOwnScoreAndOpponentProgress()
    {
        const string scenario = "quickmatch-realtime-score-progress";
        await Fixture.ResetDatabaseAsync();

        var alice = await Fixture.RegisterUniqueUserAsync("mpscorealice");
        var bob = await Fixture.RegisterUniqueUserAsync("mpscorebob");
        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 4);
        await Fixture.ForceUserStatsAsync(bob.Email, totalXp: 390, level: 4);

        var alicePage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.alice");
        var bobPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.bob");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(alicePage, alice),
                Fixture.LoginAsAsync(bobPage, bob));

            await Task.WhenAll(
                Fixture.GoToAndWaitForAppReadyAsync(alicePage, "/multiplayer/quick-match"),
                Fixture.GoToAndWaitForAppReadyAsync(bobPage, "/multiplayer/quick-match"));

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimeScrambledWord)).ToBeVisibleAsync(new() { Timeout = 8_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeScrambledWord)).ToBeVisibleAsync(new() { Timeout = 8_000 });

            var scrambledText = await alicePage.GetByTestId(Selectors.Multiplayer.RealtimeScrambledWord).InnerTextAsync();
            var scrambled = Regex.Replace(scrambledText, "[^\\p{L}]", "");
            var answer = await Fixture.GetOriginalForScrambledWordAsync(scrambled);

            await alicePage.GetByTestId(Selectors.Multiplayer.RealtimeAnswerInput).FillAsync(answer);
            await alicePage.GetByTestId(Selectors.Multiplayer.RealtimeSubmit).ClickAsync();

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimePlayerScore)).ToContainTextAsync("1/15", new() { Timeout = 8_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeOpponentScore)).ToContainTextAsync("1/15", new() { Timeout = 8_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                alicePage,
                area: "multiplayer",
                scenario,
                state: "alice-score-updated",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserA");

            await Fixture.TakeCheckpointScreenshotAsync(
                bobPage,
                area: "multiplayer",
                scenario,
                state: "bob-opponent-progress",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserB");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(alicePage, "multiplayer", $"{scenario}-alice");
            await Fixture.TakeFailureArtifactsAsync(bobPage, "multiplayer", $"{scenario}-bob");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await alicePage.Context.CloseAsync();
            await bobPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task QuickMatch_Forfeit_ShowsPerspectiveResultsAwardsXpAndSavesHistory()
    {
        const string scenario = "quickmatch-forfeit-result-history";
        await Fixture.ResetDatabaseAsync();

        var alice = await Fixture.RegisterUniqueUserAsync("mpforfeitalice");
        var bob = await Fixture.RegisterUniqueUserAsync("mpforfeitbob");
        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 4);
        await Fixture.ForceUserStatsAsync(bob.Email, totalXp: 390, level: 4);

        var alicePage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.alice");
        var bobPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.bob");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(alicePage, alice),
                Fixture.LoginAsAsync(bobPage, bob));

            await Task.WhenAll(
                Fixture.GoToAndWaitForAppReadyAsync(alicePage, "/multiplayer/quick-match"),
                Fixture.GoToAndWaitForAppReadyAsync(bobPage, "/multiplayer/quick-match"));

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });

            await alicePage.GetByTestId(Selectors.Multiplayer.RealtimeForfeit).ClickAsync();

            await Expect(alicePage).ToHaveURLAsync(new Regex(".*/multiplayer/result/[0-9a-fA-F-]{36}"), new() { Timeout = 10_000 });
            await Expect(bobPage).ToHaveURLAsync(new Regex(".*/multiplayer/result/[0-9a-fA-F-]{36}"), new() { Timeout = 10_000 });

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("PROHRA");
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultXp)).ToContainTextAsync("+30 XP");
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultLeagueXp)).ToContainTextAsync("+15 XP");

            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("VÍTĚZSTVÍ");
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultXp)).ToContainTextAsync("+100 XP");
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultLeagueXp)).ToContainTextAsync("+50 XP");

            await Fixture.TakeCheckpointScreenshotAsync(
                bobPage,
                area: "multiplayer",
                scenario,
                state: "bob-victory-result",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserB");

            await Fixture.GoToAndWaitForAppReadyAsync(bobPage, "/multiplayer/history");
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.HistoryPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.HistoryStatsPlayed)).ToContainTextAsync("1");
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.HistoryStatsWins)).ToContainTextAsync("1");

            var historyRows = bobPage.GetByTestId(Selectors.Multiplayer.HistoryMatchRow);
            await Expect(historyRows).ToHaveCountAsync(1);
            await Expect(historyRows.First).ToContainTextAsync("Výhra");
            await Expect(historyRows.First).ToContainTextAsync(alice.Username);
            await Expect(historyRows.First).ToContainTextAsync("+100");

            await Fixture.TakeCheckpointScreenshotAsync(
                bobPage,
                area: "multiplayer",
                scenario,
                state: "bob-history-after-forfeit",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserB");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(alicePage, "multiplayer", $"{scenario}-alice");
            await Fixture.TakeFailureArtifactsAsync(bobPage, "multiplayer", $"{scenario}-bob");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await alicePage.Context.CloseAsync();
            await bobPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task QuickMatch_WinnerByCorrectCount_CompletesMatchAndShowsVictory()
    {
        const string scenario = "quickmatch-winner-by-correct-count";
        await Fixture.ResetDatabaseAsync();

        var alice = await Fixture.RegisterUniqueUserAsync("mpwinalice");
        var bob = await Fixture.RegisterUniqueUserAsync("mpwinbob");
        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 4);
        await Fixture.ForceUserStatsAsync(bob.Email, totalXp: 390, level: 4);

        var alicePage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.alice");
        var bobPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.bob");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(alicePage, alice),
                Fixture.LoginAsAsync(bobPage, bob));

            await Task.WhenAll(
                Fixture.GoToAndWaitForAppReadyAsync(alicePage, "/multiplayer/quick-match"),
                Fixture.GoToAndWaitForAppReadyAsync(bobPage, "/multiplayer/quick-match"));

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });

            for (var solved = 0; solved < 15; solved++)
            {
                var answer = await GetCurrentRealtimeAnswerAsync(alicePage);
                await alicePage.GetByTestId(Selectors.Multiplayer.RealtimeAnswerInput).FillAsync(answer);
                await alicePage.GetByTestId(Selectors.Multiplayer.RealtimeSubmit).ClickAsync();

                if (solved < 14)
                {
                    await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimePlayerScore))
                        .ToContainTextAsync($"{solved + 1}/15", new() { Timeout = 8_000 });
                }
            }

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 12_000 });
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("VÍTĚZSTVÍ");
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultXp)).ToContainTextAsync("+100 XP");
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultLeagueXp)).ToContainTextAsync("+50 XP");

            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 12_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("PROHRA");

            await Fixture.TakeCheckpointScreenshotAsync(
                alicePage,
                area: "multiplayer",
                scenario,
                state: "alice-victory-result",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserA");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(alicePage, "multiplayer", $"{scenario}-alice");
            await Fixture.TakeFailureArtifactsAsync(bobPage, "multiplayer", $"{scenario}-bob");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await alicePage.Context.CloseAsync();
            await bobPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task QuickMatch_TieBySpeed_FasterPlayerWinsAndResultPageShowsTiebreaker()
    {
        const string scenario = "quickmatch-tie-by-speed";
        await Fixture.ResetDatabaseAsync();

        var alice = await Fixture.RegisterUniqueUserAsync("mpspeedalice");
        var bob = await Fixture.RegisterUniqueUserAsync("mpspeedbob");
        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 4);
        await Fixture.ForceUserStatsAsync(bob.Email, totalXp: 390, level: 4);

        await using var aliceConnection = await CreateStartedHubConnectionAsync(alice);
        await using var bobConnection = await CreateStartedHubConnectionAsync(bob);

        var aliceRounds = Channel.CreateUnbounded<MultiplayerRoundDto>();
        var bobRounds = Channel.CreateUnbounded<MultiplayerRoundDto>();
        var aliceMatchTcs = new TaskCompletionSource<MatchFoundEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bobMatchTcs = new TaskCompletionSource<MatchFoundEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var aliceResultTcs = new TaskCompletionSource<MatchResultDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bobResultTcs = new TaskCompletionSource<MatchResultDto>(TaskCreationOptions.RunContinuationsAsynchronously);

        aliceConnection.On<MatchFoundEvent>("MatchFound", match => aliceMatchTcs.TrySetResult(match));
        bobConnection.On<MatchFoundEvent>("MatchFound", match => bobMatchTcs.TrySetResult(match));
        aliceConnection.On<MultiplayerRoundDto>("RoundStarted", round => aliceRounds.Writer.TryWrite(round));
        bobConnection.On<MultiplayerRoundDto>("RoundStarted", round => bobRounds.Writer.TryWrite(round));
        aliceConnection.On<MatchResultDto>("MatchEnded", result => aliceResultTcs.TrySetResult(result));
        bobConnection.On<MatchResultDto>("MatchEnded", result => bobResultTcs.TrySetResult(result));

        (await aliceConnection.InvokeAsync<bool>("JoinMatchmaking")).Should().BeTrue();
        (await bobConnection.InvokeAsync<bool>("JoinMatchmaking")).Should().BeTrue();

        var aliceMatch = await AwaitAsync(aliceMatchTcs.Task, TimeSpan.FromSeconds(10), "Alice should receive MatchFound");
        var bobMatch = await AwaitAsync(bobMatchTcs.Task, TimeSpan.FromSeconds(10), "Bob should receive MatchFound");
        aliceMatch.MatchId.Should().Be(bobMatch.MatchId);

        var aliceRound1 = await AwaitRoundAsync(aliceRounds.Reader, 1);
        var bobRound1 = await AwaitRoundAsync(bobRounds.Reader, 1);
        await aliceConnection.InvokeAsync("SubmitAnswer", await GetAnswerForRoundAsync(aliceRound1), 1000);
        await bobConnection.InvokeAsync("SubmitAnswer", await GetAnswerForRoundAsync(bobRound1), 5000);

        for (var roundNumber = 2; roundNumber <= 15; roundNumber++)
        {
            await AwaitRoundAsync(aliceRounds.Reader, roundNumber);
            await aliceConnection.InvokeAsync("SubmitAnswer", "__wrong__", 1);

            await AwaitRoundAsync(bobRounds.Reader, roundNumber);
            await bobConnection.InvokeAsync("SubmitAnswer", "__wrong__", 1);
        }

        var aliceResult = await AwaitAsync(aliceResultTcs.Task, TimeSpan.FromSeconds(10), "Alice should receive speed tiebreak result");
        var bobResult = await AwaitAsync(bobResultTcs.Task, TimeSpan.FromSeconds(10), "Bob should receive speed tiebreak result");

        aliceResult.DidYouWin.Should().BeTrue();
        aliceResult.YourScore.Should().Be(1);
        aliceResult.OpponentScore.Should().Be(1);
        aliceResult.YourTime.Should().BeLessThan(aliceResult.OpponentTime);
        aliceResult.XPEarned.Should().Be(100);
        bobResult.DidYouWin.Should().BeFalse();
        bobResult.YourScore.Should().Be(1);
        bobResult.OpponentScore.Should().Be(1);

        var page = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.alice-result");
        try
        {
            await Fixture.LoginAsAsync(page, alice);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/multiplayer/result/{aliceMatch.MatchId}");
            await Expect(page.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("VÍTĚZSTVÍ");
            await Expect(page.GetByTestId(Selectors.Multiplayer.ResultXp)).ToContainTextAsync("+100 XP");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "multiplayer",
                scenario,
                state: "alice-speed-tiebreaker-result",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserA");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(page, "multiplayer", $"{scenario}-alice-result");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task QuickMatch_Draw_WhenCorrectCountAndTimeAreEqual_ShowsDrawResult()
    {
        const string scenario = "quickmatch-draw-result";
        await Fixture.ResetDatabaseAsync();

        var alice = await Fixture.RegisterUniqueUserAsync("mpdrawalice");
        var bob = await Fixture.RegisterUniqueUserAsync("mpdrawbob");
        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 4);
        await Fixture.ForceUserStatsAsync(bob.Email, totalXp: 390, level: 4);

        await using var aliceConnection = await CreateStartedHubConnectionAsync(alice);
        await using var bobConnection = await CreateStartedHubConnectionAsync(bob);

        var aliceRounds = Channel.CreateUnbounded<MultiplayerRoundDto>();
        var bobRounds = Channel.CreateUnbounded<MultiplayerRoundDto>();
        var aliceMatchTcs = new TaskCompletionSource<MatchFoundEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bobMatchTcs = new TaskCompletionSource<MatchFoundEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var aliceResultTcs = new TaskCompletionSource<MatchResultDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bobResultTcs = new TaskCompletionSource<MatchResultDto>(TaskCreationOptions.RunContinuationsAsynchronously);

        aliceConnection.On<MatchFoundEvent>("MatchFound", match => aliceMatchTcs.TrySetResult(match));
        bobConnection.On<MatchFoundEvent>("MatchFound", match => bobMatchTcs.TrySetResult(match));
        aliceConnection.On<MultiplayerRoundDto>("RoundStarted", round => aliceRounds.Writer.TryWrite(round));
        bobConnection.On<MultiplayerRoundDto>("RoundStarted", round => bobRounds.Writer.TryWrite(round));
        aliceConnection.On<MatchResultDto>("MatchEnded", result => aliceResultTcs.TrySetResult(result));
        bobConnection.On<MatchResultDto>("MatchEnded", result => bobResultTcs.TrySetResult(result));

        (await aliceConnection.InvokeAsync<bool>("JoinMatchmaking")).Should().BeTrue();
        (await bobConnection.InvokeAsync<bool>("JoinMatchmaking")).Should().BeTrue();

        var aliceMatch = await AwaitAsync(aliceMatchTcs.Task, TimeSpan.FromSeconds(10), "Alice should receive MatchFound");
        var bobMatch = await AwaitAsync(bobMatchTcs.Task, TimeSpan.FromSeconds(10), "Bob should receive MatchFound");
        aliceMatch.MatchId.Should().Be(bobMatch.MatchId);

        var aliceRound1 = await AwaitRoundAsync(aliceRounds.Reader, 1);
        var bobRound1 = await AwaitRoundAsync(bobRounds.Reader, 1);
        await aliceConnection.InvokeAsync("SubmitAnswer", await GetAnswerForRoundAsync(aliceRound1), 1000);
        await bobConnection.InvokeAsync("SubmitAnswer", await GetAnswerForRoundAsync(bobRound1), 1000);

        for (var roundNumber = 2; roundNumber <= 15; roundNumber++)
        {
            await AwaitRoundAsync(aliceRounds.Reader, roundNumber);
            await aliceConnection.InvokeAsync("SubmitAnswer", "__wrong__", 0);

            await AwaitRoundAsync(bobRounds.Reader, roundNumber);
            await bobConnection.InvokeAsync("SubmitAnswer", "__wrong__", 0);
        }

        var aliceResult = await AwaitAsync(aliceResultTcs.Task, TimeSpan.FromSeconds(10), "Alice should receive draw result");
        var bobResult = await AwaitAsync(bobResultTcs.Task, TimeSpan.FromSeconds(10), "Bob should receive draw result");

        aliceResult.IsDraw.Should().BeTrue();
        aliceResult.WinnerId.Should().BeNull();
        aliceResult.YourScore.Should().Be(1);
        aliceResult.OpponentScore.Should().Be(1);
        aliceResult.YourTime.Should().Be(aliceResult.OpponentTime);
        bobResult.IsDraw.Should().BeTrue();
        bobResult.WinnerId.Should().BeNull();

        var page = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.alice-result");
        try
        {
            await Fixture.LoginAsAsync(page, alice);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/multiplayer/result/{aliceMatch.MatchId}");
            await Expect(page.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("REMÍZA");
            await Expect(page.GetByText("Rychlejší vyhrává")).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "multiplayer",
                scenario,
                state: "alice-draw-result",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserA");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(page, "multiplayer", $"{scenario}-alice-result");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task QuickMatch_TimerExpiry_CompletesMatchAsDrawAndShowsResult()
    {
        const string scenario = "quickmatch-timer-expiry";
        await Fixture.ResetDatabaseAsync();
        await Fixture.SetQuickMatchTimeLimitAsync(5);

        var alice = await Fixture.RegisterUniqueUserAsync("mptimeralice");
        var bob = await Fixture.RegisterUniqueUserAsync("mptimerbob");
        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 4);
        await Fixture.ForceUserStatsAsync(bob.Email, totalXp: 390, level: 4);

        var alicePage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.alice");
        var bobPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.bob");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(alicePage, alice),
                Fixture.LoginAsAsync(bobPage, bob));

            await Task.WhenAll(
                Fixture.GoToAndWaitForAppReadyAsync(alicePage, "/multiplayer/quick-match"),
                Fixture.GoToAndWaitForAppReadyAsync(bobPage, "/multiplayer/quick-match"));

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimeTimer))
                .ToContainTextAsync(new Regex("0:0[1-5]"), new() { Timeout = 8_000 });

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 15_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 15_000 });

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("REMÍZA");
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultXp)).ToContainTextAsync("+30 XP");
            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.ResultLeagueXp)).ToContainTextAsync("+15 XP");
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("REMÍZA");

            await Fixture.TakeCheckpointScreenshotAsync(
                alicePage,
                area: "multiplayer",
                scenario,
                state: "alice-expired-draw-result",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserA");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(alicePage, "multiplayer", $"{scenario}-alice");
            await Fixture.TakeFailureArtifactsAsync(bobPage, "multiplayer", $"{scenario}-bob");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await alicePage.Context.CloseAsync();
            await bobPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task QuickMatch_DisconnectGrace_ForfeitsAfterThirtySecondsAndAwardsOpponent()
    {
        const string scenario = "quickmatch-disconnect-grace";
        await Fixture.ResetDatabaseAsync();

        var alice = await Fixture.RegisterUniqueUserAsync("mpdisconnectalice");
        var bob = await Fixture.RegisterUniqueUserAsync("mpdisconnectbob");
        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 4);
        await Fixture.ForceUserStatsAsync(bob.Email, totalXp: 390, level: 4);

        var alicePage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.alice");
        var bobPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.bob");
        var aliceClosed = false;

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(alicePage, alice),
                Fixture.LoginAsAsync(bobPage, bob));

            await Task.WhenAll(
                Fixture.GoToAndWaitForAppReadyAsync(alicePage, "/multiplayer/quick-match"),
                Fixture.GoToAndWaitForAppReadyAsync(bobPage, "/multiplayer/quick-match"));

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });

            await alicePage.Context.CloseAsync();
            aliceClosed = true;

            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeFeedback)).ToContainTextAsync("Čekání na soupeře", new() { Timeout = 10_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultPage)).Not.ToBeVisibleAsync(new() { Timeout = 5_000 });

            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 40_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("VÍTĚZSTVÍ");
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultXp)).ToContainTextAsync("+100 XP");
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultLeagueXp)).ToContainTextAsync("+50 XP");

            await Fixture.TakeCheckpointScreenshotAsync(
                bobPage,
                area: "multiplayer",
                scenario,
                state: "bob-victory-after-disconnect",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserB");
        }
        catch
        {
            if (!aliceClosed)
            {
                await Fixture.TakeFailureArtifactsAsync(alicePage, "multiplayer", $"{scenario}-alice");
            }

            await Fixture.TakeFailureArtifactsAsync(bobPage, "multiplayer", $"{scenario}-bob");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            if (!aliceClosed)
            {
                await alicePage.Context.CloseAsync();
            }

            await bobPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task QuickMatch_ReconnectWithinGrace_RestoresMatchAndPreventsForfeit()
    {
        const string scenario = "quickmatch-reconnect-within-grace";
        await Fixture.ResetDatabaseAsync();

        var alice = await Fixture.RegisterUniqueUserAsync("mpreconnectalice");
        var bob = await Fixture.RegisterUniqueUserAsync("mpreconnectbob");
        await Fixture.ForceUserStatsAsync(alice.Email, totalXp: 420, level: 4);
        await Fixture.ForceUserStatsAsync(bob.Email, totalXp: 390, level: 4);

        var alicePage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.alice");
        var bobPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.bob");
        IPage? reconnectedAlicePage = null;
        var aliceClosed = false;

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(alicePage, alice),
                Fixture.LoginAsAsync(bobPage, bob));

            await Task.WhenAll(
                Fixture.GoToAndWaitForAppReadyAsync(alicePage, "/multiplayer/quick-match"),
                Fixture.GoToAndWaitForAppReadyAsync(bobPage, "/multiplayer/quick-match"));

            await Expect(alicePage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });

            var matchPath = new Uri(alicePage.Url).PathAndQuery;
            await alicePage.Context.CloseAsync();
            aliceClosed = true;

            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeFeedback)).ToContainTextAsync("Čekání na soupeře", new() { Timeout = 10_000 });

            reconnectedAlicePage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.alice-reconnected");
            await Fixture.LoginAsAsync(reconnectedAlicePage, alice);
            await Fixture.GoToAndWaitForAppReadyAsync(reconnectedAlicePage, matchPath);

            await Expect(reconnectedAlicePage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(reconnectedAlicePage.GetByTestId(Selectors.Multiplayer.RealtimeScrambledWord)).ToBeVisibleAsync(new() { Timeout = 8_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultPage)).Not.ToBeVisibleAsync(new() { Timeout = 1_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                reconnectedAlicePage,
                area: "multiplayer",
                scenario,
                state: "alice-reconnected-game",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerUserA");

            await bobPage.WaitForTimeoutAsync(32_000);
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync();
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultPage)).Not.ToBeVisibleAsync(new() { Timeout = 1_000 });

            await reconnectedAlicePage.GetByTestId(Selectors.Multiplayer.RealtimeForfeit).ClickAsync();
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(bobPage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("VÍTĚZSTVÍ");
        }
        catch
        {
            if (!aliceClosed)
            {
                await Fixture.TakeFailureArtifactsAsync(alicePage, "multiplayer", $"{scenario}-alice");
            }

            if (reconnectedAlicePage != null)
            {
                await Fixture.TakeFailureArtifactsAsync(reconnectedAlicePage, "multiplayer", $"{scenario}-alice-reconnected");
            }

            await Fixture.TakeFailureArtifactsAsync(bobPage, "multiplayer", $"{scenario}-bob");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            if (!aliceClosed)
            {
                await alicePage.Context.CloseAsync();
            }

            if (reconnectedAlicePage != null)
            {
                await reconnectedAlicePage.Context.CloseAsync();
            }

            await bobPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_CreateModal_SelectsSettingsShowsRoomCodeAndCopiesIt()
    {
        const string scenario = "private-room-create-settings-code-copy";

        await RunScenarioAsync("multiplayer", scenario, async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("privateroomhost");
            await Fixture.LoginAsAsync(page, user);
            await page.Context.GrantPermissionsAsync(
                new[] { "clipboard-read", "clipboard-write" },
                new() { Origin = Fixture.BaseUrl });

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/multiplayer");
            await page.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateWordCount10)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateWordCount15)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateWordCount20)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateTimeLimit2)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateTimeLimit3)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateTimeLimit5)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateDifficulty)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateBestOf1)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateBestOf3)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateBestOf5)).ToBeVisibleAsync();

            await page.GetByTestId(Selectors.Multiplayer.PrivateWordCount20).ClickAsync();
            await page.GetByTestId(Selectors.Multiplayer.PrivateTimeLimit5).ClickAsync();
            await page.GetByTestId(Selectors.Multiplayer.PrivateBestOf3).ClickAsync();
            await page.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();

            await Expect(page).ToHaveURLAsync(new Regex(".*/multiplayer/room/LEXIQ-[A-Z0-9]{4}"), new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateRoomCode)).ToHaveTextAsync(new Regex("^LEXIQ-[A-Z0-9]{4}$"));
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateSettingsWordCount)).ToContainTextAsync("20");
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateSettingsTimeLimit)).ToContainTextAsync("5");
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateSettingsBestOf)).ToContainTextAsync("Na 3 hry");

            var roomCode = (await page.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();
            await page.GetByTestId(Selectors.Multiplayer.PrivateCopyCode).ClickAsync();
            var clipboardText = await page.EvaluateAsync<string>("navigator.clipboard.readText()");
            clipboardText.Should().Be(roomCode);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "multiplayer",
                scenario,
                state: "host-lobby-code-settings",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task PrivateRoom_InvalidSettings_AreRejectedByHub()
    {
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("privateroominvalid");
        await using var connection = await CreateStartedHubConnectionAsync(host);

        var failureTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var createdTcs = new TaskCompletionSource<RoomCreatedEvent>(TaskCreationOptions.RunContinuationsAsynchronously);

        connection.On<string>("RoomCreationFailed", error => failureTcs.TrySetResult(error));
        connection.On<RoomCreatedEvent>("RoomCreated", room => createdTcs.TrySetResult(room));

        await connection.InvokeAsync(
            "CreateRoom",
            new RoomSettingsDto(
                WordCount: 12,
                TimeLimitMinutes: 4,
                Difficulty: DifficultyLevel.Beginner,
                BestOf: 2));

        var error = await AwaitAsync(
            failureTcs.Task,
            TimeSpan.FromSeconds(5),
            "invalid private room settings must be rejected by the hub");

        error.Should().NotBeNullOrWhiteSpace();
        error.Should().Contain("Počet slov");
        error.Should().Contain("Časový limit");
        error.Should().Contain("Série");

        var created = await Task.WhenAny(createdTcs.Task, Task.Delay(TimeSpan.FromMilliseconds(750)));
        created.Should().NotBe(createdTcs.Task, "invalid settings must not create a room");
    }

    [Fact]
    public async Task PrivateRoom_JoinValidRoom_ShowsBothPlayersInLobby()
    {
        const string scenario = "private-room-join-valid";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("privateroomhostjoin");
        var guest = await Fixture.RegisterUniqueUserAsync("privateroomguestjoin");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();

            await Expect(hostPage).ToHaveURLAsync(new Regex(".*/multiplayer/room/LEXIQ-[A-Z0-9]{4}"), new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode.ToLowerInvariant());
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();

            await Expect(guestPage).ToHaveURLAsync(new Regex($".*/multiplayer/room/{roomCode}$"), new() { Timeout = 10_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer)).ToHaveCountAsync(2, new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer)).ToHaveCountAsync(2, new() { Timeout = 10_000 });

            var hostLobbyNames = await hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayerName).AllInnerTextsAsync();
            var guestLobbyNames = await guestPage.GetByTestId(Selectors.Multiplayer.PrivatePlayerName).AllInnerTextsAsync();
            hostLobbyNames.Should().Contain(host.Username);
            hostLobbyNames.Should().Contain(guest.Username);
            guestLobbyNames.Should().Contain(host.Username);
            guestLobbyNames.Should().Contain(guest.Username);

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-sees-guest",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");

            await Fixture.TakeCheckpointScreenshotAsync(
                guestPage,
                area: "multiplayer",
                scenario,
                state: "guest-joined-lobby",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerGuest");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_JoinInvalidCode_ShowsValidationAndNotFoundErrors()
    {
        const string scenario = "private-room-join-invalid-code";

        await RunScenarioAsync("multiplayer", scenario, async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("privateroombadjoin");
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/multiplayer");
            await page.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await page.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync("abc");
            await page.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateJoinValidation)).ToContainTextAsync("LEXIQ-XXXX");
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync();
            page.Url.Should().EndWith("/multiplayer");

            await page.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync("LEXIQ-ZZZZ");
            await page.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Multiplayer.PrivateRoomError)).ToContainTextAsync("Místnost nenalezena", new() { Timeout = 10_000 });
            page.Url.Should().EndWith("/multiplayer");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "multiplayer",
                scenario,
                state: "not-found-error",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerGuest");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task PrivateRoom_JoinExpiredCode_ShowsExpiredError()
    {
        const string scenario = "private-room-join-expired-code";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prexphost");
        var guest = await Fixture.RegisterUniqueUserAsync("prexpguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();
            await Fixture.ExpirePrivateRoomAsync(roomCode);

            await Fixture.LoginAsAsync(guestPage, guest);
            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();

            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateRoomError)).ToContainTextAsync("Místnost vypršela", new() { Timeout = 10_000 });
            guestPage.Url.Should().EndWith("/multiplayer");

            await Fixture.TakeCheckpointScreenshotAsync(
                guestPage,
                area: "multiplayer",
                scenario,
                state: "expired-error",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerGuest");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_ExpiredRoomCleanup_RemovesOldCodeAndReleasesHost()
    {
        const string scenario = "private-room-expiry-cleanup";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prcleanuphost");
        var guest = await Fixture.RegisterUniqueUserAsync("prcleanupguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Fixture.LoginAsAsync(hostPage, host);
            await Fixture.LoginAsAsync(guestPage, guest);

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var oldRoomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();
            await Fixture.ExpirePrivateRoomAsync(oldRoomCode);
            await Fixture.RunRoomCleanupAsync();

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var newRoomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();
            newRoomCode.Should().MatchRegex("^LEXIQ-[A-Z0-9]{4}$");
            newRoomCode.Should().NotBe(oldRoomCode);

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-new-room-after-cleanup",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(oldRoomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();

            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateRoomError)).ToContainTextAsync("Místnost nenalezena", new() { Timeout = 10_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                guestPage,
                area: "multiplayer",
                scenario,
                state: "old-code-not-found-after-cleanup",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerGuest");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_JoinFullRoom_ShowsFullError()
    {
        const string scenario = "private-room-join-full-room";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prfullhost");
        var firstGuest = await Fixture.RegisterUniqueUserAsync("prfullg1");
        var secondGuest = await Fixture.RegisterUniqueUserAsync("prfullg2");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var firstGuestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest1");
        var secondGuestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest2");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(firstGuestPage, firstGuest),
                Fixture.LoginAsAsync(secondGuestPage, secondGuest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(firstGuestPage, "/multiplayer");
            await firstGuestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(firstGuestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await firstGuestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await firstGuestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(firstGuestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer)).ToHaveCountAsync(2, new() { Timeout = 10_000 });

            await Fixture.GoToAndWaitForAppReadyAsync(secondGuestPage, "/multiplayer");
            await secondGuestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(secondGuestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await secondGuestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await secondGuestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();

            await Expect(secondGuestPage.GetByTestId(Selectors.Multiplayer.PrivateRoomError)).ToContainTextAsync("Místnost je plná", new() { Timeout = 10_000 });
            secondGuestPage.Url.Should().EndWith("/multiplayer");

            await Fixture.TakeCheckpointScreenshotAsync(
                secondGuestPage,
                area: "multiplayer",
                scenario,
                state: "full-room-error",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerGuest");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(firstGuestPage, "multiplayer", $"{scenario}-guest1");
            await Fixture.TakeFailureArtifactsAsync(secondGuestPage, "multiplayer", $"{scenario}-guest2");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await firstGuestPage.Context.CloseAsync();
            await secondGuestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_UserCannotCreateTwoActiveRooms()
    {
        const string scenario = "private-room-single-active-room";
        await Fixture.ResetDatabaseAsync();

        var user = await Fixture.RegisterUniqueUserAsync("prsingle");
        var firstPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.first");
        var secondPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.second");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(firstPage, user),
                Fixture.LoginAsAsync(secondPage, user));

            await Fixture.GoToAndWaitForAppReadyAsync(firstPage, "/multiplayer");
            await firstPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(firstPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await firstPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(firstPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Fixture.GoToAndWaitForAppReadyAsync(secondPage, "/multiplayer");
            await secondPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(secondPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await secondPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();

            await Expect(secondPage.GetByTestId(Selectors.Multiplayer.PrivateRoomError)).ToContainTextAsync("Už máš aktivní místnost", new() { Timeout = 10_000 });
            secondPage.Url.Should().EndWith("/multiplayer");

            await Fixture.TakeCheckpointScreenshotAsync(
                secondPage,
                area: "multiplayer",
                scenario,
                state: "second-room-blocked",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(firstPage, "multiplayer", $"{scenario}-first");
            await Fixture.TakeFailureArtifactsAsync(secondPage, "multiplayer", $"{scenario}-second");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await firstPage.Context.CloseAsync();
            await secondPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_HostLeave_CancelsRoomAndNotifiesGuest()
    {
        const string scenario = "private-room-host-leave-cancels-room";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prleavehost");
        var guest = await Fixture.RegisterUniqueUserAsync("prleaveguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateLeave).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateRoomError)).ToContainTextAsync("Místnost byla zrušena", new() { Timeout = 10_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                guestPage,
                area: "multiplayer",
                scenario,
                state: "guest-room-cancelled",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerGuest");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_GuestLeave_RemovesGuestAndHostReturnsToWaiting()
    {
        const string scenario = "private-room-guest-leave-removes-guest";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prgleavehost");
        var guest = await Fixture.RegisterUniqueUserAsync("prgleaveguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer)).ToHaveCountAsync(2, new() { Timeout = 10_000 });

            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateLeave).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer)).ToHaveCountAsync(1, new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode)).ToHaveTextAsync(roomCode);

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-waiting-after-guest-left",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_ReadyToggle_SetsAndCancelsReadyState()
    {
        const string scenario = "private-room-ready-toggle";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prreadyhost");
        var guest = await Fixture.RegisterUniqueUserAsync("prreadyguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCancelReady)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer).Filter(new() { HasText = host.Username })).ToContainTextAsync("Připraven");
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer).Filter(new() { HasText = host.Username })).ToContainTextAsync("Připraven", new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCancelReady).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateReady)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer).Filter(new() { HasText = host.Username })).ToContainTextAsync("Čeká", new() { Timeout = 10_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-ready-cancelled",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_BothReady_StartsCountdownForBothPlayers()
    {
        const string scenario = "private-room-both-ready-countdown";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prcounthost");
        var guest = await Fixture.RegisterUniqueUserAsync("prcountguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCountdown)).ToContainTextAsync(new Regex("[123]"), new() { Timeout = 8_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateCountdown)).ToContainTextAsync(new Regex("[123]"), new() { Timeout = 8_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "countdown-started",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_LobbyChat_SendsMessageToBothPlayers()
    {
        const string scenario = "private-room-lobby-chat-send";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prchathost");
        var guest = await Fixture.RegisterUniqueUserAsync("prchatguest");
        var message = $"Ahoj z chatu {Guid.NewGuid():N}"[..28];

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer)).ToHaveCountAsync(2, new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatInput).FillAsync(message);
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatSend)).ToBeEnabledAsync();
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatSend).ClickAsync();

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessageText).Filter(new() { HasText = message }))
                .ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessageText).Filter(new() { HasText = message }))
                .ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatInput)).ToHaveValueAsync(string.Empty);

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-message-visible",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_LobbyChat_EmptyMessage_IsRejected()
    {
        const string scenario = "private-room-lobby-chat-empty-message";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("premptyhost");
        var guest = await Fixture.RegisterUniqueUserAsync("premptyguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer)).ToHaveCountAsync(2, new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatInput).FillAsync("   ");
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatSend)).ToBeDisabledAsync();
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatInput).PressAsync("Enter");

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessage)).ToHaveCountAsync(0);
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessage)).ToHaveCountAsync(0);

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "whitespace-rejected",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_LobbyChat_MaxTwoHundredCharacters_IsEnforced()
    {
        const string scenario = "private-room-lobby-chat-max-length";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prmaxhost");
        var guest = await Fixture.RegisterUniqueUserAsync("prmaxguest");
        var overLimitMessage = new string('a', 210);
        var expectedMessage = new string('a', 200);

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer)).ToHaveCountAsync(2, new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatInput).FillAsync(overLimitMessage);
            var value = await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatInput).InputValueAsync();
            value.Should().HaveLength(200);
            value.Should().Be(expectedMessage);

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatSend).ClickAsync();

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessageText).Filter(new() { HasText = expectedMessage }))
                .ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessageText).Filter(new() { HasText = expectedMessage }))
                .ToBeVisibleAsync(new() { Timeout = 10_000 });

            var hostText = await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessageText).Last.InnerTextAsync();
            var guestText = await guestPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessageText).Last.InnerTextAsync();
            hostText.Should().HaveLength(200);
            guestText.Should().HaveLength(200);

            var chatBox = await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatSection).BoundingBoxAsync();
            var messageBox = await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessageText).Last.BoundingBoxAsync();
            chatBox.Should().NotBeNull();
            messageBox.Should().NotBeNull();
            (messageBox!.X + messageBox.Width).Should().BeLessThanOrEqualTo(chatBox!.X + chatBox.Width + 1);

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "message-truncated-to-max",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_LobbyChat_RateLimit_ShowsLocalizedErrorAndKeepsLobby()
    {
        const string scenario = "private-room-lobby-chat-rate-limit";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prratehost");
        var guest = await Fixture.RegisterUniqueUserAsync("prrateguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer)).ToHaveCountAsync(2, new() { Timeout = 10_000 });

            for (var index = 1; index <= 10; index++)
            {
                var message = $"Rychla zprava {index}";
                await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatInput).FillAsync(message);
                await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatSend).ClickAsync();
                await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessageText).Filter(new() { HasText = message }))
                    .ToBeVisibleAsync(new() { Timeout = 10_000 });
            }

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessage)).ToHaveCountAsync(10);

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatInput).FillAsync("Tahle zprava uz je moc rychla");
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatSend).ClickAsync();

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatError))
                .ToContainTextAsync("Posíláš zprávy příliš rychle", new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessage)).ToHaveCountAsync(10);

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "rate-limit-error",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_LobbyChat_XssPayload_IsDisplayedEscaped()
    {
        const string scenario = "private-room-lobby-chat-xss-escaped";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prxsshost");
        var guest = await Fixture.RegisterUniqueUserAsync("prxssguest");
        const string payload = "<img src=x onerror=\"window.__lexiquestXss=1\">";
        const string escapedPayload = "&lt;img src=x onerror=&quot;window.__lexiquestXss=1&quot;&gt;";

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Task.WhenAll(
                hostPage.EvaluateAsync("() => { window.__lexiquestXss = 0; }"),
                guestPage.EvaluateAsync("() => { window.__lexiquestXss = 0; }"));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivatePlayer)).ToHaveCountAsync(2, new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatInput).FillAsync(payload);
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatSend).ClickAsync();

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessageText).Filter(new() { HasText = escapedPayload }))
                .ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateChatMessageText).Filter(new() { HasText = escapedPayload }))
                .ToBeVisibleAsync(new() { Timeout = 10_000 });

            var hostInjectedImages = await hostPage.Locator("img[onerror]").CountAsync();
            var guestInjectedImages = await guestPage.Locator("img[onerror]").CountAsync();
            hostInjectedImages.Should().Be(0);
            guestInjectedImages.Should().Be(0);

            var hostXss = await hostPage.EvaluateAsync<int>("() => window.__lexiquestXss || 0");
            var guestXss = await guestPage.EvaluateAsync<int>("() => window.__lexiquestXss || 0");
            hostXss.Should().Be(0);
            guestXss.Should().Be(0);

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "payload-escaped",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_BestOf3_BothReady_NavigatesBothPlayersToRealtimeGame()
    {
        const string scenario = "private-room-best-of3-starts-realtime-game";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prbo3host");
        var guest = await Fixture.RegisterUniqueUserAsync("prbo3guest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateBestOf3).ClickAsync();
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateSettingsBestOf)).ToContainTextAsync("Na 3 hry");
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();

            await Expect(hostPage).ToHaveURLAsync(new Regex(".*/multiplayer/game/[0-9a-fA-F-]{36}"), new() { Timeout = 12_000 });
            await Expect(guestPage).ToHaveURLAsync(new Regex(".*/multiplayer/game/[0-9a-fA-F-]{36}"), new() { Timeout = 12_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-realtime-game",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_BestOf3_CompletedMatch_ShowsSeriesScore()
    {
        const string scenario = "private-room-best-of3-series-score";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prbo3scorehost");
        var guest = await Fixture.RegisterUniqueUserAsync("prbo3scoreguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateWordCount10).ClickAsync();
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateBestOf3).ClickAsync();
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });

            for (var solved = 0; solved < 10; solved++)
            {
                var answer = await GetCurrentRealtimeAnswerAsync(hostPage);
                await hostPage.GetByTestId(Selectors.Multiplayer.RealtimeAnswerInput).FillAsync(answer);
                await hostPage.GetByTestId(Selectors.Multiplayer.RealtimeSubmit).ClickAsync();

                if (solved < 9)
                {
                    await Expect(hostPage.GetByTestId(Selectors.Multiplayer.RealtimePlayerScore))
                        .ToContainTextAsync($"{solved + 1}/", new() { Timeout = 8_000 });
                }
            }

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 12_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("VÍTĚZSTVÍ");
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultXp)).ToContainTextAsync("+100 XP");
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultLeagueXp)).ToHaveCountAsync(0);
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultSeriesScore)).ToContainTextAsync("Série: 1:0");

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-result-series-score",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_BestOf5_CompletedMatch_ShowsSeriesScore()
    {
        const string scenario = "private-room-best-of5-series-score";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prbo5scorehost");
        var guest = await Fixture.RegisterUniqueUserAsync("prbo5scoreguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateWordCount10).ClickAsync();
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateBestOf5).ClickAsync();
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateSettingsBestOf)).ToContainTextAsync("Na 5 her");
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });

            for (var solved = 0; solved < 10; solved++)
            {
                var answer = await GetCurrentRealtimeAnswerAsync(hostPage);
                await hostPage.GetByTestId(Selectors.Multiplayer.RealtimeAnswerInput).FillAsync(answer);
                await hostPage.GetByTestId(Selectors.Multiplayer.RealtimeSubmit).ClickAsync();

                if (solved < 9)
                {
                    await Expect(hostPage.GetByTestId(Selectors.Multiplayer.RealtimePlayerScore))
                        .ToContainTextAsync($"{solved + 1}/", new() { Timeout = 8_000 });
                }
            }

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 12_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("VÍTĚZSTVÍ");
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultLeagueXp)).ToHaveCountAsync(0);
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultSeriesScore)).ToContainTextAsync("Série: 1:0");

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-result-series-score",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_CompletedMatch_DoesNotAwardLeagueXp()
    {
        const string scenario = "private-room-no-league-xp";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prnoleaguehost");
        var guest = await Fixture.RegisterUniqueUserAsync("prnoleagueguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
            await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateWordCount10).ClickAsync();
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateBestOf1).ClickAsync();
            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

            await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
            await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();
            await guestPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });

            for (var solved = 0; solved < 10; solved++)
            {
                var answer = await GetCurrentRealtimeAnswerAsync(hostPage);
                await hostPage.GetByTestId(Selectors.Multiplayer.RealtimeAnswerInput).FillAsync(answer);
                await hostPage.GetByTestId(Selectors.Multiplayer.RealtimeSubmit).ClickAsync();

                if (solved < 9)
                {
                    await Expect(hostPage.GetByTestId(Selectors.Multiplayer.RealtimePlayerScore))
                        .ToContainTextAsync($"{solved + 1}/", new() { Timeout = 8_000 });
                }
            }

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 12_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultTitle)).ToContainTextAsync("VÍTĚZSTVÍ");
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultXp)).ToContainTextAsync("+100 XP");
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultLeagueXp)).ToHaveCountAsync(0);
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultNoLeagueInfo))
                .ToContainTextAsync("Soukromé místnosti nepřidávají ligové XP");
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultSeriesScore)).ToHaveCountAsync(0);

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-result-no-league-xp",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");

            await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/leagues");
            await Expect(hostPage.GetByTestId(Selectors.Leagues.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Leagues.CurrentUserRow)).ToContainTextAsync(host.Username);
            await Expect(hostPage.GetByTestId(Selectors.Leagues.CurrentUserRow)).ToContainTextAsync("0 XP");

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-league-still-zero",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_RematchRequest_Accept_ReturnsBothPlayersToLobby()
    {
        const string scenario = "private-room-rematch-accept";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prrematchhost");
        var guest = await Fixture.RegisterUniqueUserAsync("prrematchguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            var roomCode = await CompleteBestOf1PrivateRoomAsync(hostPage, guestPage);
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 12_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 12_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.ResultNext).ClickAsync();
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultRematchPending))
                .ToContainTextAsync("Čeká se na soupeře");
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.ResultRematchRequest))
                .ToContainTextAsync("Soupeř chce odvetu");
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.ResultRematchAccept)).ToBeVisibleAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.ResultRematchDecline)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                guestPage,
                area: "multiplayer",
                scenario,
                state: "guest-rematch-request",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerGuest");

            await guestPage.GetByTestId(Selectors.Multiplayer.ResultRematchAccept).ClickAsync();

            await Expect(hostPage).ToHaveURLAsync(new Regex($".*/multiplayer/room/{Regex.Escape(roomCode)}"), new() { Timeout = 10_000 });
            await Expect(guestPage).ToHaveURLAsync(new Regex($".*/multiplayer/room/{Regex.Escape(roomCode)}"), new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateReady)).ToBeVisibleAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateReady)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-lobby-after-accept",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    [Fact]
    public async Task PrivateRoom_RematchRequest_Decline_NotifiesRequester()
    {
        const string scenario = "private-room-rematch-decline";
        await Fixture.ResetDatabaseAsync();

        var host = await Fixture.RegisterUniqueUserAsync("prdeclinehost");
        var guest = await Fixture.RegisterUniqueUserAsync("prdeclineguest");

        var hostPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.host");
        var guestPage = await Fixture.NewPageAsync(testName: $"multiplayer.{scenario}.guest");

        try
        {
            await Task.WhenAll(
                Fixture.LoginAsAsync(hostPage, host),
                Fixture.LoginAsAsync(guestPage, guest));

            await CompleteBestOf1PrivateRoomAsync(hostPage, guestPage);
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 12_000 });
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync(new() { Timeout = 12_000 });

            await hostPage.GetByTestId(Selectors.Multiplayer.ResultNext).ClickAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.ResultRematchRequest))
                .ToContainTextAsync("Soupeř chce odvetu");

            await guestPage.GetByTestId(Selectors.Multiplayer.ResultRematchDecline).ClickAsync();

            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultRematchDeclined))
                .ToContainTextAsync("odvetu odmítl");
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.ResultRematchRequest)).ToHaveCountAsync(0);
            await Expect(hostPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync();
            await Expect(guestPage.GetByTestId(Selectors.Multiplayer.ResultPage)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                hostPage,
                area: "multiplayer",
                scenario,
                state: "host-rematch-declined",
                viewport: "1366x900",
                theme: "light",
                persona: "multiplayerHost");
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(hostPage, "multiplayer", $"{scenario}-host");
            await Fixture.TakeFailureArtifactsAsync(guestPage, "multiplayer", $"{scenario}-guest");
            await Fixture.WriteEnvironmentLogsAsync($"multiplayer-{scenario}");
            throw;
        }
        finally
        {
            await hostPage.Context.CloseAsync();
            await guestPage.Context.CloseAsync();
        }
    }

    private async Task<HubConnection> CreateStartedHubConnectionAsync(TestUser user)
    {
        var auth = await Fixture.AuthenticateAsync(user);
        var connection = new HubConnectionBuilder()
            .WithUrl($"{Fixture.ApiBaseUrl}/hubs/match", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(auth.AccessToken);
            })
            .Build();

        await connection.StartAsync();
        return connection;
    }

    private async Task<string> CompleteBestOf1PrivateRoomAsync(IPage hostPage, IPage guestPage)
    {
        await Fixture.GoToAndWaitForAppReadyAsync(hostPage, "/multiplayer");
        await hostPage.GetByTestId(Selectors.Multiplayer.CreateRoom).ClickAsync();
        await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
        await hostPage.GetByTestId(Selectors.Multiplayer.PrivateWordCount10).ClickAsync();
        await hostPage.GetByTestId(Selectors.Multiplayer.PrivateBestOf1).ClickAsync();
        await hostPage.GetByTestId(Selectors.Multiplayer.PrivateCreateSubmit).ClickAsync();
        await Expect(hostPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });
        var roomCode = (await hostPage.GetByTestId(Selectors.Multiplayer.PrivateRoomCode).InnerTextAsync()).Trim();

        await Fixture.GoToAndWaitForAppReadyAsync(guestPage, "/multiplayer");
        await guestPage.GetByTestId(Selectors.Multiplayer.JoinRoom).ClickAsync();
        await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
        await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinInput).FillAsync(roomCode);
        await guestPage.GetByTestId(Selectors.Multiplayer.PrivateJoinSubmit).ClickAsync();
        await Expect(guestPage.GetByTestId(Selectors.Multiplayer.PrivateLobby)).ToBeVisibleAsync(new() { Timeout = 10_000 });

        await hostPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();
        await guestPage.GetByTestId(Selectors.Multiplayer.PrivateReady).ClickAsync();

        await Expect(hostPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });
        await Expect(guestPage.GetByTestId(Selectors.Multiplayer.RealtimeGame)).ToBeVisibleAsync(new() { Timeout = 20_000 });

        for (var solved = 0; solved < 10; solved++)
        {
            var answer = await GetCurrentRealtimeAnswerAsync(hostPage);
            await hostPage.GetByTestId(Selectors.Multiplayer.RealtimeAnswerInput).FillAsync(answer);
            await hostPage.GetByTestId(Selectors.Multiplayer.RealtimeSubmit).ClickAsync();

            if (solved < 9)
            {
                await Expect(hostPage.GetByTestId(Selectors.Multiplayer.RealtimePlayerScore))
                    .ToContainTextAsync($"{solved + 1}/", new() { Timeout = 8_000 });
            }
        }

        return roomCode;
    }

    private async Task<string> GetCurrentRealtimeAnswerAsync(IPage page)
    {
        await Expect(page.GetByTestId(Selectors.Multiplayer.RealtimeScrambledWord)).ToBeVisibleAsync(new() { Timeout = 8_000 });
        var scrambledText = await page.GetByTestId(Selectors.Multiplayer.RealtimeScrambledWord).InnerTextAsync();
        var scrambled = Regex.Replace(scrambledText, "[^\\p{L}]", "");
        return await Fixture.GetOriginalForScrambledWordAsync(scrambled);
    }

    private Task<string> GetAnswerForRoundAsync(MultiplayerRoundDto round)
    {
        var scrambled = Regex.Replace(round.ScrambledWord, "[^\\p{L}]", "");
        return Fixture.GetOriginalForScrambledWordAsync(scrambled);
    }

    private static async Task<MultiplayerRoundDto> AwaitRoundAsync(
        ChannelReader<MultiplayerRoundDto> reader,
        int roundNumber,
        int timeoutSeconds = 10)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        while (await reader.WaitToReadAsync(cts.Token))
        {
            while (reader.TryRead(out var round))
            {
                if (round.RoundNumber == roundNumber)
                {
                    return round;
                }
            }
        }

        throw new TimeoutException($"Round {roundNumber} was not received within {timeoutSeconds} seconds.");
    }

    private static async Task<T> AwaitAsync<T>(Task<T> task, TimeSpan timeout, string because)
    {
        var completed = await Task.WhenAny(task, Task.Delay(timeout));
        completed.Should().Be(task, because);
        return await task;
    }

    private static async Task AssertNoMatchAsync(Task<MatchFoundEvent> task, TimeSpan timeout, string because)
    {
        var completed = await Task.WhenAny(task, Task.Delay(timeout));
        completed.Should().NotBe(task, because);
    }
}
