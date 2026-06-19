using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// Implementation of game service using HTTP client.
/// </summary>
public class GameService : IGameService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GameService> _logger;

    public GameService(IHttpClientFactory httpClientFactory, ILogger<GameService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("LexiQuestApi");
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ScrambledWordDto?> StartGameAsync(StartGameRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/game/start", request, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized attempt to start game");
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ScrambledWordDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<GameRoundResult?> SubmitAnswerAsync(Guid sessionId, string answer, int timeSpentMs, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new SubmitAnswerRequest
            {
                SessionId = sessionId,
                Answer = answer,
                TimeSpentMs = timeSpentMs
            };

            var response = await _httpClient.PostAsJsonAsync($"api/v1/game/{sessionId}/answer", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GameRoundResult>(cancellationToken);
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
            var response = await _httpClient.GetAsync($"api/v1/game/{sessionId}", cancellationToken);
            
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
            var response = await _httpClient.PostAsync($"api/v1/game/{sessionId}/forfeit", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to forfeit game for session {SessionId}", sessionId);
            return false;
        }
    }
}
