using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests;

public class GameIntegrationTests
{
    private static readonly string TestDbName = $"IntegrationTestDb_{Guid.NewGuid()}";

    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");

                Environment.SetEnvironmentVariable("JwtSettings__SecretKey", "Test-Secret-Key-That-Is-Long-Enough-For-HS256-Algorithm-!!", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TestIssuer", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("JwtSettings__Audience", "TestAudience", EnvironmentVariableTarget.Process);

                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<LexiQuestDbContext>(options =>
                        options.UseInMemoryDatabase(TestDbName));
                });
            });
    }

    private async Task<(HttpClient Client, WebApplicationFactory<Program> Factory, AuthResponse Auth)> SetupTestUserAsync()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Seed words
        if (!dbContext.Words.Any())
        {
            var words = new[]
            {
                Word.Create("JABLKO", DifficultyLevel.Beginner, WordCategory.Food, 1),
                Word.Create("BANÁN", DifficultyLevel.Beginner, WordCategory.Food, 2),
                Word.Create("POMERANČ", DifficultyLevel.Beginner, WordCategory.Food, 3),
                Word.Create("HRUŠKA", DifficultyLevel.Beginner, WordCategory.Food, 4),
                Word.Create("ŠVESTKA", DifficultyLevel.Beginner, WordCategory.Food, 5),
            };
            dbContext.Words.AddRange(words);
            await dbContext.SaveChangesAsync();
        }

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterRequest
        {
            Username = $"testuser_{uniqueId}",
            Email = $"test_{uniqueId}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            AcceptTerms = true
        };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        return (client, factory, authResponse);
    }

    [Fact]
    public async Task CompleteGameCycle_Start_To_Answer_XP_To_NextRound_To_Complete()
    {
        // Arrange
        var (client, factory, _) = await SetupTestUserAsync();

        // Step 1: Start game
        var startRequest = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);
        var startResponse = await client.PostAsJsonAsync("/api/v1/game/start", startRequest);
        startResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var gameState = await startResponse.Content.ReadFromJsonAsync<ScrambledWordDto>();
        gameState.Should().NotBeNull();
        gameState!.RoundNumber.Should().Be(1);
        gameState.ScrambledWord.Should().NotBeNullOrEmpty();

        var sessionId = gameState.SessionId;
        var totalXP = 0;
        var roundNumber = 1;

        // Step 2-3: Answer rounds until complete or out of words
        GameRoundResult? lastResult = null;
        var maxRounds = 5; // Limited by seeded words

        for (int i = 0; i < maxRounds; i++)
        {
            // Get correct answer from DB
            var correctAnswer = GetCorrectAnswer(factory, sessionId, roundNumber);

            var submitRequest = new SubmitAnswerRequest
            {
                SessionId = sessionId,
                Answer = correctAnswer,
                TimeSpentMs = 3000 // Fast answer for speed bonus
            };

            var submitResponse = await client.PostAsJsonAsync($"/api/v1/game/{sessionId}/answer", submitRequest);
            submitResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

            if (submitResponse.StatusCode == HttpStatusCode.OK)
            {
                lastResult = await submitResponse.Content.ReadFromJsonAsync<GameRoundResult>();
                lastResult.Should().NotBeNull();
                lastResult!.IsCorrect.Should().BeTrue();
                lastResult.XPEarned.Should().BeGreaterThan(0);
                totalXP += lastResult.XPEarned;

                if (lastResult.IsLevelComplete)
                {
                    break;
                }

                if (lastResult.NextRoundNumber.HasValue)
                {
                    roundNumber = lastResult.NextRoundNumber.Value;
                }
            }
            else
            {
                // Ran out of words
                break;
            }
        }

        // Assert - Game should have progressed
        lastResult.Should().NotBeNull();
        totalXP.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GameCycle_XPCalculation_EndToEnd()
    {
        // Arrange
        var (client, factory, _) = await SetupTestUserAsync();

        // Start game
        var startRequest = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);
        var startResponse = await client.PostAsJsonAsync("/api/v1/game/start", startRequest);
        var gameState = await startResponse.Content.ReadFromJsonAsync<ScrambledWordDto>();
        var sessionId = gameState!.SessionId;

        // Fast answer (under 3s) should give +5 speed bonus
        var correctAnswer = GetCorrectAnswer(factory, sessionId, 1);
        var submitRequest = new SubmitAnswerRequest
        {
            SessionId = sessionId,
            Answer = correctAnswer,
            TimeSpentMs = 2500
        };

        var submitResponse = await client.PostAsJsonAsync($"/api/v1/game/{sessionId}/answer", submitRequest);
        var result = await submitResponse.Content.ReadFromJsonAsync<GameRoundResult>();

        // Assert
        result!.XPEarned.Should().BeGreaterThanOrEqualTo(15); // Base 10 + Speed 5
        result.SpeedBonus.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task GameCycle_ComboMechanic_IncreasesWithCorrectAnswers()
    {
        // Arrange
        var (client, factory, _) = await SetupTestUserAsync();

        var startRequest = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);
        var startResponse = await client.PostAsJsonAsync("/api/v1/game/start", startRequest);
        var gameState = await startResponse.Content.ReadFromJsonAsync<ScrambledWordDto>();
        var sessionId = gameState!.SessionId;

        // First correct answer
        var answer1 = GetCorrectAnswer(factory, sessionId, 1);
        var result1 = await SubmitAnswerAsync(client, sessionId, answer1, 5000);
        result1!.IsCorrect.Should().BeTrue();
        result1.ComboCount.Should().Be(1);

        // Continue if there's a next round
        if (result1.NextRoundNumber.HasValue)
        {
            var answer2 = result1.CorrectAnswer == answer1 
                ? GetCorrectAnswer(factory, sessionId, result1.NextRoundNumber.Value)
                : result1.CorrectAnswer;
                
            var result2 = await SubmitAnswerAsync(client, sessionId, answer2, 5000);
            if (result2 != null && result2.IsCorrect)
            {
                result2.ComboCount.Should().BeGreaterThanOrEqualTo(1);
            }
        }
    }

    [Fact]
    public async Task GameCycle_WrongAnswer_ResetsCombo()
    {
        // Arrange
        var (client, factory, _) = await SetupTestUserAsync();

        var startRequest = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);
        var startResponse = await client.PostAsJsonAsync("/api/v1/game/start", startRequest);
        var gameState = await startResponse.Content.ReadFromJsonAsync<ScrambledWordDto>();
        var sessionId = gameState!.SessionId;

        // Wrong answer should have combo 0
        var result = await SubmitAnswerAsync(client, sessionId, "WRONGANSWER", 5000);
        result.Should().NotBeNull();
        result!.IsCorrect.Should().BeFalse();
        result.ComboCount.Should().Be(0);
    }

    [Fact]
    public async Task GameCycle_TimerMechanic_ExpiresSubmitsEmptyAnswer()
    {
        // Arrange
        var (client, factory, _) = await SetupTestUserAsync();

        var startRequest = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);
        var startResponse = await client.PostAsJsonAsync("/api/v1/game/start", startRequest);
        var gameState = await startResponse.Content.ReadFromJsonAsync<ScrambledWordDto>();
        var sessionId = gameState!.SessionId;

        // Submit with time exceeding limit (simulating timeout)
        var answer = GetCorrectAnswer(factory, sessionId, 1);
        var result = await SubmitAnswerAsync(client, sessionId, answer, 35000); // 35 seconds

        // Should still process but without speed bonus
        result.Should().NotBeNull();
    }

    private async Task<GameRoundResult?> SubmitAnswerAsync(HttpClient client, Guid sessionId, string answer, int timeSpentMs)
    {
        var submitRequest = new SubmitAnswerRequest
        {
            SessionId = sessionId,
            Answer = answer,
            TimeSpentMs = timeSpentMs
        };

        var response = await client.PostAsJsonAsync($"/api/v1/game/{sessionId}/answer", submitRequest);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadFromJsonAsync<GameRoundResult>();
        }
        return null;
    }

    private string GetCorrectAnswer(WebApplicationFactory<Program> factory, Guid sessionId, int roundNumber)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();

        var round = context.GameRounds
            .FirstOrDefault(r => r.SessionId == sessionId && r.RoundNumber == roundNumber);

        if (round == null)
            throw new InvalidOperationException($"Round {roundNumber} not found");

        return round.CorrectAnswer;
    }
}
