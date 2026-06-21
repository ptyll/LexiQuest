using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
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
        _httpClientFactory.CreateClient("PublicApiClient").Returns(_httpClient);

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
    public async Task GameService_StartGame_AuthenticatedUser_SendsBearerToken()
    {
        // Arrange
        var authService = Substitute.For<IAuthService>();
        authService.GetTokenAsync().Returns("game-token");

        var service = new GameService(_httpClientFactory, _logger, authService: authService);
        var expectedResponse = new ScrambledWordDto(
            Guid.NewGuid(),
            1,
            "LKBOJA",
            6,
            DifficultyLevel.Beginner,
            30,
            10,
            5);

        _mockHandler.SetResponse(HttpStatusCode.Created, expectedResponse);
        var request = new StartGameRequest(Mode: GameMode.Training, Difficulty: DifficultyLevel.Beginner);

        // Act
        var result = await service.StartGameAsync(request);

        // Assert
        result.Should().NotBeNull();
        var sentRequest = _mockHandler.Requests.Should().ContainSingle().Subject;
        sentRequest.Headers.Authorization.Should().NotBeNull();
        sentRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        sentRequest.Headers.Authorization.Parameter.Should().Be("game-token");
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
    public async Task GameService_SubmitAnswer_PathSession_DoesNotRequestOfflineTrainingSeed()
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

        _mockHandler.SetResponder(request =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(expectedResponse)
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var service = new GameService(_httpClientFactory, _logger, new OnlineJsRuntime());

        // Act
        var result = await service.SubmitAnswerAsync(sessionId, "JABLKO", 5000);

        // Assert
        result.Should().NotBeNull();
        _mockHandler.Requests.Should().ContainSingle(request => request.Method == HttpMethod.Post);
        _mockHandler.Requests
            .Select(request => request.RequestUri?.PathAndQuery ?? string.Empty)
            .Should()
            .NotContain(path => path.Contains("offline-training-seed", StringComparison.Ordinal));
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

    [Fact]
    public async Task GameService_ReplayQueuedRequests_ConcurrentCalls_DoNotSubmitSameQueuedAnswerTwice()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var jsRuntime = new LocalStorageJsRuntime();
        jsRuntime.Storage["lexiquest_offline_game_queue"] = JsonSerializer.Serialize(
            new[]
            {
                new
                {
                    sessionId,
                    answer = "JABLKO",
                    timeSpentMs = 1_000,
                    queuedAt = DateTimeOffset.UtcNow
                }
            },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var firstRequestStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseResponse = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

        _mockHandler.SetAsyncResponder((request, _) =>
        {
            firstRequestStarted.TrySetResult();
            return releaseResponse.Task;
        });

        var service = new GameService(_httpClientFactory, _logger, jsRuntime);

        // Act
        var firstReplay = service.ReplayQueuedRequestsAsync();
        await firstRequestStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        var secondReplay = service.ReplayQueuedRequestsAsync();

        releaseResponse.SetResult(new HttpResponseMessage(HttpStatusCode.OK));
        await Task.WhenAll(firstReplay, secondReplay);

        // Assert
        _mockHandler.Requests.Count(request => request.Method == HttpMethod.Post).Should().Be(1);
        jsRuntime.Storage.Should().NotContainKey("lexiquest_offline_game_queue");
    }

    /// <summary>
    /// Mock HTTP message handler for testing.
    /// </summary>
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpStatusCode _statusCode;
        private object? _responseContent;
        private Func<HttpRequestMessage, HttpResponseMessage>? _responder;
        private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? _asyncResponder;

        public List<HttpRequestMessage> Requests { get; } = [];

        public void SetResponse(HttpStatusCode statusCode, object? content)
        {
            _statusCode = statusCode;
            _responseContent = content;
            _responder = null;
            _asyncResponder = null;
        }

        public void SetResponder(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
            _asyncResponder = null;
        }

        public void SetAsyncResponder(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
        {
            _asyncResponder = responder;
            _responder = null;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);

            if (_asyncResponder is not null)
            {
                return _asyncResponder(request, cancellationToken);
            }

            if (_responder is not null)
            {
                return Task.FromResult(_responder(request));
            }

            var response = new HttpResponseMessage(_statusCode);
            
            if (_responseContent != null)
            {
                response.Content = JsonContent.Create(_responseContent);
            }

            return Task.FromResult(response);
        }
    }

    private sealed class OnlineJsRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args)
        {
            if (identifier == "lexiQuestPwa.getOnlineStatus")
            {
                return ValueTask.FromResult((TValue)(object)true);
            }

            return ValueTask.FromResult(default(TValue)!);
        }
    }

    private sealed class LocalStorageJsRuntime : IJSRuntime
    {
        public Dictionary<string, string> Storage { get; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args)
        {
            if (identifier == "lexiQuestPwa.getOnlineStatus")
            {
                return ValueTask.FromResult((TValue)(object)true);
            }

            if (identifier == "localStorage.getItem")
            {
                var key = (string)args![0]!;
                Storage.TryGetValue(key, out var value);
                return ValueTask.FromResult((TValue)(object?)value!);
            }

            if (identifier == "localStorage.setItem")
            {
                Storage[(string)args![0]!] = (string)args[1]!;
                return ValueTask.FromResult(default(TValue)!);
            }

            if (identifier == "localStorage.removeItem")
            {
                Storage.Remove((string)args![0]!);
                return ValueTask.FromResult(default(TValue)!);
            }

            return ValueTask.FromResult(default(TValue)!);
        }
    }
}
