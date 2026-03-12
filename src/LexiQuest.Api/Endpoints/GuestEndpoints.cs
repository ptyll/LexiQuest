using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Endpoints;

/// <summary>
/// Endpoints for guest game mode - no authentication required.
/// Limited to 5 games per 24h per IP address.
/// </summary>
public static class GuestEndpoints
{
    public static IEndpointRouteBuilder MapGuestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("Guest");

        // POST /api/v1/game/guest/start - Start a new guest game
        group.MapPost("/game/guest/start", async (
            IGuestSessionService guestSessionService,
            IGuestLimiter guestLimiter,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var ipAddress = GetClientIpAddress(httpContext);

            // Check rate limit
            var limitResult = guestLimiter.CanStartGame(ipAddress);
            if (!limitResult.Allowed)
            {
                return Results.Problem(
                    title: "Rate Limit Exceeded",
                    detail: limitResult.Message,
                    statusCode: StatusCodes.Status429TooManyRequests,
                    extensions: new Dictionary<string, object?>
                    {
                        ["resetTime"] = limitResult.ResetTime,
                        ["remainingGames"] = 0
                    }
                );
            }

            // Record the game start
            guestLimiter.RecordGame(ipAddress);

            // Start the game session
            var session = guestSessionService.StartGame();

            var response = new GuestStartResponse(
                SessionId: session.SessionId,
                ScrambledWords: session.ScrambledWords.Select(w => new GuestScrambledWordDto(
                    w.WordId,
                    w.Scrambled,
                    w.Length
                )).ToList(),
                RemainingGames: limitResult.RemainingGames,
                Message: $"Zbývající hry dnes: {limitResult.RemainingGames}"
            );

            return Results.Ok(response);
        })
        .WithName("GuestStartGame")
        .WithSummary("Start a new guest game session")
        .WithDescription("Creates a new anonymous game session with 5 beginner words. Limited to 5 games per 24h per IP.")
        .Produces<GuestStartResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status429TooManyRequests);

        // POST /api/v1/game/guest/answer - Submit an answer
        group.MapPost("/game/guest/answer", (
            GuestAnswerRequest request,
            IGuestSessionService guestSessionService) =>
        {
            try
            {
                var result = guestSessionService.SubmitAnswer(
                    request.SessionId,
                    request.WordId,
                    request.Answer
                );

                var response = new GuestAnswerResponse(
                    IsCorrect: result.IsCorrect,
                    XpEarned: result.XpEarned,
                    CorrectAnswer: result.CorrectAnswer,
                    UserAnswer: result.UserAnswer,
                    TotalSessionXp: result.TotalSessionXp,
                    WordsSolved: result.WordsSolved,
                    WordsRemaining: result.WordsRemaining,
                    IsGameComplete: result.WordsRemaining == 0
                );

                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    title: "Session Error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
        })
        .WithName("GuestSubmitAnswer")
        .WithSummary("Submit an answer for a word")
        .WithDescription("Checks the answer and returns XP earned if correct.")
        .Produces<GuestAnswerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // GET /api/v1/guest/status - Get guest game status
        group.MapGet("/guest/status", (
            IGuestLimiter guestLimiter,
            HttpContext httpContext) =>
        {
            var ipAddress = GetClientIpAddress(httpContext);
            var status = guestLimiter.GetStatus(ipAddress);

            var response = new GuestStatusResponse(
                TotalAllowed: status.TotalAllowed,
                Used: status.Used,
                Remaining: status.Remaining,
                ResetTime: status.ResetTime
            );

            return Results.Ok(response);
        })
        .WithName("GuestStatus")
        .WithSummary("Get guest game status")
        .WithDescription("Returns the number of games played and remaining for the current IP.")
        .Produces<GuestStatusResponse>(StatusCodes.Status200OK);

        // POST /api/v1/game/guest/convert - Convert guest to registered user
        group.MapPost("/game/guest/convert", async (
            GuestConvertRequest request,
            IGuestSessionService guestSessionService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Get the final progress before ending the session
                var progress = guestSessionService.GetSessionProgress(request.SessionId);

                // End the guest session
                var session = guestSessionService.EndGame(request.SessionId);

                // Note: Actual user registration would be handled by IUserService
                // This endpoint returns the progress that should be transferred

                var response = new GuestConvertResponse(
                    Success: true,
                    UserId: null, // Would be set after actual registration
                    Message: "Guest progress ready for transfer. Complete registration to save.",
                    TransferredXp: request.TransferProgress ? progress.TotalXp : 0,
                    TransferredWordsSolved: request.TransferProgress ? progress.WordsSolved : 0
                );

                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    title: "Session Error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
        })
        .WithName("GuestConvert")
        .WithSummary("Convert guest session to registered account")
        .WithDescription("Ends the guest session and returns progress that can be transferred to a new account.")
        .Produces<GuestConvertResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    /// <summary>
    /// Gets the client IP address from the HTTP context.
    /// </summary>
    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded header (when behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP if multiple are present
            return forwardedFor.Split(',')[0].Trim();
        }

        // Fall back to connection remote IP
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            // Return IPv4 mapped to IPv6 as IPv4
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                return remoteIp.MapToIPv4().ToString();
            }
            return remoteIp.ToString();
        }

        // Final fallback
        return "unknown";
    }
}
