using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace LexiQuest.Api.Tests.Endpoints;

public class GuestEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly IGuestSessionService _guestSessionService;
    private readonly IGuestLimiter _guestLimiter;

    public GuestEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _guestSessionService = Substitute.For<IGuestSessionService>();
        _guestLimiter = Substitute.For<IGuestLimiter>();

        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _guestSessionService);
                services.AddSingleton(_ => _guestLimiter);
            });
        });

        _client = customFactory.CreateClient();
    }

    [Fact]
    public async Task GuestStartEndpoint_Returns200_WithScrambledWord()
    {
        // Arrange
        _guestLimiter.CanStartGame(Arg.Any<string>()).Returns(new GuestLimitResult
        {
            Allowed = true,
            RemainingGames = 4
        });

        _guestSessionService.StartGame().Returns(new GuestSessionResult
        {
            SessionId = Guid.NewGuid(),
            ScrambledWords = new List<ScrambledWordInfo>
            {
                new() { WordId = Guid.NewGuid(), Scrambled = "sep", Original = "pes", Length = 3 }
            },
            IsGuest = true
        });

        // Act
        var response = await _client.PostAsync("/api/v1/game/guest/start", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GuestStartResponse>();
        result.Should().NotBeNull();
        result!.SessionId.Should().NotBeEmpty();
        result.ScrambledWords.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GuestStartEndpoint_LimitReached_Returns429()
    {
        // Arrange
        _guestLimiter.CanStartGame(Arg.Any<string>()).Returns(new GuestLimitResult
        {
            Allowed = false,
            RemainingGames = 0,
            Message = "Denní limit dosažen"
        });

        // Act
        var response = await _client.PostAsync("/api/v1/game/guest/start", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task GuestAnswerEndpoint_Returns200_WithResult()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var wordId = Guid.NewGuid();
        var request = new GuestAnswerRequest(sessionId, wordId, "pes");

        _guestSessionService.SubmitAnswer(sessionId, wordId, "pes")
            .Returns(new GuestAnswerResult
            {
                IsCorrect = true,
                XpEarned = 10,
                CorrectAnswer = "pes",
                TotalSessionXp = 10,
                WordsSolved = 1,
                WordsRemaining = 4
            });

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/game/guest/answer", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GuestAnswerResponse>();
        result.Should().NotBeNull();
        result!.IsCorrect.Should().BeTrue();
        result.XpEarned.Should().Be(10);
    }

    [Fact]
    public async Task GuestStatusEndpoint_Returns200_WithRemainingGames()
    {
        // Arrange
        _guestLimiter.GetStatus(Arg.Any<string>()).Returns(new GuestLimitStatus
        {
            TotalAllowed = 5,
            Used = 2,
            Remaining = 3
        });

        // Act
        var response = await _client.GetAsync("/api/v1/guest/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GuestStatusResponse>();
        result.Should().NotBeNull();
        result!.TotalAllowed.Should().Be(5);
        result.Used.Should().Be(2);
        result.Remaining.Should().Be(3);
    }

    [Fact]
    public async Task GuestConvertEndpoint_Returns200_WithTransferData()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _guestSessionService.GetSessionProgress(sessionId).Returns(new GuestSessionProgress(50, 3));
        _guestSessionService.EndGame(sessionId).Returns(new GuestSessionResult
        {
            SessionId = sessionId,
            IsGuest = true
        });

        var request = new GuestConvertRequest(sessionId, TransferProgress: true);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/game/guest/convert", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GuestConvertResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.TransferredXp.Should().Be(50);
        result.TransferredWordsSolved.Should().Be(3);
    }

    [Fact]
    public async Task GuestConvertEndpoint_WithoutTransferProgress_ReturnsZeroTransferData()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _guestSessionService.GetSessionProgress(sessionId).Returns(new GuestSessionProgress(50, 3));
        _guestSessionService.EndGame(sessionId).Returns(new GuestSessionResult
        {
            SessionId = sessionId,
            IsGuest = true
        });

        var request = new GuestConvertRequest(sessionId, TransferProgress: false);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/game/guest/convert", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GuestConvertResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.TransferredXp.Should().Be(0);
        result.TransferredWordsSolved.Should().Be(0);
    }
}

// DTOs for tests
public record GuestStartResponse(Guid SessionId, List<ScrambledWordDto> ScrambledWords, int RemainingGames);
public record ScrambledWordDto(Guid WordId, string Scrambled, int Length);
public record GuestAnswerRequest(Guid SessionId, Guid WordId, string Answer);
public record GuestAnswerResponse(bool IsCorrect, int XpEarned, string CorrectAnswer, int TotalSessionXp, int WordsSolved, int WordsRemaining);
public record GuestStatusResponse(int TotalAllowed, int Used, int Remaining);
public record GuestConvertRequest(Guid SessionId, bool TransferProgress);
public record GuestConvertResponse(bool Success, string? UserId, string Message, int TransferredXp, int TransferredWordsSolved);
