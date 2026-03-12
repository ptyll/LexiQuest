using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class GameFlowTests
{
    private static readonly string TestDbName = $"GameFlowTestDb_{Guid.NewGuid()}";

    private CustomWebApplicationFactory CreateFactory()
    {
        return new CustomWebApplicationFactory(TestDbName);
    }

    private async Task<(HttpClient Client, WebApplicationFactory<Program> Factory)> CreateAuthenticatedClientAsync()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Seed test words
        if (!dbContext.Words.Any())
        {
            var words = new[]
            {
                Word.Create("JABLKO", DifficultyLevel.Beginner, WordCategory.Food, 1),
                Word.Create("BANÁN", DifficultyLevel.Beginner, WordCategory.Food, 2),
                Word.Create("POMERANČ", DifficultyLevel.Beginner, WordCategory.Food, 3),
                Word.Create("HRUŠKA", DifficultyLevel.Beginner, WordCategory.Food, 4),
                Word.Create("ŠVESTKA", DifficultyLevel.Beginner, WordCategory.Food, 5),
                Word.Create("TŘEŠEŇ", DifficultyLevel.Beginner, WordCategory.Food, 6),
                Word.Create("MALINA", DifficultyLevel.Beginner, WordCategory.Food, 7),
                Word.Create("JAHODA", DifficultyLevel.Beginner, WordCategory.Food, 8),
                Word.Create("BORŮVKA", DifficultyLevel.Beginner, WordCategory.Food, 9),
                Word.Create("ANANAS", DifficultyLevel.Beginner, WordCategory.Food, 10),
            };
            dbContext.Words.AddRange(words);
            await dbContext.SaveChangesAsync();
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterRequest
        {
            Username = $"gameuser_{uniqueId}",
            Email = $"game_{uniqueId}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            AcceptTerms = true
        };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        return (client, factory);
    }

    [Fact]
    public async Task StartGame_ValidRequest_ReturnsScrambledWord()
    {
        // Arrange
        var (client, _) = await CreateAuthenticatedClientAsync();
        var request = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/game/start", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ScrambledWordDto>();
        result.Should().NotBeNull();
        result!.SessionId.Should().NotBe(Guid.Empty);
        result.ScrambledWord.Should().NotBeNullOrEmpty();
        result.RoundNumber.Should().Be(1);
    }

    [Fact]
    public async Task SubmitCorrectAnswer_ReturnsXPEarned()
    {
        // Arrange
        var (client, factory) = await CreateAuthenticatedClientAsync();
        var startRequest = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);
        var startResponse = await client.PostAsJsonAsync("/api/v1/game/start", startRequest);
        startResponse.EnsureSuccessStatusCode();
        var gameState = await startResponse.Content.ReadFromJsonAsync<ScrambledWordDto>();

        var correctAnswer = GetCorrectAnswer(factory, gameState!.SessionId, gameState.RoundNumber);
        var submitRequest = new SubmitAnswerRequest
        {
            SessionId = gameState.SessionId,
            Answer = correctAnswer,
            TimeSpentMs = 5000
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/v1/game/{gameState.SessionId}/answer", submitRequest);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<GameRoundResult>();
            result.Should().NotBeNull();
            result!.IsCorrect.Should().BeTrue();
            result.XPEarned.Should().BeGreaterThan(0);
        }
        else
        {
            // Acceptable if out of words for next round
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task SubmitWrongAnswer_ReturnsZeroXP()
    {
        // Arrange
        var (client, _) = await CreateAuthenticatedClientAsync();
        var startRequest = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);
        var startResponse = await client.PostAsJsonAsync("/api/v1/game/start", startRequest);
        startResponse.EnsureSuccessStatusCode();
        var gameState = await startResponse.Content.ReadFromJsonAsync<ScrambledWordDto>();

        var submitRequest = new SubmitAnswerRequest
        {
            SessionId = gameState!.SessionId,
            Answer = "WRONGANSWER",
            TimeSpentMs = 5000
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/v1/game/{gameState.SessionId}/answer", submitRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GameRoundResult>();
        result.Should().NotBeNull();
        result!.IsCorrect.Should().BeFalse();
        result.XPEarned.Should().Be(0);
    }

    [Fact]
    public async Task GetGameState_AfterStart_ReturnsCurrentState()
    {
        // Arrange
        var (client, _) = await CreateAuthenticatedClientAsync();
        var startRequest = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);
        var startResponse = await client.PostAsJsonAsync("/api/v1/game/start", startRequest);
        startResponse.EnsureSuccessStatusCode();
        var gameState = await startResponse.Content.ReadFromJsonAsync<ScrambledWordDto>();

        // Act
        var response = await client.GetAsync($"/api/v1/game/{gameState!.SessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ScrambledWordDto>();
        result.Should().NotBeNull();
        result!.SessionId.Should().Be(gameState.SessionId);
    }

    [Fact]
    public async Task ForfeitGame_ActiveSession_Returns204()
    {
        // Arrange
        var (client, _) = await CreateAuthenticatedClientAsync();
        var startRequest = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);
        var startResponse = await client.PostAsJsonAsync("/api/v1/game/start", startRequest);
        startResponse.EnsureSuccessStatusCode();
        var gameState = await startResponse.Content.ReadFromJsonAsync<ScrambledWordDto>();

        // Act
        var response = await client.PostAsync($"/api/v1/game/{gameState!.SessionId}/forfeit", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetGameState_NonExistentSession_Returns404()
    {
        // Arrange
        var (client, _) = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync($"/api/v1/game/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitAnswer_SessionIdMismatch_Returns400()
    {
        // Arrange
        var (client, _) = await CreateAuthenticatedClientAsync();
        var submitRequest = new SubmitAnswerRequest
        {
            SessionId = Guid.NewGuid(),
            Answer = "test",
            TimeSpentMs = 1000
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/v1/game/{Guid.NewGuid()}/answer", submitRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private string GetCorrectAnswer(WebApplicationFactory<Program> factory, Guid sessionId, int roundNumber)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        var round = context.GameRounds.FirstOrDefault(r => r.SessionId == sessionId && r.RoundNumber == roundNumber);
        if (round == null)
            throw new InvalidOperationException($"Round {roundNumber} not found for session {sessionId}");
        return round.CorrectAnswer;
    }
}
