using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Admin;

namespace LexiQuest.Blazor.Services;

public class AdminService : IAdminService
{
    private readonly HttpClient _httpClient;

    public AdminService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<AdminDashboardStatsDto?> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/admin/dashboard/stats", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AdminDashboardStatsDto>(cancellationToken: cancellationToken);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/admin/check", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PaginatedResult<AdminWordDto>> GetWordsAsync(AdminWordListRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildWordQuery(request);
            var response = await _httpClient.GetAsync($"api/admin/words?{query}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PaginatedResult<AdminWordDto>>(cancellationToken: cancellationToken)
                    ?? new PaginatedResult<AdminWordDto>(new List<AdminWordDto>(), 0, 1, 25);
            }
            return new PaginatedResult<AdminWordDto>(new List<AdminWordDto>(), 0, 1, 25);
        }
        catch
        {
            return new PaginatedResult<AdminWordDto>(new List<AdminWordDto>(), 0, 1, 25);
        }
    }

    public async Task<AdminWordDto?> CreateWordAsync(AdminWordCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/words", request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AdminWordDto>(cancellationToken: cancellationToken);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteWordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/words/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PaginatedResult<AdminUserDto>> GetUsersAsync(AdminUserListRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildUserQuery(request);
            var response = await _httpClient.GetAsync($"api/admin/users?{query}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PaginatedResult<AdminUserDto>>(cancellationToken: cancellationToken)
                    ?? new PaginatedResult<AdminUserDto>(new List<AdminUserDto>(), 0, 1, 25);
            }
            return new PaginatedResult<AdminUserDto>(new List<AdminUserDto>(), 0, 1, 25);
        }
        catch
        {
            return new PaginatedResult<AdminUserDto>(new List<AdminUserDto>(), 0, 1, 25);
        }
    }

    public async Task<bool> SuspendUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/admin/users/{id}/suspend", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UnsuspendUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/admin/users/{id}/unsuspend", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string BuildWordQuery(AdminWordListRequest request)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(request.Search)) parts.Add($"search={Uri.EscapeDataString(request.Search)}");
        if (!string.IsNullOrEmpty(request.Difficulty)) parts.Add($"difficulty={Uri.EscapeDataString(request.Difficulty)}");
        if (!string.IsNullOrEmpty(request.Category)) parts.Add($"category={Uri.EscapeDataString(request.Category)}");
        parts.Add($"page={request.Page}");
        parts.Add($"pageSize={request.PageSize}");
        return string.Join("&", parts);
    }

    private static string BuildUserQuery(AdminUserListRequest request)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(request.Search)) parts.Add($"search={Uri.EscapeDataString(request.Search)}");
        if (request.IsSuspended.HasValue) parts.Add($"isSuspended={request.IsSuspended.Value}");
        if (request.IsPremium.HasValue) parts.Add($"isPremium={request.IsPremium.Value}");
        parts.Add($"page={request.Page}");
        parts.Add($"pageSize={request.PageSize}");
        return string.Join("&", parts);
    }
}
