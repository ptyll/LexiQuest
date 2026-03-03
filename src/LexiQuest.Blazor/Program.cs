using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tempo.Blazor.Configuration;
using Tempo.Blazor.FluentValidation;
using Tempo.Blazor.Services;
using LexiQuest.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Tempo.Blazor components
builder.Services.AddTempoBlazor();

// Tempo.Blazor FluentValidation (scan Shared assembly for validators)
builder.Services.AddTempoFluentValidation(typeof(LexiQuest.Shared.AssemblyMarker).Assembly);

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
