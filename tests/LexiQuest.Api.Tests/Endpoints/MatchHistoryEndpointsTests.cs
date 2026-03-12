using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using LexiQuest.Api;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Infrastructure.Auth;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.Endpoints;

public class MatchHistoryEndpointsTests : IDisposable
{
    private static readonly string TestDbName = $"TestDb_MatchHistory_{Guid.NewGuid()}";
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

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

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Fact]
    public async Task GetHistory_Unauthorized_Returns401()
    {
        // Arrange
        _factory = CreateFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/multiplayer/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStats_Unauthorized_Returns401()
    {
        // Arrange
        _factory = CreateFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/multiplayer/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHistory_Authorized_ReturnsEmptyList()
    {
        // Arrange
        _factory = CreateFactory();
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var userId = Guid.NewGuid();
        var token = GenerateTestToken(userId, "testuser");
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/multiplayer/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MatchHistoryResponseDto>();
        result.Should().NotBeNull();
        result!.Entries.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetStats_Authorized_ReturnsZeroStats()
    {
        // Arrange
        _factory = CreateFactory();
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var userId = Guid.NewGuid();
        var token = GenerateTestToken(userId, "testuser");
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/multiplayer/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MultiplayerStatsDto>();
        result.Should().NotBeNull();
        result!.TotalMatchesPlayed.Should().Be(0);
        result.Wins.Should().Be(0);
        result.Losses.Should().Be(0);
        result.Draws.Should().Be(0);
        result.WinRatePercentage.Should().Be(0);
    }

    [Fact]
    public async Task GetHistory_WithMatches_ReturnsHistory()
    {
        // Arrange
        _factory = CreateFactory();
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var userId = Guid.NewGuid();
        var opponentId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow.AddMinutes(-10);

        // Add a match result
        var matchResult = MatchResult.Create(
            matchId: Guid.NewGuid(),
            player1Id: userId,
            player2Id: opponentId,
            player1Username: "testuser",
            player2Username: "opponent",
            player1Score: 10,
            player2Score: 5,
            player1Time: TimeSpan.FromMinutes(2),
            player2Time: TimeSpan.FromMinutes(2.5),
            player1MaxCombo: 5,
            player2MaxCombo: 3,
            winnerId: userId,
            isDraw: false,
            player1XpEarned: 100,
            player2XpEarned: 30,
            player1LeagueXpEarned: 50,
            player2LeagueXpEarned: 15,
            isPrivateRoom: false,
            roomCode: null,
            seriesPlayer1Wins: null,
            seriesPlayer2Wins: null,
            wordCount: 15,
            timeLimitMinutes: 3,
            difficulty: DifficultyLevel.Beginner,
            startedAt: startedAt
        );

        dbContext.MatchResults.Add(matchResult);
        await dbContext.SaveChangesAsync();

        var token = GenerateTestToken(userId, "testuser");
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/multiplayer/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MatchHistoryResponseDto>();
        result.Should().NotBeNull();
        result!.Entries.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        
        var entry = result.Entries[0];
        entry.OpponentUsername.Should().Be("opponent");
        entry.YourScore.Should().Be(10);
        entry.OpponentScore.Should().Be(5);
        entry.Result.Should().Be(MatchResultType.Win);
        entry.XPEarned.Should().Be(100);
        entry.Type.Should().Be(Shared.DTOs.Multiplayer.MatchType.QuickMatch);
    }

    [Fact]
    public async Task GetStats_WithMatches_ReturnsCorrectStats()
    {
        // Arrange
        _factory = CreateFactory();
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var userId = Guid.NewGuid();
        var opponentId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow.AddMinutes(-10);

        // Add match results: 2 wins, 1 loss
        for (int i = 0; i < 2; i++)
        {
            var matchResult = MatchResult.Create(
                matchId: Guid.NewGuid(),
                player1Id: userId,
                player2Id: opponentId,
                player1Username: "testuser",
                player2Username: "opponent",
                player1Score: 10,
                player2Score: 5,
                player1Time: TimeSpan.FromMinutes(2),
                player2Time: TimeSpan.FromMinutes(2.5),
                player1MaxCombo: 5,
                player2MaxCombo: 3,
                winnerId: userId,
                isDraw: false,
                player1XpEarned: 100,
                player2XpEarned: 30,
                player1LeagueXpEarned: 50,
                player2LeagueXpEarned: 15,
                isPrivateRoom: false,
                roomCode: null,
                seriesPlayer1Wins: null,
                seriesPlayer2Wins: null,
                wordCount: 15,
                timeLimitMinutes: 3,
                difficulty: DifficultyLevel.Beginner,
                startedAt: startedAt
            );
            dbContext.MatchResults.Add(matchResult);
        }

        // Add one loss
        var lossResult = MatchResult.Create(
            matchId: Guid.NewGuid(),
            player1Id: opponentId,
            player2Id: userId,
            player1Username: "opponent",
            player2Username: "testuser",
            player1Score: 12,
            player2Score: 8,
            player1Time: TimeSpan.FromMinutes(2),
            player2Time: TimeSpan.FromMinutes(2.5),
            player1MaxCombo: 6,
            player2MaxCombo: 4,
            winnerId: opponentId,
            isDraw: false,
            player1XpEarned: 100,
            player2XpEarned: 30,
            player1LeagueXpEarned: 50,
            player2LeagueXpEarned: 15,
            isPrivateRoom: false,
            roomCode: null,
            seriesPlayer1Wins: null,
            seriesPlayer2Wins: null,
            wordCount: 15,
            timeLimitMinutes: 3,
            difficulty: DifficultyLevel.Beginner,
            startedAt: startedAt
        );
        dbContext.MatchResults.Add(lossResult);

        await dbContext.SaveChangesAsync();

        var token = GenerateTestToken(userId, "testuser");
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/multiplayer/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MultiplayerStatsDto>();
        result.Should().NotBeNull();
        result!.TotalMatchesPlayed.Should().Be(3);
        result.Wins.Should().Be(2);
        result.Losses.Should().Be(1);
        result.Draws.Should().Be(0);
        result.WinRatePercentage.Should().BeApproximately(66.7, 0.1);
        result.TotalXPEarned.Should().Be(230); // 100 + 100 + 30
    }

    [Fact]
    public async Task GetHistory_WithFilter_ReturnsFilteredResults()
    {
        // Arrange
        _factory = CreateFactory();
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var userId = Guid.NewGuid();
        var opponentId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow.AddMinutes(-10);

        // Add quick match
        var quickMatch = MatchResult.Create(
            matchId: Guid.NewGuid(),
            player1Id: userId,
            player2Id: opponentId,
            player1Username: "testuser",
            player2Username: "opponent",
            player1Score: 10,
            player2Score: 5,
            player1Time: TimeSpan.FromMinutes(2),
            player2Time: TimeSpan.FromMinutes(2.5),
            player1MaxCombo: 5,
            player2MaxCombo: 3,
            winnerId: userId,
            isDraw: false,
            player1XpEarned: 100,
            player2XpEarned: 30,
            player1LeagueXpEarned: 50,
            player2LeagueXpEarned: 15,
            isPrivateRoom: false,
            roomCode: null,
            seriesPlayer1Wins: null,
            seriesPlayer2Wins: null,
            wordCount: 15,
            timeLimitMinutes: 3,
            difficulty: DifficultyLevel.Beginner,
            startedAt: startedAt
        );
        dbContext.MatchResults.Add(quickMatch);

        // Add private room match
        var privateMatch = MatchResult.Create(
            matchId: Guid.NewGuid(),
            player1Id: userId,
            player2Id: opponentId,
            player1Username: "testuser",
            player2Username: "opponent",
            player1Score: 8,
            player2Score: 12,
            player1Time: TimeSpan.FromMinutes(2),
            player2Time: TimeSpan.FromMinutes(2.5),
            player1MaxCombo: 4,
            player2MaxCombo: 6,
            winnerId: opponentId,
            isDraw: false,
            player1XpEarned: 30,
            player2XpEarned: 100,
            player1LeagueXpEarned: 0,
            player2LeagueXpEarned: 0,
            isPrivateRoom: true,
            roomCode: "LEXIQ-ABCD",
            seriesPlayer1Wins: 0,
            seriesPlayer2Wins: 1,
            wordCount: 10,
            timeLimitMinutes: 2,
            difficulty: DifficultyLevel.Intermediate,
            startedAt: startedAt
        );
        dbContext.MatchResults.Add(privateMatch);

        await dbContext.SaveChangesAsync();

        var token = GenerateTestToken(userId, "testuser");
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Filter for QuickMatch only
        var response = await _client.GetAsync("/api/v1/multiplayer/history?filter=QuickMatch");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MatchHistoryResponseDto>();
        result.Should().NotBeNull();
        result!.Entries.Should().HaveCount(1);
        result.Entries[0].Type.Should().Be(Shared.DTOs.Multiplayer.MatchType.QuickMatch);
    }

    [Fact]
    public async Task GetHistory_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        _factory = CreateFactory();
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var userId = Guid.NewGuid();
        var opponentId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow.AddMinutes(-10);

        // Add 5 matches
        for (int i = 0; i < 5; i++)
        {
            var matchResult = MatchResult.Create(
                matchId: Guid.NewGuid(),
                player1Id: userId,
                player2Id: opponentId,
                player1Username: "testuser",
                player2Username: "opponent",
                player1Score: 10 + i,
                player2Score: 5,
                player1Time: TimeSpan.FromMinutes(2),
                player2Time: TimeSpan.FromMinutes(2.5),
                player1MaxCombo: 5,
                player2MaxCombo: 3,
                winnerId: userId,
                isDraw: false,
                player1XpEarned: 100,
                player2XpEarned: 30,
                player1LeagueXpEarned: 50,
                player2LeagueXpEarned: 15,
                isPrivateRoom: false,
                roomCode: null,
                seriesPlayer1Wins: null,
                seriesPlayer2Wins: null,
                wordCount: 15,
                timeLimitMinutes: 3,
                difficulty: DifficultyLevel.Beginner,
                startedAt: startedAt.AddMinutes(-i) // Different times for ordering
            );
            dbContext.MatchResults.Add(matchResult);
        }

        await dbContext.SaveChangesAsync();

        var token = GenerateTestToken(userId, "testuser");
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Get page 1 with 2 items per page
        var response = await _client.GetAsync("/api/v1/multiplayer/history?pageNumber=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MatchHistoryResponseDto>();
        result.Should().NotBeNull();
        result!.Entries.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    private static string GenerateTestToken(Guid userId, string username)
    {
        // Manual JWT token generation for testing
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes("Test-Secret-Key-That-Is-Long-Enough-For-HS256-Algorithm-!!"));
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, 
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(System.Security.Claims.ClaimTypes.Name, username),
            new Claim(System.Security.Claims.ClaimTypes.Email, $"{username}@test.com"),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName, username),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
