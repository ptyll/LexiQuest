using System.Net;
using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Blazor.Services;

public class PathService : IPathService
{
    private readonly IAuthenticatedApiClient _apiClient;

    public PathService(IAuthenticatedApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<LearningPathDto>> GetPathsAsync()
    {
        return await _apiClient.GetFromJsonAsync<List<LearningPathDto>>("api/v1/paths")
            ?? [];
    }

    public async Task<PathProgressDto?> GetPathProgressAsync(Guid pathId)
    {
        var response = await _apiClient.GetAsync($"api/v1/paths/{pathId}/progress");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PathProgressDto>();
    }
}
