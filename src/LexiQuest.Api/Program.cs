using System.Text;
using LexiQuest.Core.Interfaces;
using LexiQuest.Infrastructure.Auth;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/lexiquest-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7));

    // Database
    builder.Services.AddDbContext<LexiQuestDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

    // Unit of Work
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // JWT Settings
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    builder.Services.Configure<JwtSettings>(jwtSettings);
    builder.Services.AddScoped<ITokenService, TokenService>();

    // Authentication
    var secretKey = jwtSettings.GetValue<string>("SecretKey")
                    ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
    builder.Services.AddAuthentication(options =>
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

    builder.Services.AddAuthorization();

    // Localization
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("BlazorClient", policy =>
        {
            policy.WithOrigins(
                    builder.Configuration.GetValue<string>("BlazorClient:Url") ?? "https://localhost:5001")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // Controllers
    builder.Services.AddControllers();

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Memory Cache
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, LexiQuest.Infrastructure.Caching.MemoryCacheService>();

    // Health Checks
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LexiQuest API v1"));
    }

    app.UseHttpsRedirection();
    app.UseCors("BlazorClient");
    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("LexiQuest API starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
