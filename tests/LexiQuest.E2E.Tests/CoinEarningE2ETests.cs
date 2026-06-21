using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.DTOs.Shop;
using LexiQuest.Shared.Enums;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class CoinEarningE2ETests : E2ETestBase
{
    public CoinEarningE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Coins_PathLevelComplete_EarnsTenCoinsOnce()
    {
        await RunScenarioAsync("shop", "coin-earning-path-level", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("coinlevel");
            await Fixture.SeedUserAchievementAsync(user.Email, "first_word", progress: 1, isUnlocked: true);

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var path = await GetBeginnerPathAsync(apiClient);

            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.Path, DifficultyLevel.Beginner, path.Id, LevelNumber: 1));
            await Fixture.ForceSessionTotalRoundsAsync(game.SessionId, totalRounds: 1);

            var answer = await Fixture.GetActiveRoundAnswerAsync(game.SessionId);
            var result = await Fixture.SubmitAnswerViaApiAsync(apiClient, game.SessionId, answer, timeSpentMs: 1_000);

            result.IsLevelComplete.Should().BeTrue();
            (await GetCoinBalanceAsync(apiClient)).Should().Be(10);

            var replay = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.Path, DifficultyLevel.Beginner, path.Id, LevelNumber: 1));
            await Fixture.ForceSessionTotalRoundsAsync(replay.SessionId, totalRounds: 1);

            var replayAnswer = await Fixture.GetActiveRoundAnswerAsync(replay.SessionId);
            var replayResult = await Fixture.SubmitAnswerViaApiAsync(apiClient, replay.SessionId, replayAnswer, timeSpentMs: 1_000);

            replayResult.IsLevelComplete.Should().BeTrue();
            (await GetCoinBalanceAsync(apiClient)).Should().Be(10);
        }, assertNoConsoleErrors: false);
    }

    [Fact]
    public async Task Coins_BossVictory_EarnsFiftyCoins()
    {
        await RunScenarioAsync("shop", "coin-earning-boss", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("coinboss");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            var session = await StartBossAsync(apiClient, BossType.Marathon);
            await Fixture.ForceSessionTotalRoundsAsync(session.Id, totalRounds: 1);

            var answer = await Fixture.GetActiveRoundAnswerAsync(session.Id);
            var result = await SubmitBossAnswerViaApiAsync(apiClient, session.Id, answer);

            result.IsCompleted.Should().BeTrue();
            (await GetCoinBalanceAsync(apiClient)).Should().Be(50);
        }, assertNoConsoleErrors: false);
    }

    [Fact]
    public async Task Coins_DailyChallengeComplete_EarnsTwentyCoins()
    {
        await RunScenarioAsync("shop", "coin-earning-daily", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("coindaily");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            var challenge = await apiClient.GetFromJsonAsync<DailyChallengeDto>("api/v1/game/daily");
            challenge.Should().NotBeNull();

            var originals = await Fixture.GetWordOriginalsAsync([challenge!.WordId]);
            using var response = await apiClient.PostAsJsonAsync(
                "api/v1/game/daily/submit",
                new DailyChallengeSubmitRequest(originals[challenge.WordId], 1_000));

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ChallengeResultDto>();
            result.Should().NotBeNull();
            result!.IsCorrect.Should().BeTrue();
            (await GetCoinBalanceAsync(apiClient)).Should().Be(20);

            using var secondAttempt = await apiClient.PostAsJsonAsync(
                "api/v1/game/daily/submit",
                new DailyChallengeSubmitRequest(originals[challenge.WordId], 1_000));
            secondAttempt.StatusCode.Should().Be(HttpStatusCode.Conflict);
            (await GetCoinBalanceAsync(apiClient)).Should().Be(20);
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Coins_FirstWordAchievement_EarnsFiftyCoinsOnce()
    {
        await RunScenarioAsync("shop", "coin-earning-achievement", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("coinachievement");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.Training, DifficultyLevel.Beginner));
            var answer = await Fixture.GetActiveRoundAnswerAsync(game.SessionId);
            await Fixture.SubmitAnswerViaApiAsync(apiClient, game.SessionId, answer, timeSpentMs: 1_000);

            (await GetCoinBalanceAsync(apiClient)).Should().Be(50);
            (await Fixture.GetUserAchievementCountAsync(user.Email, "first_word")).Should().Be(1);

            var secondGame = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.Training, DifficultyLevel.Beginner));
            var secondAnswer = await Fixture.GetActiveRoundAnswerAsync(secondGame.SessionId);
            await Fixture.SubmitAnswerViaApiAsync(apiClient, secondGame.SessionId, secondAnswer, timeSpentMs: 1_000);

            (await GetCoinBalanceAsync(apiClient)).Should().Be(50);
            (await Fixture.GetUserAchievementCountAsync(user.Email, "first_word")).Should().Be(1);
        }, assertNoConsoleErrors: false);
    }

    private static async Task<LearningPathDto> GetBeginnerPathAsync(HttpClient apiClient)
    {
        var paths = await apiClient.GetFromJsonAsync<List<LearningPathDto>>("api/v1/paths");
        paths.Should().NotBeNull();
        return paths!.Single(path => path.Difficulty == DifficultyLevel.Beginner);
    }

    private static async Task<int> GetCoinBalanceAsync(HttpClient apiClient)
    {
        var coins = await apiClient.GetFromJsonAsync<CoinBalanceDto>("api/v1/shop/coins");
        coins.Should().NotBeNull();
        return coins!.Balance;
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
        return session!;
    }

    private static async Task<BossRoundResultDto> SubmitBossAnswerViaApiAsync(
        HttpClient apiClient,
        Guid sessionId,
        string answer)
    {
        using var response = await apiClient.PostAsJsonAsync(
            $"api/v1/boss/{sessionId}/answer",
            new BossAnswerRequest
            {
                Answer = answer,
                TimeSpentMs = 1_000
            });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BossRoundResultDto>();
        result.Should().NotBeNull();
        return result!;
    }
}
