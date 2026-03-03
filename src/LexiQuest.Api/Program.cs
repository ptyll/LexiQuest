using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using LexiQuest.Api.Endpoints;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Core.Validators;
using LexiQuest.Infrastructure.Services;
using LexiQuest.Infrastructure.Auth;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Infrastructure.Persistence.Repositories;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IGameSessionService, GameSessionService>();
        services.AddScoped<IXpCalculator, XpCalculator>();

        // Password Hasher
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        // JWT Settings
        var jwtSettings = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSettings);
        services.AddScoped<ITokenService, TokenService>();

        // Authentication
        var secretKey = jwtSettings.GetValue<string>("SecretKey")
                        ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
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

        // Localization
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // CORS
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
        services.AddHealthChecks();
    }

    public static void ConfigureMiddleware(WebApplication app, IWebHostEnvironment environment)
    {
        // Configure the HTTP request pipeline
        if (environment.IsDevelopment())
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
        app.MapGameEndpoints();
        app.MapHealthChecks("/health");
    }
}
