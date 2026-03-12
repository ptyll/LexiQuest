using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class AuthFlowTests
{
    private static readonly string TestDbName = $"AuthFlowTestDb_{Guid.NewGuid()}";

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

    private RegisterRequest CreateUniqueRegisterRequest()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        return new RegisterRequest
        {
            Username = $"testuser_{uniqueId}",
            Email = $"test_{uniqueId}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            AcceptTerms = true
        };
    }

    [Fact]
    public async Task Register_Login_AccessProtectedEndpoint_FullFlow()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        await EnsureDbCreated(factory);

        var registerRequest = CreateUniqueRegisterRequest();

        // Act - Register
        var registerResponse = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        // Assert - Register succeeds
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.User.Should().NotBeNull();
        authResponse.User.Email.Should().Be(registerRequest.Email);
        authResponse.User.Username.Should().Be(registerRequest.Username);

        // Act - Login with same credentials
        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/users/login", loginRequest);

        // Assert - Login succeeds
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginAuth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        loginAuth.Should().NotBeNull();
        loginAuth!.AccessToken.Should().NotBeNullOrEmpty();

        // Act - Access protected endpoint with token
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginAuth.AccessToken);
        var profileResponse = await client.GetAsync("/api/v1/users/me");

        // Assert - Protected endpoint accessible
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        await EnsureDbCreated(factory);

        var registerRequest = CreateUniqueRegisterRequest();

        // Register first time
        var firstResponse = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Register again with same email
        var duplicateRequest = new RegisterRequest
        {
            Username = $"other_{Guid.NewGuid().ToString("N")[..8]}",
            Email = registerRequest.Email,
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            AcceptTerms = true
        };
        var secondResponse = await client.PostAsJsonAsync("/api/v1/users/register", duplicateRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        await EnsureDbCreated(factory);

        var registerRequest = CreateUniqueRegisterRequest();
        var registerResponse = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        // Act - Login with wrong password
        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = "WrongPassword123!"
        };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/users/login", loginRequest);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithoutToken_Returns401()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act - Access protected endpoint without auth
        var response = await client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_NonExistentUser_Returns401()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();
        await EnsureDbCreated(factory);

        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_InvalidData_Returns400()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();

        var registerRequest = new RegisterRequest
        {
            Username = "",
            Email = "not-an-email",
            Password = "short",
            ConfirmPassword = "mismatch",
            AcceptTerms = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
