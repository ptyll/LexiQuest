using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Api;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.Endpoints;

public class UserSettingsEndpointsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;

    public UserSettingsEndpointsTests()
    {
        _factory = new CustomWebApplicationFactory("TestDb_UserSettings");

        // Ensure database is created
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private async Task AuthenticateAsync(HttpClient client)
    {
        // Register and login a test user
        var registerRequest = new RegisterRequest
        {
            Username = "testuser_settings",
            Email = "testuser_settings@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            AcceptTerms = true
        };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        registerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);

        var loginRequest = new LoginRequest
        {
            Email = "testuser_settings@example.com",
            Password = "Test123!"
        };

        var loginResponse = await client.PostAsJsonAsync("/api/v1/users/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);
    }

    [Fact]
    public async Task GetUserProfile_Returns200WithProfile()
    {
        // Arrange
        var client = _factory.CreateClient();
        await AuthenticateAsync(client);

        // Act
        var response = await client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        profile.Should().NotBeNull();
        profile!.Username.Should().NotBeNullOrEmpty();
        profile.Email.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetUserProfile_Unauthorized_Returns401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_ValidData_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        await AuthenticateAsync(client);
        var request = new UpdateProfileRequest
        {
            Username = "updateduser",
            Email = "updated@example.com"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateProfile_EmptyUsername_Returns400()
    {
        // Arrange
        var client = _factory.CreateClient();
        await AuthenticateAsync(client);
        var request = new UpdateProfileRequest
        {
            Username = "",
            Email = "test@example.com"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/users/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_ValidOldPassword_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        await AuthenticateAsync(client);
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "Test123!",
            NewPassword = "NewPass123!",
            ConfirmPassword = "NewPass123!"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/users/me/password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_InvalidOldPassword_Returns400()
    {
        // Arrange
        var client = _factory.CreateClient();
        await AuthenticateAsync(client);
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPass123!",
            ConfirmPassword = "NewPass123!"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/users/me/password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePreferences_ValidData_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        await AuthenticateAsync(client);
        var request = new UserPreferencesDto
        {
            Theme = "dark",
            Language = "cs",
            AnimationsEnabled = true,
            SoundsEnabled = false
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/users/me/preferences", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdatePrivacySettings_ValidData_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        await AuthenticateAsync(client);
        var request = new PrivacySettingsDto
        {
            ProfileVisibility = ProfileVisibility.Friends,
            LeaderboardVisible = true,
            StatsSharingEnabled = false
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/users/me/privacy", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
