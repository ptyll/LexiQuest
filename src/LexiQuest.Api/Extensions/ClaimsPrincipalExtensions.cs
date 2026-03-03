using System.Security.Claims;

namespace LexiQuest.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirst("sub");

        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User ID claim not found.");
        }

        return userId;
    }
}
