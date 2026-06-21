using LexiQuest.Blazor;
using LexiQuest.Web.Components;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("E2E"))
{
    StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
}

// Blazor Interactive Auto
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Client-side services (pro server-side rendering)
builder.Services.AddLexiQuestClientServices(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

if (app.Environment.IsEnvironment("E2E"))
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/appsettings.json" ||
            context.Request.Path == "/appsettings.E2E.json")
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                ApiBaseUrl = app.Configuration["ApiBaseUrl"]
            });
            return;
        }

        await next();
    });
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(LexiQuest.Blazor.ServiceCollectionExtensions).Assembly);

app.Run();
