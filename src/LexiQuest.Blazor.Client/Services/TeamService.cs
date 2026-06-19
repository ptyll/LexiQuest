using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Teams;

namespace LexiQuest.Blazor.Services;

public class TeamService : ITeamService
{
    private readonly HttpClient _httpClient;

    public TeamService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<TeamDto?> GetMyTeamAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<TeamDto>("api/v1/users/me/team");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<TeamDto?> GetTeamByIdAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<TeamDto>($"api/v1/teams/{id}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<TeamMemberDto>> GetTeamMembersAsync(Guid teamId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<TeamMemberDto>>($"api/v1/teams/{teamId}/members");
            return result ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }

    public async Task<TeamDto?> CreateTeamAsync(CreateTeamRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/teams", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TeamDto>();
        }
        return null;
    }

    public async Task<bool> LeaveTeamAsync(Guid teamId)
    {
        var response = await _httpClient.PostAsync($"api/v1/teams/{teamId}/leave", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DisbandTeamAsync(Guid teamId)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/teams/{teamId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> InviteMemberAsync(Guid teamId, InviteMemberRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/v1/teams/{teamId}/invite", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> KickMemberAsync(Guid teamId, Guid userId)
    {
        var response = await _httpClient.PostAsync($"api/v1/teams/{teamId}/kick/{userId}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> TransferLeadershipAsync(Guid teamId, Guid newLeaderId)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/v1/teams/{teamId}/transfer-leadership", new { UserId = newLeaderId });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RequestJoinAsync(Guid teamId, CreateJoinRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/v1/teams/{teamId}/join-request", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveJoinRequestAsync(Guid requestId)
    {
        var response = await _httpClient.PostAsync($"api/v1/teams/join-requests/{requestId}/approve", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RejectJoinRequestAsync(Guid requestId)
    {
        var response = await _httpClient.PostAsync($"api/v1/teams/join-requests/{requestId}/reject", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<TeamInviteDto>> GetMyInvitesAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<TeamInviteDto>>("api/v1/teams/invites/my");
            return result ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }

    public async Task<List<TeamJoinRequestDto>> GetJoinRequestsAsync(Guid teamId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<TeamJoinRequestDto>>($"api/v1/teams/{teamId}/join-requests");
            return result ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }

    public async Task<List<TeamRankingDto>> GetRankingAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<TeamRankingDto>>("api/v1/teams/ranking");
            return result ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }

    public async Task<bool> CanCreateTeamAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<bool>("api/v1/teams/can-create");
            return result;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
