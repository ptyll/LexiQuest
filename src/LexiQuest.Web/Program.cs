using LexiQuest.Blazor;
using LexiQuest.Web.Components;

var builder = WebApplication.CreateBuilder(args);

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
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(LexiQuest.Blazor.ServiceCollectionExtensions).Assembly);

app.Run();
