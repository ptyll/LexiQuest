using Microsoft.Extensions.Diagnostics.HealthChecks;
using LexiQuest.Infrastructure.Persistence;

namespace LexiQuest.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly LexiQuestDbContext _dbContext;

    public DatabaseHealthCheck(LexiQuestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database connection is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed.", ex);
        }
    }
}
