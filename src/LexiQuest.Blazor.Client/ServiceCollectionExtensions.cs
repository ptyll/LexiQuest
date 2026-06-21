using System.Globalization;
using LexiQuest.Blazor.Services;
using LexiQuest.Blazor.Validators;
using Tempo.Blazor.Configuration;
using Tempo.Blazor.FluentValidation;
using Tempo.Blazor.Services;

namespace LexiQuest.Blazor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLexiQuestClientServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Tempo.Blazor components
        services.AddTempoBlazor();

        // Tempo.Blazor FluentValidation (scan Shared and Blazor assemblies for validators)
        services.AddTempoFluentValidation(
            typeof(LexiQuest.Shared.AssemblyMarker).Assembly,
            typeof(ServiceCollectionExtensions).Assembly);

        // Auth Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        // Game Services
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ILeagueService, LeagueService>();
        services.AddScoped<IDailyChallengeService, DailyChallengeService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IBossService, BossService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IStreakProtectionClient, StreakProtectionClient>();
        services.AddScoped<IGuestGameService, GuestGameService>();
        services.AddScoped<IPathService, PathService>();
        services.AddScoped<IDictionaryService, DictionaryService>();
        services.AddScoped<IAIChallengeClient, AIChallengeClient>();
        services.AddScoped<IPremiumService, PremiumService>();
        services.AddScoped<IShopService, ShopService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<NotificationRefreshService>();

        // SignalR Multiplayer Service
        services.AddScoped<IMatchHubClient, MatchHubClient>();

        // Match History Service
        services.AddScoped<IMatchHistoryClient, MatchHistoryClient>();

        // Admin Service
        services.AddScoped<IAdminService, AdminService>();

        // Error Logging Service
        services.AddScoped<IErrorLoggingService, ErrorLoggingService>();

        // HttpClient Factory with Authorization Handler
        services.AddTransient<AuthorizationMessageHandler>();
        services.AddHttpClient("PublicApiClient", client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:5000");
        });

        services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:5000");
        }).AddHttpMessageHandler<AuthorizationMessageHandler>();

        services.AddHttpClient("LexiQuestApi", client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:5000");
        }).AddHttpMessageHandler<AuthorizationMessageHandler>();

        // Theme and Toast services
        services.AddScoped<ThemeService>();
        services.AddScoped<ToastService>();
        services.AddScoped<IToastService, TempoToastServiceAdapter>();

        // HttpClient configured to point to API
        services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:5000")
        });

        // Localization
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Set Czech culture
        var culture = new CultureInfo("cs");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        return services;
    }
}
