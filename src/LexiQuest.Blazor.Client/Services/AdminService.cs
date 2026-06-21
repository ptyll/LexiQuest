using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Admin;

namespace LexiQuest.Blazor.Services;

public class AdminService : IAdminService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public AdminService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("PublicApiClient");
        _authService = authService;
    }

    public async Task<AdminDashboardStatsDto?> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/admin/dashboard/stats", cancellationToken: cancellationToken);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/admin/check", cancellationToken: cancellationToken);
            return response.IsSuccessStatusCode
                && await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CanManageWordsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/admin/check/words", cancellationToken: cancellationToken);
            return response.IsSuccessStatusCode
                && await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"api/v1/admin/words?{query}", cancellationToken: cancellationToken);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Post, "api/v1/admin/words", request, cancellationToken);
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

    public async Task<AdminWordDto?> UpdateWordAsync(Guid id, AdminWordUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Put, $"api/v1/admin/words/{id}", request, cancellationToken);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Delete, $"api/v1/admin/words/{id}", cancellationToken: cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<BulkImportResult?> ImportWordsAsync(string csvContent, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(
                HttpMethod.Post,
                "api/v1/admin/words/import",
                new AdminWordImportRequest(csvContent),
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BulkImportResult>(cancellationToken: cancellationToken);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> ExportWordsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/admin/words/export", cancellationToken: cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<WordStatsDto?> GetWordStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/admin/words/stats", cancellationToken: cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WordStatsDto>(cancellationToken: cancellationToken);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<PaginatedResult<AdminUserDto>> GetUsersAsync(AdminUserListRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildUserQuery(request);
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"api/v1/admin/users?{query}", cancellationToken: cancellationToken);
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
            using var response = await SendAuthorizedAsync(HttpMethod.Post, $"api/v1/admin/users/{id}/suspend", cancellationToken: cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<AdminUserDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"api/v1/admin/users/{id}", cancellationToken: cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AdminUserDto>(cancellationToken: cancellationToken);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UnsuspendUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Post, $"api/v1/admin/users/{id}/unsuspend", cancellationToken: cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ResetUserPasswordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Post, $"api/v1/admin/users/{id}/reset-password", cancellationToken: cancellationToken);
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
        if (request.MinLength.HasValue) parts.Add($"minLength={request.MinLength.Value}");
        if (request.MaxLength.HasValue) parts.Add($"maxLength={request.MaxLength.Value}");
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
        if (request.MinLevel.HasValue) parts.Add($"minLevel={request.MinLevel.Value}");
        if (request.MaxLevel.HasValue) parts.Add($"maxLevel={request.MaxLevel.Value}");
        parts.Add($"page={request.Page}");
        parts.Add($"pageSize={request.PageSize}");
        return string.Join("&", parts);
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpMethod method,
        string requestUri,
        object? jsonBody = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAuthorizedOnceAsync(method, requestUri, jsonBody, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
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
            request.Content = JsonContent.Create(jsonBody);
        }

        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _httpClient.SendAsync(request, cancellationToken);
    }
}
