using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.Controllers;

public class LoginControllerTests
{
    private static readonly string TestDbName = $"TestDb_{Guid.NewGuid()}";
    
    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                
                // Set environment variables for JWT
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

    private HttpClient CreateClient() => CreateFactory().CreateClient();

    private async Task<(HttpClient Client, WebApplicationFactory<Program> Factory)> CreateClientWithUserAsync(string email, string password, string username)
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();
        
        // Create user directly in database
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<LexiQuest.Core.Domain.Entities.User>>();
        
        var user = LexiQuest.Core.Domain.Entities.User.Create(email, username);
        var hashedPassword = passwordHasher.HashPassword(user, password);
        user.SetPasswordHash(hashedPassword);
        
        // Ensure owned entities are created (needed for InMemory database)
        var userType = user.GetType();
        var statsProperty = userType.GetProperty("Stats");
        var streakProperty = userType.GetProperty("Streak");
        var preferencesProperty = userType.GetProperty("Preferences");
        var premiumProperty = userType.GetProperty("Premium");
        
        if (statsProperty?.GetValue(user) == null)
            statsProperty?.SetValue(user, LexiQuest.Core.Domain.ValueObjects.UserStats.CreateDefault());
        if (streakProperty?.GetValue(user) == null)
            streakProperty?.SetValue(user, LexiQuest.Core.Domain.ValueObjects.Streak.CreateDefault());
        if (preferencesProperty?.GetValue(user) == null)
            preferencesProperty?.SetValue(user, LexiQuest.Core.Domain.ValueObjects.UserPreferences.CreateDefault());
        if (premiumProperty?.GetValue(user) == null)
            premiumProperty?.SetValue(user, LexiQuest.Core.Domain.ValueObjects.PremiumStatus.CreateDefault());
        
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        
        // Verify user was saved correctly
        var savedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (savedUser == null)
            throw new InvalidOperationException("User was not saved to database");
        if (string.IsNullOrEmpty(savedUser.PasswordHash))
            throw new InvalidOperationException("PasswordHash was not saved");
        
        // Verify password can be verified
        var verificationResult = passwordHasher.VerifyHashedPassword(savedUser, savedUser.PasswordHash, password);
        if (verificationResult != PasswordVerificationResult.Success)
            throw new InvalidOperationException($"Password verification failed: {verificationResult}");
        
        return (client, factory);
    }

    [Fact]
    public async Task LoginEndpoint_ValidCredentials_Returns200WithTokens()
    {
        // Arrange
        var (client, factory) = await CreateClientWithUserAsync("test@example.com", "Password123!", "testuser");
        using var _ = factory;
        
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/login", request);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LoginEndpoint_InvalidCredentials_Returns401()
    {
        // Arrange
        var (client, factory) = await CreateClientWithUserAsync("test@example.com", "Password123!", "testuser");
        using var _ = factory;
        
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword123!",
            RememberMe = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginEndpoint_NonexistentEmail_Returns401()
    {
        // Arrange
        var client = CreateClient();
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginEndpoint_InvalidRequest_Returns400()
    {
        // Arrange
        var client = CreateClient();
        var request = new LoginRequest
        {
            Email = "", // Invalid - empty
            Password = "Password123!",
            RememberMe = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LoginEndpoint_LockedAccount_Returns423()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        
        // Create locked user
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<LexiQuest.Core.Domain.Entities.User>>();
        
        var user = LexiQuest.Core.Domain.Entities.User.Create("locked@example.com", "lockeduser");
        var hashedPassword = passwordHasher.HashPassword(user, "Password123!");
        user.SetPasswordHash(hashedPassword);
        user.LockAccountUntil(DateTime.UtcNow.AddMinutes(15));
        
        // Ensure owned entities are created
        var userType = user.GetType();
        var statsProperty = userType.GetProperty("Stats");
        var streakProperty = userType.GetProperty("Streak");
        var preferencesProperty = userType.GetProperty("Preferences");
        var premiumProperty = userType.GetProperty("Premium");
        
        if (statsProperty?.GetValue(user) == null)
            statsProperty?.SetValue(user, LexiQuest.Core.Domain.ValueObjects.UserStats.CreateDefault());
        if (streakProperty?.GetValue(user) == null)
            streakProperty?.SetValue(user, LexiQuest.Core.Domain.ValueObjects.Streak.CreateDefault());
        if (preferencesProperty?.GetValue(user) == null)
            preferencesProperty?.SetValue(user, LexiQuest.Core.Domain.ValueObjects.UserPreferences.CreateDefault());
        if (premiumProperty?.GetValue(user) == null)
            premiumProperty?.SetValue(user, LexiQuest.Core.Domain.ValueObjects.PremiumStatus.CreateDefault());
        
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        
        using var _ = factory;

        var request = new LoginRequest
        {
            Email = "locked@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Locked); // 423
    }
}
