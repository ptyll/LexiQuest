using System.Net.Http.Headers;

namespace LexiQuest.Blazor.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IAuthService _authService;

    public AuthorizationMessageHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Add authorization header if user is authenticated
        if (await _authService.IsAuthenticatedAsync())
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Handle 401 Unauthorized - try to refresh token
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshResult = await _authService.RefreshTokenAsync();
            
            if (refreshResult.Success)
            {
                // Retry the request with new token
                var newToken = await _authService.GetTokenAsync();
                if (!string.IsNullOrEmpty(newToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }
        }

        return response;
    }
}
