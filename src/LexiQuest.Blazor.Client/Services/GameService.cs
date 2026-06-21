using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.JSInterop;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// Implementation of game service using HTTP client.
/// </summary>
public class GameService : IGameService
{
    private const string OfflineTrainingSeedKey = "lexiquest_offline_training_seed";
    private const string OfflineGameQueueKey = "lexiquest_offline_game_queue";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<GameService> _logger;
    private readonly IAuthService? _authService;
    private readonly IJSRuntime? _jsRuntime;
    private readonly HashSet<Guid> _offlineTrainingSessionIds = [];
    private int _isReplayingOfflineQueue;

    public GameService(
        IHttpClientFactory httpClientFactory,
        ILogger<GameService> logger,
        IJSRuntime? jsRuntime = null,
        IAuthService? authService = null)
    {
        _httpClient = httpClientFactory.CreateClient("PublicApiClient");
        _logger = logger;
        _jsRuntime = jsRuntime;
        _authService = authService;
    }

    /// <inheritdoc />
    public async Task<ScrambledWordDto?> StartGameAsync(StartGameRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Mode == GameMode.Training && !await IsOnlineAsync())
        {
            return await StartOfflineTrainingAsync();
        }

        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Post, "api/v1/game/start", request, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized attempt to start game");
                return null;
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ScrambledWordDto>(cancellationToken);

            if (result is not null && request.Mode == GameMode.Training)
            {
                _offlineTrainingSessionIds.Add(result.SessionId);
                await CacheOfflineTrainingSeedAsync(result.SessionId, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game");
            return request.Mode == GameMode.Training
                ? await StartOfflineTrainingAsync()
                : null;
        }
    }

