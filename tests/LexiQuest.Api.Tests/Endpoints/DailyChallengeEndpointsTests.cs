using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Api;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.Endpoints;

public class DailyChallengeEndpointsTests : IDisposable
{
    private static readonly string TestDbName = $"TestDb_DailyChallenge_{Guid.NewGuid()}";
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
    public async Task GetDailyChallenge_Unauthorized_Returns401()
    {
        // Arrange
        _factory = CreateFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/game/daily");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDailyLeaderboard_Unauthorized_Returns401()
    {
        // Arrange
        _factory = CreateFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/game/daily/leaderboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task StartDailyChallenge_Unauthorized_Returns401()
    {
        // Arrange
        _factory = CreateFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Act
        var response = await _client.PostAsync("/api/v1/game/daily/start", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
