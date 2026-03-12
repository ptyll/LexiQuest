using FluentValidation;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Endpoints.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        // Get current user profile
        group.MapGet("/me", async (
            IUserService userService,
            CancellationToken cancellationToken,
            HttpContext httpContext) =>
        {
            var userId = GetCurrentUserId(httpContext);
            if (userId == null)
                return Results.Unauthorized();

            var profile = await userService.GetProfileAsync(userId.Value, cancellationToken);
            return profile != null 
                ? Results.Ok(profile) 
                : Results.NotFound();
        })
        .WithName("GetUserProfile")
        .Produces<UserProfileDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        // Update profile
        group.MapPut("/me", async (
            UpdateProfileRequest request,
            IValidator<UpdateProfileRequest> validator,
            IUserService userService,
            CancellationToken cancellationToken,
            HttpContext httpContext) =>
        {
            var userId = GetCurrentUserId(httpContext);
            if (userId == null)
                return Results.Unauthorized();

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            try
            {
                var success = await userService.UpdateProfileAsync(userId.Value, request, cancellationToken);
                return success 
                    ? Results.Ok() 
                    : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .WithName("UpdateUserProfile")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        // Change password
        group.MapPut("/me/password", async (
            ChangePasswordRequest request,
            IValidator<ChangePasswordRequest> validator,
            IUserService userService,
            CancellationToken cancellationToken,
            HttpContext httpContext) =>
        {
            var userId = GetCurrentUserId(httpContext);
            if (userId == null)
                return Results.Unauthorized();

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            try
            {
                var success = await userService.ChangePasswordAsync(userId.Value, request, cancellationToken);
                return success 
                    ? Results.Ok() 
                    : Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("ChangePassword")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        // Update preferences
        group.MapPut("/me/preferences", async (
            UserPreferencesDto preferences,
            IUserService userService,
            CancellationToken cancellationToken,
            HttpContext httpContext) =>
        {
            var userId = GetCurrentUserId(httpContext);
            if (userId == null)
                return Results.Unauthorized();

            var success = await userService.UpdatePreferencesAsync(userId.Value, preferences, cancellationToken);
            return success 
                ? Results.Ok() 
                : Results.NotFound();
        })
        .WithName("UpdateUserPreferences")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        // Update privacy settings
        group.MapPut("/me/privacy", async (
            PrivacySettingsDto privacy,
            IUserService userService,
            CancellationToken cancellationToken,
            HttpContext httpContext) =>
        {
            var userId = GetCurrentUserId(httpContext);
            if (userId == null)
                return Results.Unauthorized();

            var success = await userService.UpdatePrivacySettingsAsync(userId.Value, privacy, cancellationToken);
            return success 
                ? Results.Ok() 
                : Results.NotFound();
        })
        .WithName("UpdatePrivacySettings")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        // Check username availability
        group.MapGet("/check-username", async (
            string username,
            IUserService userService,
            CancellationToken cancellationToken) =>
        {
            var isAvailable = await userService.IsUsernameAvailableAsync(username, null, cancellationToken);
            return Results.Ok(new { available = isAvailable });
        })
        .WithName("CheckUsernameAvailability")
        .Produces<object>(StatusCodes.Status200OK);

        return app;
    }

    private static Guid? GetCurrentUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value 
            ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
            return null;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
