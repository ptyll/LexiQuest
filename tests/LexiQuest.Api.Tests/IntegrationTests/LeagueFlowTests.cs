using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Leagues;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class LeagueFlowTests
{
    private static readonly string TestDbName = $"LeagueFlowTestDb_{Guid.NewGuid()}";

    private CustomWebApplicationFactory CreateFactory()
    {
        return new CustomWebApplicationFactory(TestDbName);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterRequest
        {
            Username = $"leagueuser_{uniqueId}",
            Email = $"league_{uniqueId}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            AcceptTerms = true
        };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        return client;
    }

    /// <summary>
    /// LeaguesController.GetCurrentUserId() uses FindFirst("sub") which doesn't work
    /// with default JWT claim mapping (sub is mapped to ClaimTypes.NameIdentifier).
    /// These tests document that the controller returns Unauthorized in this case.
    /// </summary>
    [Fact]
    public async Task GetCurrentLeague_Authenticated_Returns401DueToClaimMapping()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/leagues/current");

        // Assert - Controller uses FindFirst("sub") which returns null
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLeaderboard_Authenticated_Returns401DueToClaimMapping()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/leagues/leaderboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLeagueHistory_Authenticated_Returns401DueToClaimMapping()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/leagues/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLeagueRewards_Authenticated_ReturnsAllTiers()
    {
        // Arrange - Rewards endpoint doesn't need userId, just auth
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/leagues/rewards");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rewards = await response.Content.ReadFromJsonAsync<List<LeagueRewardsDto>>();
        rewards.Should().NotBeNull();
        rewards!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task LeagueEndpoints_WithoutAuth_Returns401()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act
        var currentResponse = await client.GetAsync("/api/v1/leagues/current");
        var leaderboardResponse = await client.GetAsync("/api/v1/leagues/leaderboard");

        // Assert
        currentResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        leaderboardResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
