using LexiQuest.Shared.DTOs;

namespace LexiQuest.Api.Endpoints;

/// <summary>
/// Endpoints for receiving client-side error reports.
/// </summary>
public static class ClientErrorEndpoints
{
    public static IEndpointRouteBuilder MapClientErrorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("ClientErrors");

        group.MapPost("/client-errors", (
            ClientErrorDto dto,
            ILogger<Program> logger) =>
        {
            logger.LogWarning(
                "Client error: {Message} | Component: {ComponentName} | User: {UserId} | Url: {Url} | Timestamp: {Timestamp}",
                dto.Message,
                dto.ComponentName,
                dto.UserId,
                dto.Url,
                dto.Timestamp);

            if (!string.IsNullOrEmpty(dto.StackTrace))
            {
                logger.LogWarning("Client stack trace: {StackTrace}", dto.StackTrace);
            }

            return Results.Ok();
        })
        .WithName("ReportClientError")
        .WithSummary("Report a client-side error for server-side logging")
        .Accepts<ClientErrorDto>("application/json")
        .Produces(StatusCodes.Status200OK)
        .AllowAnonymous();

        return app;
    }
}
