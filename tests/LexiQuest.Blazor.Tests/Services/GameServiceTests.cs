using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Services;

public class GameServiceTests
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GameService> _logger;
    private readonly GameService _gameService;
    private readonly MockHttpMessageHandler _mockHandler;

    public GameServiceTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler)
        {
            BaseAddress = new Uri("https://localhost:5000/")
        };

        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClientFactory.CreateClient("LexiQuestApi").Returns(_httpClient);

        _logger = Substitute.For<ILogger<GameService>>();
        _gameService = new GameService(_httpClientFactory, _logger);
    }

    [Fact]
    public async Task GameService_StartGame_ReturnsScrambledWord()
    {
        // Arrange
        var expectedResponse = new ScrambledWordDto(
            Guid.NewGuid(),
            1,
            "LKBOJA",
            6,
            DifficultyLevel.Beginner,
            30,
            10,
            5
        );

        _mockHandler.SetResponse(HttpStatusCode.Created, expectedResponse);
        var request = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);

        // Act
        var result = await _gameService.StartGameAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.ScrambledWord.Should().Be("LKBOJA");
        result.RoundNumber.Should().Be(1);
    }

    [Fact]
    public async Task GameService_StartGame_Unauthorized_ReturnsNull()
    {
        // Arrange
        _mockHandler.SetResponse(HttpStatusCode.Unauthorized, null);
        var request = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);

        // Act
        var result = await _gameService.StartGameAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GameService_SubmitAnswer_ReturnsResult()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedResponse = new GameRoundResult(
            IsCorrect: true,
            CorrectAnswer: "JABLKO",
            XPEarned: 15,
            SpeedBonus: 3,
            ComboCount: 1,
            IsLevelComplete: false,
            LivesRemaining: 5,
            NextScrambledWord: "BANÁN",
            NextRoundNumber: 2,
            IsGameOver: false
        );

        _mockHandler.SetResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _gameService.SubmitAnswerAsync(sessionId, "JABLKO", 5000);

        // Assert
        result.Should().NotBeNull();
        result!.IsCorrect.Should().BeTrue();
        result.XPEarned.Should().Be(15);
        result.CorrectAnswer.Should().Be("JABLKO");
    }

    [Fact]
    public async Task GameService_SubmitAnswer_WrongAnswer_ReturnsZeroXP()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedResponse = new GameRoundResult(
            IsCorrect: false,
            CorrectAnswer: "JABLKO",
            XPEarned: 0,
            SpeedBonus: 0,
            ComboCount: 0,
            IsLevelComplete: false,
            LivesRemaining: 4,
            NextScrambledWord: null,
            NextRoundNumber: null,
            IsGameOver: false
        );

        _mockHandler.SetResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _gameService.SubmitAnswerAsync(sessionId, "WRONG", 5000);

        // Assert
        result.Should().NotBeNull();
        result!.IsCorrect.Should().BeFalse();
        result.XPEarned.Should().Be(0);
    }

    [Fact]
    public async Task GameService_GetGameState_ReturnsState()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedResponse = new ScrambledWordDto(
            sessionId,
            3,
            "ABNÁN",
            5,
            DifficultyLevel.Beginner,
            30,
            10,
            4
        );

        _mockHandler.SetResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _gameService.GetGameStateAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result!.SessionId.Should().Be(sessionId);
        result.RoundNumber.Should().Be(3);
    }

    [Fact]
    public async Task GameService_GetGameState_NotFound_ReturnsNull()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _mockHandler.SetResponse(HttpStatusCode.NotFound, null);

        // Act
        var result = await _gameService.GetGameStateAsync(sessionId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GameService_ForfeitGame_ReturnsTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _mockHandler.SetResponse(HttpStatusCode.NoContent, null);

        // Act
        var result = await _gameService.ForfeitGameAsync(sessionId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GameService_ForfeitGame_Failure_ReturnsFalse()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _mockHandler.SetResponse(HttpStatusCode.BadRequest, null);

        // Act
        var result = await _gameService.ForfeitGameAsync(sessionId);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Mock HTTP message handler for testing.
    /// </summary>
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpStatusCode _statusCode;
        private object? _responseContent;

        public void SetResponse(HttpStatusCode statusCode, object? content)
        {
            _statusCode = statusCode;
            _responseContent = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode);
            
            if (_responseContent != null)
            {
                response.Content = JsonContent.Create(_responseContent);
            }

            return Task.FromResult(response);
        }
    }
}
