using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LexiQuest.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddLexiQuestClientServices(builder.Configuration);

await builder.Build().RunAsync();
