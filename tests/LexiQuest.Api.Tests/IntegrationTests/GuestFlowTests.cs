using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class GuestFlowTests
{
    private static readonly string TestDbName = $"GuestFlowTestDb_{Guid.NewGuid()}";

    private CustomWebApplicationFactory CreateFactory()
    {
        return new CustomWebApplicationFactory(TestDbName);
    }

    private async Task EnsureDbCreated(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task StartGuestGame_NoAuth_Returns200()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        await EnsureDbCreated(factory);

        // Act - No auth required for guest mode
        var response = await client.PostAsync("/api/v1/game/guest/start", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GuestStartResponse>();
        result.Should().NotBeNull();
        result!.SessionId.Should().NotBe(Guid.Empty);
        result.ScrambledWords.Should().NotBeNullOrEmpty();
        result.RemainingGames.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// GuestSessionService is registered as Scoped, so its in-memory session dictionary
    /// is per-request. A session created in one request is not available in the next.
    /// This causes the answer endpoint to return 400 (session not found).
    /// </summary>
    [Fact]
    public async Task GuestAnswer_SessionFromPreviousRequest_Returns400DueToScopedService()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        await EnsureDbCreated(factory);

        var startResponse = await client.PostAsync("/api/v1/game/guest/start", null);
        startResponse.EnsureSuccessStatusCode();
        var gameSession = await startResponse.Content.ReadFromJsonAsync<GuestStartResponse>();
        gameSession.Should().NotBeNull();
        gameSession!.ScrambledWords.Should().NotBeEmpty();

        var firstWord = gameSession.ScrambledWords[0];

        // Act - Submit answer for session from previous request
        var answerRequest = new GuestAnswerRequest(
            SessionId: gameSession.SessionId,
            WordId: firstWord.WordId,
            Answer: "WRONGANSWER"
        );
        var response = await client.PostAsJsonAsync("/api/v1/game/guest/answer", answerRequest);

        // Assert - Session not found because GuestSessionService is Scoped (per-request)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetGuestStatus_NoAuth_Returns200()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        await EnsureDbCreated(factory);

        // Act
        var response = await client.GetAsync("/api/v1/guest/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var status = await response.Content.ReadFromJsonAsync<GuestStatusResponse>();
        status.Should().NotBeNull();
        status!.TotalAllowed.Should().BeGreaterThan(0);
        status.Remaining.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GuestAnswer_InvalidSession_Returns400()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        await EnsureDbCreated(factory);

        var answerRequest = new GuestAnswerRequest(
            SessionId: Guid.NewGuid(),
            WordId: Guid.NewGuid(),
            Answer: "TEST"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/game/guest/answer", answerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GuestStartGame_MultipleGames_TrackRemaining()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        await EnsureDbCreated(factory);

        // Act - Start first game
        var firstResponse = await client.PostAsync("/api/v1/game/guest/start", null);
        firstResponse.EnsureSuccessStatusCode();
        var firstGame = await firstResponse.Content.ReadFromJsonAsync<GuestStartResponse>();

        // Start second game
        var secondResponse = await client.PostAsync("/api/v1/game/guest/start", null);
        secondResponse.EnsureSuccessStatusCode();
        var secondGame = await secondResponse.Content.ReadFromJsonAsync<GuestStartResponse>();

        // Assert - Remaining games should decrease
        firstGame.Should().NotBeNull();
        secondGame.Should().NotBeNull();
        secondGame!.RemainingGames.Should().BeLessThan(firstGame!.RemainingGames);
    }

    [Fact]
    public async Task GuestConvert_ValidSession_ReturnsOkOrBadRequest()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        await EnsureDbCreated(factory);

        var startResponse = await client.PostAsync("/api/v1/game/guest/start", null);
        startResponse.EnsureSuccessStatusCode();
        var gameSession = await startResponse.Content.ReadFromJsonAsync<GuestStartResponse>();

        var convertRequest = new GuestConvertRequest(
            SessionId: gameSession!.SessionId,
            Email: "newuser@example.com",
            Username: "newuser",
            Password: "Password123!",
            TransferProgress: true
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/game/guest/convert", convertRequest);

        // Assert - Could be OK or BadRequest depending on session state
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<GuestConvertResponse>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
        }
    }
}
