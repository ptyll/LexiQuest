using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using LexiQuest.Api.Endpoints;
using LexiQuest.Api.Endpoints.Users;
using LexiQuest.Api.Hubs;
using LexiQuest.Api.Middleware;
using LexiQuest.Api.Testing;
using LexiQuest.Api.Validators;
using LexiQuest.Core.Configuration;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Jobs;
using LexiQuest.Core.Services;
using LexiQuest.Core.Services.BossRules;
using LexiQuest.Core.Validators;
using LexiQuest.Infrastructure.Services;
using Microsoft.Extensions.Localization;
using LexiQuest.Infrastructure.Auth;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Infrastructure.Persistence.Repositories;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Notifications;
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
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            
            ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

            var app = builder.Build();

            await EnsureDevelopmentDatabaseAsync(app, app.Environment);
            
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

    private static async Task EnsureDevelopmentDatabaseAsync(WebApplication app, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment() && !environment.IsEnvironment("E2E"))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();

        await dbContext.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(dbContext);
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
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
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
        if (environment.IsEnvironment("E2E"))
        {
            services.AddSingleton<E2EXpRuntimeSettings>();
            services.AddSingleton<E2EStatsRuntimeSettings>();
            services.AddSingleton<E2EHttpDelayRuntimeSettings>();
            services.AddScoped<IXpCalculator, E2EXpCalculator>();
        }
        else
        {
            services.AddScoped<IXpCalculator, XpCalculator>();
        }
        services.AddScoped<IXpService, Core.Services.XpService>();
        services.AddScoped<ILevelCalculator, LevelCalculator>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.Configure<PremiumAccessOptions>(configuration.GetSection(PremiumAccessOptions.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, LexiQuest.Infrastructure.Services.EmailService>();
        services.AddScoped<ILeagueService, LeagueService>();
        services.AddScoped<IDailyChallengeService, DailyChallengeService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<StripeSubscriptionService>();
        services.AddScoped<ISubscriptionService>(sp => sp.GetRequiredService<StripeSubscriptionService>());
        services.AddScoped<IPremiumFeatureService, PremiumFeatureService>();
        services.AddScoped<IStreakProtectionService, StreakProtectionService>();
        services.AddScoped<IStreakService, StreakService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IDictionaryService, DictionaryService>();
        services.AddScoped<ILivesService, LivesService>();
        services.AddScoped<IPathService, Infrastructure.Services.PathService>();
        services.AddScoped<ICoinService, CoinService>();
        services.AddScoped<IAIChallengeService, AIChallengeService>();
        services.AddScoped<IBossGameService, BossGameService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<MarathonBossRules>();
        services.AddScoped<ConditionBossRules>();
        services.AddScoped<TwistBossRules>();
        services.AddSingleton<IMatchmakingService, MatchmakingService>();
        services.AddScoped<IMultiplayerGameService, MultiplayerGameService>();
        services.AddScoped<IMatchHistoryService, MatchHistoryService>();
        services.AddSingleton<IRoomService, RoomService>();
        services.AddSingleton<ILobbyChatService, LobbyChatService>();
        services.AddSingleton<MultiplayerRuntimeSettings>();
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
        services.AddScoped<PremiumExpiryReminderJob>();
        services.AddScoped<InactiveReminderJob>();
        services.AddScoped<LeagueResetJob>();
        services.AddScoped<RoomCleanupJob>();

        // Admin Services
        services.AddScoped<IAdminAuthorizationService, AdminAuthorizationService>();
        services.AddScoped<IAdminWordService, AdminWordService>();
        services.AddScoped<IAdminUserService, AdminUserService>();

        // Guest Mode Services
        services.AddScoped<IGuestSessionService, GuestSessionService>();
        services.AddSingleton<IGuestLimiter, GuestLimiter>();
        services.AddSingleton<IGuestProgressTransferService, GuestProgressTransferService>();

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
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"].ToString();
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)
                                          ?? context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub);

                        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                        {
                            return;
                        }

                        if (context.Principal?.Identity is not ClaimsIdentity identity)
                        {
                            return;
                        }

                        var roleRepository = context.HttpContext.RequestServices.GetRequiredService<IAdminRoleAssignmentRepository>();
                        var assignments = await roleRepository.GetByUserIdAsync(userId, context.HttpContext.RequestAborted);

                        foreach (var role in assignments.Select(a => a.Role.ToString()).Distinct())
                        {
                            if (!identity.HasClaim(ClaimTypes.Role, role))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }
                        }
                    }
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
                        configuration.GetValue<string>("BlazorClient:Url") ?? "https://localhost:7300")
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

        if (environment.IsEnvironment("E2E"))
        {
            services.AddSingleton<AdjustableTimeProvider>();
            services.AddSingleton<TimeProvider>(sp => sp.GetRequiredService<AdjustableTimeProvider>());
        }
        else
        {
            services.AddSingleton(TimeProvider.System);
        }

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

        // HSTS in production
        if (!environment.IsDevelopment())
        {
            app.UseHsts();
        }

        // Security headers must be first in the pipeline
        app.UseSecurityHeaders();

        app.UseHttpsRedirection();
        app.UseCors("BlazorClient");
        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseAuthorization();

        if (environment.IsEnvironment("E2E"))
        {
            app.Use(async (context, next) =>
            {
                var delaySettings = context.RequestServices.GetRequiredService<E2EHttpDelayRuntimeSettings>();
                var delay = delaySettings.ConsumeDelayForPath(context.Request.Path);
                if (delay is not null)
                {
                    await delay.WaitAsync(context.RequestAborted);
                }

                await next();
            });
        }

        app.MapControllers();
        app.MapGameEndpoints();
        app.MapUserEndpoints();
        app.MapGuestEndpoints();
        app.MapClientErrorEndpoints();

        if (environment.IsEnvironment("E2E"))
        {
            var e2e = app.MapGroup("/api/v1/e2e")
                .WithTags("E2E");

            e2e.MapPost("/time/advance", (E2ETimeAdvanceRequest request, AdjustableTimeProvider timeProvider) =>
            {
                timeProvider.Advance(TimeSpan.FromSeconds(request.Seconds));
                return Results.Ok(new { UtcNow = timeProvider.GetUtcNow() });
            });

            e2e.MapPost("/state/reset", (
                AdjustableTimeProvider timeProvider,
                IGuestLimiter guestLimiter,
                MultiplayerRuntimeSettings multiplayerSettings,
                E2EXpRuntimeSettings xpSettings,
                E2EStatsRuntimeSettings statsSettings,
                E2EHttpDelayRuntimeSettings httpDelaySettings) =>
            {
                timeProvider.Reset();
                multiplayerSettings.Reset();
                xpSettings.Reset();
                statsSettings.Reset();
                httpDelaySettings.Reset();
                guestLimiter.Reset("127.0.0.1");
                guestLimiter.Reset("::1");
                guestLimiter.Reset("unknown");

                return Results.Ok();
            });

            e2e.MapPost("/multiplayer/quick-match-time-limit", (
                E2EQuickMatchTimeLimitRequest request,
                MultiplayerRuntimeSettings multiplayerSettings) =>
            {
                multiplayerSettings.SetQuickMatchTimeLimitSeconds(request.Seconds);
                return Results.Ok(new { Seconds = multiplayerSettings.QuickMatchTimeLimitSeconds });
            });

            e2e.MapPost("/xp/fixed-correct-answer", (
                E2EFixedCorrectAnswerXpRequest request,
                E2EXpRuntimeSettings xpSettings) =>
            {
                xpSettings.SetFixedCorrectAnswerXp(request.Amount);
                return Results.Ok(new { Amount = xpSettings.FixedCorrectAnswerXp });
            });

            e2e.MapPost("/stats/fail-next-user-request", (E2EStatsRuntimeSettings statsSettings) =>
            {
                statsSettings.FailNextUserStatsRequest();
                return Results.Ok();
            });

            e2e.MapPost("/stats/delay-next-user-request", (E2EStatsRuntimeSettings statsSettings) =>
            {
                statsSettings.DelayNextUserStatsRequest();
                return Results.Ok();
            });

            e2e.MapPost("/stats/release-user-request", (E2EStatsRuntimeSettings statsSettings) =>
            {
                statsSettings.ReleaseUserStatsRequest();
                return Results.Ok();
            });

            e2e.MapPost("/http/delay-next", (
                E2EHttpDelayRequest request,
                E2EHttpDelayRuntimeSettings delaySettings) =>
            {
                delaySettings.DelayNextRequest(request.Path);
                return Results.Ok(new { request.Path });
            });

            e2e.MapPost("/http/release", (E2EHttpDelayRuntimeSettings delaySettings) =>
            {
                delaySettings.ReleaseAll();
                return Results.Ok();
            });

            e2e.MapPost("/multiplayer/expire-room", async (
                E2EExpireRoomRequest request,
                IRoomService roomService,
                CancellationToken cancellationToken) =>
            {
                var room = await roomService.GetRoomAsync(request.RoomCode, cancellationToken);
                if (room == null)
                {
                    return Results.NotFound();
                }

                room.Expire();
                return Results.Ok(new { RoomCode = room.Code, room.Status, room.ExpiresAt });
            });

            e2e.MapPost("/multiplayer/cleanup-rooms", async (
                RoomCleanupJob job,
                CancellationToken cancellationToken) =>
            {
                await job.ExecuteAsync(cancellationToken);
                return Results.Ok();
            });

            e2e.MapPost("/leagues/reset", async (LeagueResetJob job, CancellationToken cancellationToken) =>
            {
                await job.ExecuteAsync(cancellationToken);
                return Results.Ok();
            });

            e2e.MapPost("/notifications/send", async (
                E2ESendNotificationRequest request,
                IUserRepository userRepository,
                INotificationService notificationService,
                CancellationToken cancellationToken) =>
            {
                var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (user == null)
                {
                    return Results.NotFound();
                }

                await notificationService.SendAsync(new SendNotificationRequest(
                    user.Id,
                    request.Type,
                    request.Title,
                    request.Message,
                    request.Severity,
                    request.ActionUrl), cancellationToken);

                return Results.Ok();
            });

            e2e.MapPost("/notifications/run-streak-reminders", async (
                StreakReminderJob job,
                CancellationToken cancellationToken) =>
            {
                await job.ExecuteAsync(cancellationToken);
                return Results.Ok();
            });

            e2e.MapPost("/notifications/run-daily-reminders", async (
                DailyChallengeReminderJob job,
                CancellationToken cancellationToken) =>
            {
                await job.ExecuteAsync(cancellationToken);
                return Results.Ok();
            });

            e2e.MapPost("/premium/run-expiry-reminders", async (
                PremiumExpiryReminderJob job,
                CancellationToken cancellationToken) =>
            {
                await job.ExecuteAsync(cancellationToken);
                return Results.Ok();
            });
        }

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
