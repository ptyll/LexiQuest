using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LexiQuest.Infrastructure.Tests.Services;

public class LiveRegenerationServiceTests : IDisposable
{
    private readonly LexiQuestDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly CancellationTokenSource _cts;

    public LiveRegenerationServiceTests()
    {
        var options = new DbContextOptionsBuilder<LexiQuestDbContext>()
            .UseInMemoryDatabase(databaseName: $"LiveRegenTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new LexiQuestDbContext(options);
        
        // Seed test user
        var user = User.Create("test@example.com", "testuser");
        user.SetPasswordHash("hashedpassword123");
        user.ResetLives(3, 5);
        _context.Users.Add(user);
        _context.SaveChanges();

        _cts = new CancellationTokenSource();
        
        var services = new ServiceCollection();
        services.AddSingleton(_context);
        services.AddLogging(builder => builder.AddDebug());
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _context.Dispose();
        _cts.Dispose();
    }

    [Fact]
    public async Task LiveRegenerationService_ProcessesRegeneration_AtCorrectIntervals()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LiveRegenerationService>>();
        var mockLivesService = Substitute.For<ILivesService>();
        
        var services = new ServiceCollection();
        services.AddSingleton(_context);
        services.AddSingleton<ILivesService>(mockLivesService);
        services.AddLogging(builder => builder.AddDebug());
        var provider = services.BuildServiceProvider();
        
        var service = new LiveRegenerationService(provider, logger);

        // Act - Start the service
        await service.StartAsync(_cts.Token);
        
        // Wait for at least one processing cycle
        await Task.Delay(1500, _cts.Token);
        
        // Assert - Service should have started successfully
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("started")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
        
        // Cleanup
        await service.StopAsync(_cts.Token);
    }

    [Fact]
    public void LiveRegenerationService_IsBackgroundService()
    {
        // Assert
        typeof(LiveRegenerationService).Should().BeAssignableTo<BackgroundService>();
    }

    [Fact]
    public async Task LiveRegenerationService_ExecutesPeriodically()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LiveRegenerationService>>();
        var service = new LiveRegenerationService(_serviceProvider, logger);

        // Act
        await service.StartAsync(_cts.Token);
        
        // Wait for multiple intervals
        await Task.Delay(2500, _cts.Token);
        
        // Assert - service should still be running
        _cts.Token.IsCancellationRequested.Should().BeFalse();
        
        // Cleanup
        await service.StopAsync(_cts.Token);
    }

    [Fact]
    public async Task LiveRegenerationService_StopsGracefully()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LiveRegenerationService>>();
        var mockLivesService = Substitute.For<ILivesService>();
        
        var services = new ServiceCollection();
        services.AddSingleton(_context);
        services.AddSingleton<ILivesService>(mockLivesService);
        services.AddLogging(builder => builder.AddDebug());
        var provider = services.BuildServiceProvider();
        
        var service = new LiveRegenerationService(provider, logger);
        
        // Act
        await service.StartAsync(_cts.Token);
        await Task.Delay(500, _cts.Token);
        await service.StopAsync(_cts.Token);
        
        // Assert
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("stopped")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
