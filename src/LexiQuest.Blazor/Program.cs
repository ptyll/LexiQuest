using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tempo.Blazor.Configuration;
using Tempo.Blazor.FluentValidation;
using Tempo.Blazor.Services;
using LexiQuest.Blazor;
using LexiQuest.Blazor.Services;
using LexiQuest.Blazor.Validators;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Tempo.Blazor components
builder.Services.AddTempoBlazor();

// Tempo.Blazor FluentValidation (scan Shared and Blazor assemblies for validators)
builder.Services.AddTempoFluentValidation(
    typeof(LexiQuest.Shared.AssemblyMarker).Assembly,
    typeof(Program).Assembly);

// Auth Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// Game Services
builder.Services.AddScoped<ILeagueService, LeagueService>();
builder.Services.AddScoped<IDailyChallengeService, DailyChallengeService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IGuestGameService, GuestGameService>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();
builder.Services.AddScoped<IAIChallengeClient, AIChallengeClient>();
builder.Services.AddScoped<IPremiumService, PremiumService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// SignalR Multiplayer Service
builder.Services.AddScoped<IMatchHubClient, MatchHubClient>();

// Match History Service
builder.Services.AddScoped<IMatchHistoryClient, MatchHistoryClient>();

// Admin Service
builder.Services.AddScoped<IAdminService, AdminService>();

// Error Logging Service
builder.Services.AddScoped<IErrorLoggingService, ErrorLoggingService>();

// HttpClient Factory with Authorization Handler
builder.Services.AddTransient<AuthorizationMessageHandler>();
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:5000");
}).AddHttpMessageHandler<AuthorizationMessageHandler>();

// Theme and Toast services
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<ToastService>();

// HttpClient configured to point to API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:5000")
});

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Set Czech culture
var culture = new CultureInfo("cs");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await builder.Build().RunAsync();
