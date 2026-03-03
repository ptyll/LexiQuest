using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Endpoints;

/// <summary>
/// Game API endpoints.
/// </summary>
public static class GameEndpoints
{
    /// <summary>
    /// Maps game endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/game")
            .WithTags("Game")
            .RequireAuthorization();

        // POST /api/v1/game/start - Start a new game
        group.MapPost("/start", async (
            StartGameRequest request,
            IGameSessionService gameService,
            IHttpContextAccessor httpContextAccessor,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(httpContextAccessor);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            try
            {
                var result = await gameService.StartGameAsync(userId.Value, request, cancellationToken);
                return Results.Created($"/api/v1/game/{result.SessionId}", result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("StartGame")
        .WithSummary("Start a new game session")
        .WithDescription("Creates a new game session and returns the first scrambled word")
        .Produces<ScrambledWordDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // POST /api/v1/game/{id}/answer - Submit an answer
        group.MapPost("/{id:guid}/answer", async (
            Guid id,
            SubmitAnswerRequest request,
            IGameSessionService gameService,
            IHttpContextAccessor httpContextAccessor,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(httpContextAccessor);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // Ensure session ID matches URL
            if (request.SessionId != id)
            {
                return Results.BadRequest(new { error = "Session ID mismatch" });
            }

            try
            {
                var result = await gameService.SubmitAnswerAsync(userId.Value, request, cancellationToken);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("SubmitAnswer")
        .WithSummary("Submit an answer for the current round")
        .WithDescription("Validates the answer and returns the result with XP earned")
        .Produces<GameRoundResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        // GET /api/v1/game/{id} - Get game state
        group.MapGet("/{id:guid}", async (
            Guid id,
            IGameSessionService gameService,
            CancellationToken cancellationToken) =>
        {
            var result = await gameService.GetSessionStateAsync(id, cancellationToken);
            if (result == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(result);
        })
        .WithName("GetGameState")
        .WithSummary("Get current game state")
        .WithDescription("Returns the current scrambled word and game state")
        .Produces<ScrambledWordDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);

        // POST /api/v1/game/{id}/forfeit - Forfeit game
        group.MapPost("/{id:guid}/forfeit", async (
            Guid id,
            IGameSessionService gameService,
            IHttpContextAccessor httpContextAccessor,
            CancellationToken cancellationToken) =>
        {
            var userId = GetUserId(httpContextAccessor);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var result = await gameService.ForfeitGameAsync(userId.Value, id, cancellationToken);
            if (!result)
            {
                return Results.BadRequest(new { error = "Cannot forfeit this game" });
            }

            return Results.NoContent();
        })
        .WithName("ForfeitGame")
        .WithSummary("Forfeit the current game")
        .WithDescription("Ends the game session without completing it")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static Guid? GetUserId(IHttpContextAccessor httpContextAccessor)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
            ?? httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
