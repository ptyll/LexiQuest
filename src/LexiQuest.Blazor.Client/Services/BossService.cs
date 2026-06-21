using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Blazor.Services;

public class BossService : IBossService
{
    private readonly HttpClient _httpClient;

    public BossService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<BossSessionDto> StartBossGameAsync(
        BossType bossType,
        DifficultyLevel difficulty,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/v1/boss/start",
            new BossStartRequest(bossType, difficulty),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var session = await response.Content.ReadFromJsonAsync<BossSessionDto>(cancellationToken: cancellationToken);
        return session ?? throw new InvalidOperationException("Boss session response was empty.");
    }

    public async Task<BossSessionDto?> GetBossStateAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/v1/boss/{sessionId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BossSessionDto>(cancellationToken: cancellationToken);
    }

    public async Task<BossRoundResultDto> SubmitAnswerAsync(
        Guid sessionId,
        string answer,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"api/v1/boss/{sessionId}/answer",
            new BossAnswerRequest
            {
                Answer = answer,
                TimeSpentMs = 1000
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BossRoundResultDto>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("Boss answer response was empty.");
    }

    public async Task<TwistRevealStateDto?> GetTwistRevealStateAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/v1/boss/{sessionId}/twist-reveal", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TwistRevealStateDto>(cancellationToken: cancellationToken);
    }
}
