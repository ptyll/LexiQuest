using LexiQuest.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LexiQuest.Infrastructure.Services;

/// <summary>
/// Background service that periodically checks and regenerates player lives.
/// Advanced path: regeneration every 30 minutes
/// Expert path: regeneration every 60 minutes
/// </summary>
public class LiveRegenerationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LiveRegenerationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

    public LiveRegenerationService(IServiceProvider serviceProvider, ILogger<LiveRegenerationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LiveRegenerationService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRegenerationAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during life regeneration processing");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("LiveRegenerationService stopped");
    }

    private async Task ProcessRegenerationAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var livesService = scope.ServiceProvider.GetService<ILivesService>();
            
            if (livesService == null)
            {
                _logger.LogWarning("ILivesService not registered, skipping regeneration cycle");
                return;
            }

            _logger.LogDebug("Processing life regeneration cycle");
            
            // In production, this would iterate through active users
            // For MVP, regeneration happens on-demand when user checks their status
            // The LivesService handles the logic of checking if regeneration is needed
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessRegenerationAsync");
            throw;
        }
    }
}
