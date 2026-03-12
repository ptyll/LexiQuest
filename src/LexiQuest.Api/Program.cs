using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using LexiQuest.Api.Endpoints;
using LexiQuest.Api.Endpoints.Users;
using LexiQuest.Api.Middleware;
using LexiQuest.Api.Validators;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Core.Services.BossRules;
using LexiQuest.Core.Validators;
using LexiQuest.Infrastructure.Services;
using Microsoft.Extensions.Localization;
using LexiQuest.Infrastructure.Auth;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Infrastructure.Persistence.Repositories;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Api.HealthChecks;
using LexiQuest.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Extensions.Hosting;

namespace LexiQuest.Api;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            
            ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

            var app = builder.Build();
            
            ConfigureMiddleware(app, app.Environment);

            Log.Information("LexiQuest API starting...");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Serilog
        services.AddSingleton<DiagnosticContext>(new DiagnosticContext(Log.Logger));
        services.AddHttpContextAccessor();
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/lexiquest-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger(), dispose: true);
        });

        // Database - skip registration in test environment (will be configured by test)
        if (!environment.EnvironmentName.Equals("Test", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<LexiQuestDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));
        }

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWordRepository, WordRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<ILeagueRepository, LeagueRepository>();
        services.AddScoped<IDailyChallengeRepository, DailyChallengeRepository>();
        services.AddScoped<IAchievementRepository, AchievementRepository>();
        services.AddScoped<IUserAchievementRepository, UserAchievementRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IStreakProtectionRepository, StreakProtectionRepository>();
        services.AddScoped<IShopItemRepository, ShopItemRepository>();
        services.AddScoped<IUserInventoryRepository, UserInventoryRepository>();
        services.AddScoped<ICustomDictionaryRepository, CustomDictionaryRepository>();
        services.AddScoped<IDictionaryWordRepository, DictionaryWordRepository>();
        services.AddScoped<IMatchResultRepository, MatchResultRepository>();
        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IAdminRoleAssignmentRepository, AdminRoleAssignmentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IGameSessionService, GameSessionService>();
        services.AddScoped<IXpCalculator, XpCalculator>();
        services.AddScoped<IXpService, Core.Services.XpService>();
        services.AddScoped<ILevelCalculator, LevelCalculator>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<ILeagueService, LeagueService>();
        services.AddScoped<IDailyChallengeService, DailyChallengeService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<LexiQuest.Infrastructure.Services.StripeSubscriptionService>();
        services.AddScoped<IPremiumFeatureService, PremiumFeatureService>();
        services.AddScoped<IStreakProtectionService, StreakProtectionService>();
        services.AddScoped<IStreakService, StreakService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IDictionaryService, DictionaryService>();
        services.AddScoped<ILivesService, LivesService>();
        services.AddScoped<IPathService, Infrastructure.Services.PathService>();
        services.AddScoped<ICoinService, CoinService>();
        services.AddScoped<IAIChallengeService, AIChallengeService>();
        services.AddScoped<MarathonBossRules>();
        services.AddScoped<ConditionBossRules>();
        services.AddScoped<TwistBossRules>();
        services.AddScoped<IMatchmakingService, MatchmakingService>();
        services.AddScoped<IMultiplayerGameService, MultiplayerGameService>();
        services.AddScoped<IMatchHistoryService, MatchHistoryService>();
        services.AddSingleton<IRoomService, RoomService>();
        services.AddSingleton<ILobbyChatService, LobbyChatService>();
        services.AddScoped<IBossService>(sp =>
        {
            var marathonRules = sp.GetRequiredService<MarathonBossRules>();
            var conditionRules = sp.GetRequiredService<ConditionBossRules>();
            var twistRules = sp.GetRequiredService<TwistBossRules>();
            var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
            var localizer = sp.GetRequiredService<IStringLocalizer<BossService>>();
            return new BossService(marathonRules, conditionRules, twistRules, unitOfWork, localizer);
        });

        // Notification Services
        services.AddHttpClient("WebPush");
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPushService, WebPushService>();
        services.AddScoped<StreakReminderJob>();
        services.AddScoped<DailyChallengeReminderJob>();
        services.AddScoped<InactiveReminderJob>();

        // Admin Services
        services.AddScoped<IAdminAuthorizationService, AdminAuthorizationService>();
        services.AddScoped<IAdminWordService, AdminWordService>();
        services.AddScoped<IAdminUserService, AdminUserService>();

        // Guest Mode Services
        services.AddScoped<IGuestSessionService, GuestSessionService>();
        services.AddSingleton<IGuestLimiter, GuestLimiter>();

        // Password Hasher
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateProfileValidator>();

        // JWT Settings
        var jwtSettings = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSettings);
        services.AddScoped<ITokenService, TokenService>();

        // Stripe Settings
        services.Configure<StripeSettings>(configuration.GetSection("StripeSettings"));
        services.AddSingleton(sp =>
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StripeSettings>>().Value);

        // VAPID Settings for Web Push
        services.Configure<VapidSettings>(configuration.GetSection("VapidSettings"));

        // Authentication
        var secretKey = jwtSettings.GetValue<string>("SecretKey");
        
        // In test environment, skip JWT configuration if not provided
        if (string.IsNullOrEmpty(secretKey) && !environment.EnvironmentName.Equals("Test", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured.");
        }
        
        // Only configure authentication if we have a secret key
        if (!string.IsNullOrEmpty(secretKey))
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.GetValue<string>("Audience"),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization();
        }

        // Localization
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // CORS - Configured for SignalR WebSockets
        services.AddCors(options =>
        {
            options.AddPolicy("BlazorClient", policy =>
            {
                policy.WithOrigins(
                        configuration.GetValue<string>("BlazorClient:Url") ?? "https://localhost:5001")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // Controllers
        services.AddControllers();

        // Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Memory Cache
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, LexiQuest.Infrastructure.Caching.MemoryCacheService>();

        // Health Checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });

        // SignalR
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = environment.IsDevelopment();
            options.MaximumReceiveMessageSize = 1024 * 64; // 64KB max message size
        });
    }

    public static void ConfigureMiddleware(WebApplication app, IWebHostEnvironment environment)
    {
        // Configure the HTTP request pipeline
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LexiQuest API v1"));
        }

        // Security headers must be first in the pipeline
        app.UseSecurityHeaders();

        app.UseHttpsRedirection();
        app.UseCors("BlazorClient");
        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapGameEndpoints();
        app.MapUserEndpoints();
        app.MapGuestEndpoints();

        // Health check endpoints
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // SignalR Hubs
        app.MapHub<Hubs.MatchHub>("/hubs/match");
    }
}