    /// <inheritdoc />
    public async Task<GameRoundResult?> SubmitAnswerAsync(Guid sessionId, string answer, int timeSpentMs, CancellationToken cancellationToken = default)
    {
        if (!await IsOnlineAsync())
        {
            return await SubmitOfflineTrainingAnswerAsync(sessionId, answer, timeSpentMs);
        }

        try
        {
            var request = new SubmitAnswerRequest
            {
                SessionId = sessionId,
                Answer = answer,
                TimeSpentMs = timeSpentMs
            };

            using var response = await SendAuthorizedAsync(
                HttpMethod.Post,
                $"api/v1/game/{sessionId}/answer",
                request,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<GameRoundResult>(cancellationToken);

            if (result is not null
                && result.IsCorrect
                && !result.IsLevelComplete
                && !result.IsGameOver
                && result.NextScrambledWord is not null
                && _offlineTrainingSessionIds.Contains(sessionId))
            {
                await CacheOfflineTrainingSeedAsync(sessionId, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit answer for session {SessionId}", sessionId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<ScrambledWordDto?> GetGameStateAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(
                HttpMethod.Get,
                $"api/v1/game/{sessionId}",
                cancellationToken: cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ScrambledWordDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get game state for session {SessionId}", sessionId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ForfeitGameAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(
                HttpMethod.Post,
                $"api/v1/game/{sessionId}/forfeit",
                cancellationToken: cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to forfeit game for session {SessionId}", sessionId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task ReplayQueuedRequestsAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _isReplayingOfflineQueue, 1) == 1)
        {
            return;
        }

        try
        {
            if (!await IsOnlineAsync())
            {
                return;
            }

            var queued = await GetOfflineQueueAsync();
            if (queued.Count == 0)
            {
                return;
            }

            var remaining = new List<OfflineGameQueueItem>();

            foreach (var item in queued)
            {
                try
                {
                    var request = new SubmitAnswerRequest
                    {
                        SessionId = item.SessionId,
                        Answer = item.Answer,
                        TimeSpentMs = item.TimeSpentMs
                    };

                    using var response = await SendAuthorizedAsync(
                        HttpMethod.Post,
                        $"api/v1/game/{item.SessionId}/answer",
                        request,
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        remaining.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to replay offline game answer for session {SessionId}", item.SessionId);
                    remaining.Add(item);
                }
            }

            await SaveOfflineQueueAsync(remaining);
        }
        finally
        {
            Volatile.Write(ref _isReplayingOfflineQueue, 0);
        }
    }

    private async Task CacheOfflineTrainingSeedAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        if (_jsRuntime is null)
        {
            return;
        }

        try
        {
            using var response = await SendAuthorizedAsync(
                HttpMethod.Get,
                $"api/v1/game/{sessionId}/offline-training-seed",
                cancellationToken: cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            var seed = await response.Content.ReadFromJsonAsync<OfflineTrainingSeedResponse>(
                JsonOptions,
                cancellationToken);

            if (seed is not null && seed.Words.Count > 0)
            {
                await SetLocalStorageAsync(OfflineTrainingSeedKey, JsonSerializer.Serialize(seed, JsonOptions));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache offline training seed for session {SessionId}", sessionId);
        }
    }

    private async Task<ScrambledWordDto?> StartOfflineTrainingAsync()
    {
        var seed = await GetOfflineTrainingSeedAsync();
        var word = seed?.Words.FirstOrDefault(w => w.RoundNumber == seed.CurrentRound)
            ?? seed?.Words.FirstOrDefault();

        if (seed is null || word is null)
        {
            return null;
        }

        return new ScrambledWordDto(
            SessionId: seed.SessionId,
            RoundNumber: word.RoundNumber,
            ScrambledWord: word.ScrambledWord,
            WordLength: word.WordLength,
            Difficulty: seed.Difficulty,
            TimeLimitSeconds: word.TimeLimitSeconds,
            TotalRounds: seed.TotalRounds,
            LivesRemaining: seed.LivesRemaining,
            MaxLives: seed.MaxLives,
            IsInfiniteLives: seed.IsInfiniteLives);
    }

    private async Task<GameRoundResult?> SubmitOfflineTrainingAnswerAsync(Guid sessionId, string answer, int timeSpentMs)
    {
        var seed = await GetOfflineTrainingSeedAsync();
        var word = seed?.Words.FirstOrDefault(w => w.RoundNumber == seed.CurrentRound)
            ?? seed?.Words.FirstOrDefault();

        if (seed is null || word is null || seed.SessionId != sessionId)
        {
            return null;
        }

        await EnqueueOfflineAnswerAsync(sessionId, answer, timeSpentMs);

        var isCorrect = string.Equals(
            answer?.Trim(),
            word.CorrectAnswer,
            StringComparison.OrdinalIgnoreCase);

        return new GameRoundResult(
            IsCorrect: isCorrect,
            CorrectAnswer: word.CorrectAnswer,
            XPEarned: 0,
            SpeedBonus: 0,
            ComboCount: isCorrect ? 1 : 0,
            IsLevelComplete: false,
            LivesRemaining: seed.LivesRemaining,
            NextScrambledWord: null,
            NextRoundNumber: null,
            IsGameOver: false);
    }

    private async Task EnqueueOfflineAnswerAsync(Guid sessionId, string answer, int timeSpentMs)
    {
        var queued = await GetOfflineQueueAsync();
        queued.Add(new OfflineGameQueueItem(sessionId, answer, timeSpentMs, DateTimeOffset.UtcNow));
        await SaveOfflineQueueAsync(queued);
    }

    private async Task<OfflineTrainingSeedResponse?> GetOfflineTrainingSeedAsync()
    {
        var serialized = await GetLocalStorageAsync(OfflineTrainingSeedKey);
        return string.IsNullOrWhiteSpace(serialized)
            ? null
            : JsonSerializer.Deserialize<OfflineTrainingSeedResponse>(serialized, JsonOptions);
    }

    private async Task<List<OfflineGameQueueItem>> GetOfflineQueueAsync()
    {
        var serialized = await GetLocalStorageAsync(OfflineGameQueueKey);
        return string.IsNullOrWhiteSpace(serialized)
            ? []
            : JsonSerializer.Deserialize<List<OfflineGameQueueItem>>(serialized, JsonOptions) ?? [];
    }

    private async Task SaveOfflineQueueAsync(List<OfflineGameQueueItem> queued)
    {
        if (queued.Count == 0)
        {
            await RemoveLocalStorageAsync(OfflineGameQueueKey);
            return;
        }

        await SetLocalStorageAsync(OfflineGameQueueKey, JsonSerializer.Serialize(queued, JsonOptions));
    }

    private async Task<bool> IsOnlineAsync()
    {
        if (_jsRuntime is null)
        {
            return true;
        }

        try
        {
            return await _jsRuntime.InvokeAsync<bool>("lexiQuestPwa.getOnlineStatus");
        }
        catch
        {
            return true;
        }
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpMethod method,
        string requestUri,
        object? jsonBody = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAuthorizedOnceAsync(method, requestUri, jsonBody, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized || _authService is null)
        {
            return response;
        }

        response.Dispose();
        var refreshResult = await _authService.RefreshTokenAsync();
        if (!refreshResult.Success)
        {
            return await SendAuthorizedOnceAsync(method, requestUri, jsonBody, cancellationToken);
        }

        return await SendAuthorizedOnceAsync(method, requestUri, jsonBody, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAuthorizedOnceAsync(
        HttpMethod method,
        string requestUri,
        object? jsonBody,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        if (jsonBody is not null)
        {
            request.Content = JsonContent.Create(jsonBody, options: JsonOptions);
        }

        if (_authService is not null)
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private async Task<string?> GetLocalStorageAsync(string key)
    {
        if (_jsRuntime is null)
        {
            return null;
        }

        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch
        {
            return null;
        }
    }

    private async Task SetLocalStorageAsync(string key, string value)
    {
        if (_jsRuntime is null)
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write localStorage key {Key}", key);
        }
    }

    private async Task RemoveLocalStorageAsync(string key)
    {
        if (_jsRuntime is null)
        {
            return;
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove localStorage key {Key}", key);
        }
    }

    private sealed record OfflineGameQueueItem(
        Guid SessionId,
        string Answer,
        int TimeSpentMs,
        DateTimeOffset QueuedAt);
}
