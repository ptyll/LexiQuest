using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.E2E.Tests.Infrastructure;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.DTOs.Stats;
using LexiQuest.Shared.DTOs.Teams;
using LexiQuest.Shared.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Playwright;
using Respawn;
using Respawn.Graph;
using Testcontainers.MsSql;

namespace LexiQuest.E2E.Tests;

public sealed class E2EEnvironmentFixture : IAsyncLifetime
{
    private const string JwtSecret = "LexiQuest-E2E-Secret-Key-That-Is-Long-Enough-For-HS256-Algorithm-!!";
    private const string DatabasePassword = "LexiQuest-E2E-Strong-Passw0rd!";

    private readonly SemaphoreSlim _databaseLock = new(1, 1);
    private readonly ConcurrentDictionary<IPage, PageDiagnostics> _pageDiagnostics = new();
    private readonly bool _keepContainers = Environment.GetEnvironmentVariable("E2E_KEEP_CONTAINERS") is "true" or "1" or "on";
    private Respawner? _respawner;
    private MsSqlContainer? _database;
    private IContainer? _smtp4Dev;
    private AppProcessRunner? _api;
    private AppProcessRunner? _web;

    public IPlaywright Playwright { get; private set; } = null!;

    public IBrowser Browser { get; private set; } = null!;

    public string BaseUrl => WebBaseUrl;

    public string WebBaseUrl { get; private set; } = null!;

    public string ApiBaseUrl { get; private set; } = null!;

    public string DatabaseConnectionString { get; private set; } = null!;

    public string Smtp4DevWebUrl { get; private set; } = null!;

    public int SmtpPort { get; private set; }

    public Smtp4DevClient Smtp4Dev { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(RepositoryPaths.Artifacts);
        Directory.CreateDirectory(RepositoryPaths.E2ELogs);
        Directory.CreateDirectory(RepositoryPaths.E2EScreenshots);
        Directory.CreateDirectory(RepositoryPaths.E2EVideos);
        Directory.CreateDirectory(RepositoryPaths.E2ETraces);

        using var startupTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));

        _database = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(DatabasePassword)
            .WithCleanUp(!_keepContainers)
            .Build();

        _smtp4Dev = new ContainerBuilder()
            .WithImage("rnwood/smtp4dev:latest")
            .WithPortBinding(25, true)
            .WithPortBinding(80, true)
            .WithEnvironment("ServerOptions__Urls", "http://*:80")
            .WithCleanUp(!_keepContainers)
            .WithCreateParameterModifier(parameters =>
            {
                parameters.HostConfig ??= new Docker.DotNet.Models.HostConfig();
                parameters.HostConfig.PortBindings ??= new Dictionary<string, IList<Docker.DotNet.Models.PortBinding>>();

                foreach (var bindings in parameters.HostConfig.PortBindings.Values)
                {
                    foreach (var binding in bindings)
                    {
                        binding.HostIP = "127.0.0.1";
                    }
                }
            })
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request => request.ForPort(80).ForPath("/")))
            .Build();

        await _database.StartAsync(startupTimeout.Token);
        await _smtp4Dev.StartAsync(startupTimeout.Token);

        DatabaseConnectionString = _database.GetConnectionString();
        SmtpPort = _smtp4Dev.GetMappedPublicPort(25);
        Smtp4DevWebUrl = $"http://127.0.0.1:{_smtp4Dev.GetMappedPublicPort(80)}";
        Smtp4Dev = new Smtp4DevClient(Smtp4DevWebUrl);

        await E2ETestDataSeeder.EnsureDatabaseAsync(DatabaseConnectionString, startupTimeout.Token);

        var apiPort = TestPort.GetFreeTcpPort();
        var webPort = TestPort.GetFreeTcpPort();
        ApiBaseUrl = $"http://127.0.0.1:{apiPort}";
        WebBaseUrl = $"http://127.0.0.1:{webPort}";

        _api = new AppProcessRunner(
            "LexiQuest.Api",
            RepositoryPaths.ApiProject,
            apiPort,
            new Dictionary<string, string>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "E2E",
                ["ASPNETCORE_URLS"] = ApiBaseUrl,
                ["ConnectionStrings__DefaultConnection"] = DatabaseConnectionString,
                ["JwtSettings__SecretKey"] = JwtSecret,
                ["JwtSettings__Issuer"] = "LexiQuest.E2E",
                ["JwtSettings__Audience"] = "LexiQuest.E2E.Client",
                ["JwtSettings__AccessTokenExpiryMinutes"] = "30",
                ["JwtSettings__RefreshTokenExpiryDays"] = "7",
                ["BlazorClient__Url"] = WebBaseUrl,
                ["EmailSettings__Host"] = "127.0.0.1",
                ["EmailSettings__Port"] = SmtpPort.ToString(),
                ["EmailSettings__UseSsl"] = "false",
                ["EmailSettings__Username"] = "",
                ["EmailSettings__Password"] = "",
                ["EmailSettings__FromEmail"] = "noreply@lexiquest.test",
                ["EmailSettings__FromName"] = "LexiQuest",
                ["EmailSettings__BaseUrl"] = WebBaseUrl,
                ["StripeSettings__ApiKey"] = "sk_test_e2e",
                ["StripeSettings__WebhookSecret"] = "whsec_e2e",
                ["StripeSettings__MonthlyPriceId"] = "price_e2e_monthly",
                ["StripeSettings__YearlyPriceId"] = "price_e2e_yearly",
                ["StripeSettings__LifetimePriceId"] = "price_e2e_lifetime",
                ["StripeSettings__SuccessUrl"] = $"{WebBaseUrl}/premium/success?session_id={{CHECKOUT_SESSION_ID}}&plan={{PLAN}}&e2e=true",
                ["StripeSettings__CancelUrl"] = $"{WebBaseUrl}/premium/cancel"
            },
            new Uri($"{ApiBaseUrl}/health/live"),
            new Uri($"{ApiBaseUrl}/health/ready"));

        _web = new AppProcessRunner(
            "LexiQuest.Web",
            RepositoryPaths.WebProject,
            webPort,
            new Dictionary<string, string>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "E2E",
                ["ASPNETCORE_URLS"] = WebBaseUrl,
                ["ApiBaseUrl"] = ApiBaseUrl
            },
            new Uri(WebBaseUrl));

        await _api.StartAsync(startupTimeout.Token);
        await _web.StartAsync(startupTimeout.Token);

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Environment.GetEnvironmentVariable("E2E_HEADLESS") is not "false",
            SlowMo = int.TryParse(Environment.GetEnvironmentVariable("E2E_SLOWMO_MS"), out var slowMo) ? slowMo : 0
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.DisposeAsync();
        }

        Playwright?.Dispose();

        if (_web is not null)
        {
            await _web.DisposeAsync();
        }

        if (_api is not null)
        {
            await _api.DisposeAsync();
        }

        if (_smtp4Dev is not null && !_keepContainers)
        {
            await _smtp4Dev.DisposeAsync();
        }

        if (_database is not null && !_keepContainers)
        {
            await _database.DisposeAsync();
        }

        _databaseLock.Dispose();
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await _databaseLock.WaitAsync(cancellationToken);
        try
        {
            await using var connection = new SqlConnection(DatabaseConnectionString);
            await connection.OpenAsync(cancellationToken);

            _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                SchemasToInclude = ["dbo"],
                TablesToIgnore = [new Table("__EFMigrationsHistory")]
            });

            await _respawner.ResetAsync(connection);
            await E2ETestDataSeeder.SeedAsync(DatabaseConnectionString, cancellationToken);
            await Smtp4Dev.ClearMessagesAsync(cancellationToken);
            await ResetE2EStateAsync(cancellationToken);
        }
        finally
        {
            _databaseLock.Release();
        }
    }

    public async Task<IPage> NewPageAsync(
        int width = 1366,
        int height = 900,
        string theme = "light",
        bool reducedMotion = false,
        string? testName = null)
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = width, Height = height },
            ColorScheme = theme.Equals("dark", StringComparison.OrdinalIgnoreCase) ? ColorScheme.Dark : ColorScheme.Light,
            ReducedMotion = reducedMotion ? ReducedMotion.Reduce : ReducedMotion.NoPreference,
            RecordVideoDir = RepositoryPaths.E2EVideos,
            RecordVideoSize = new RecordVideoSize { Width = width, Height = height }
        });

        if (ShouldTrace())
        {
            await context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }

        var page = await context.NewPageAsync();
        var diagnostics = new PageDiagnostics(testName ?? "unnamed");
        _pageDiagnostics[page] = diagnostics;

        page.Console += (_, message) =>
        {
            if (message.Type == "error")
            {
                if (IsIgnoredConsoleError(message.Text))
                {
                    return;
                }

                diagnostics.AddConsoleError(message.Text);
            }
        };
        page.PageError += (_, exception) => diagnostics.AddPageError(exception);
        page.RequestFailed += (_, request) =>
        {
            var failure = request.Failure ?? string.Empty;
            if (failure.Contains("net::ERR_ABORTED", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            diagnostics.AddFailedRequest($"{request.Method} {request.Url} -> {failure}");
        };

        return page;
    }

    public async Task<IResponse?> GoToAndWaitForAppReadyAsync(IPage page, string path = "/")
    {
        var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? path
            : $"{WebBaseUrl}{(path.StartsWith('/') ? path : $"/{path}")}";

        var response = await page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        try
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
            {
                Timeout = 10_000
            });
        }
        catch (TimeoutException)
        {
            // Blazor can keep background requests open; DOM readiness plus no visible busy indicator is sufficient.
        }

        await WaitForNoBusyIndicatorsAsync(page);
        return response;
    }

    public async Task WaitForNoBusyIndicatorsAsync(IPage page)
    {
        try
        {
            await page.WaitForFunctionAsync(
                """
                () => {
                    const busyElements = Array.from(document.querySelectorAll(
                        ".loading-state, .loading-container, .spinner, .loading-skeleton, [aria-busy='true']"
                    ));

                    return busyElements.every(element => {
                        const style = window.getComputedStyle(element);
                        const rect = element.getBoundingClientRect();
                        const ariaBusy = element.getAttribute('aria-busy');

                        return ariaBusy === 'false'
                            || style.display === 'none'
                            || style.visibility === 'hidden'
                            || rect.width === 0
                            || rect.height === 0;
                    });
                }
                """,
                new PageWaitForFunctionOptions { Timeout = 5_000 });
        }
        catch (TimeoutException)
        {
            // Individual tests assert the final state they need.
        }
    }

    public async Task<TestUser> RegisterUniqueUserAsync(
        string prefix = "e2e",
        string password = "TestPass123!",
        CancellationToken cancellationToken = default)
    {
        var unique = Guid.NewGuid().ToString("N")[..12];
        var user = new TestUser(
            Email: $"{prefix}.{unique}@lexiquest.test",
            Username: $"{prefix}{unique}"[..Math.Min(18, prefix.Length + unique.Length)],
            Password: password);

        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var registerResponse = await httpClient.PostAsJsonAsync("api/v1/users/register", new RegisterRequest
        {
            Email = user.Email,
            Username = user.Username,
            Password = user.Password,
            ConfirmPassword = user.Password,
            AcceptTerms = true
        }, cancellationToken);

        registerResponse.EnsureSuccessStatusCode();
        return user;
    }

    public async Task<E2EPersonaSet> CreatePersonaSetAsync(CancellationToken cancellationToken = default)
    {
        var freeUser = await RegisterUniqueUserAsync("freeuser", cancellationToken: cancellationToken);
        var premiumUser = await RegisterUniqueUserAsync("premiumuser", cancellationToken: cancellationToken);
        var lockedOutUser = await RegisterUniqueUserAsync("lockedout", cancellationToken: cancellationToken);
        var adminUser = await RegisterUniqueUserAsync("adminuser", cancellationToken: cancellationToken);
        var contentManagerUser = await RegisterUniqueUserAsync("contentmgr", cancellationToken: cancellationToken);
        var teamLeader = await RegisterUniqueUserAsync("teamleader", cancellationToken: cancellationToken);
        var teamOfficer = await RegisterUniqueUserAsync("teamofficer", cancellationToken: cancellationToken);
        var teamMember = await RegisterUniqueUserAsync("teammember", cancellationToken: cancellationToken);
        var multiplayerUserA = await RegisterUniqueUserAsync("mpusera", cancellationToken: cancellationToken);
        var multiplayerUserB = await RegisterUniqueUserAsync("mpuserb", cancellationToken: cancellationToken);
        var noProgressUser = await RegisterUniqueUserAsync("noprogress", cancellationToken: cancellationToken);

        await ForceUserPremiumAsync(premiumUser.Email, cancellationToken: cancellationToken);
        await ForceUserLockoutAsync(lockedOutUser.Email, cancellationToken: cancellationToken);
        await ForceAdminRoleAsync(adminUser.Email, AdminRole.Admin, cancellationToken);
        await ForceAdminRoleAsync(contentManagerUser.Email, AdminRole.ContentManager, cancellationToken);
        await ForceUserCoinsAsync(teamLeader.Email, 1_000, cancellationToken);
        await ForceUserStatsAsync(multiplayerUserA.Email, totalXp: 420, level: 4, cancellationToken: cancellationToken);
        await ForceUserStatsAsync(multiplayerUserB.Email, totalXp: 390, level: 4, cancellationToken: cancellationToken);

        var team = await CreateTeamViaApiAsync(
            teamLeader,
            new CreateTeamRequest("E2E persony", "PERS", "Deterministicky tým pro E2E persony.", null),
            cancellationToken);

        await SeedTeamMemberAsync(team.Id, teamOfficer.Email, TeamRole.Officer, weeklyXp: 80, allTimeXp: 500, wins: 1, cancellationToken);
        await SeedTeamMemberAsync(team.Id, teamMember.Email, TeamRole.Member, weeklyXp: 20, allTimeXp: 100, wins: 0, cancellationToken);

        return new E2EPersonaSet(
            FreeUser: freeUser,
            PremiumUser: premiumUser,
            LockedOutUser: lockedOutUser,
            AdminUser: adminUser,
            ContentManagerUser: contentManagerUser,
            TeamLeader: teamLeader,
            TeamOfficer: teamOfficer,
            TeamMember: teamMember,
            MultiplayerUserA: multiplayerUserA,
            MultiplayerUserB: multiplayerUserB,
            NoProgressUser: noProgressUser,
            GuestBrowserProfile: new E2EGuestBrowserProfile("guestBrowserProfile"),
            TeamId: team.Id);
    }

    public async Task LoginAsAsync(IPage page, TestUser user, bool rememberMe = false, CancellationToken cancellationToken = default)
    {
        var auth = await AuthenticateAsync(user, rememberMe, cancellationToken);

        await GoToAndWaitForAppReadyAsync(page);
        await page.EvaluateAsync(
            """
            ([accessToken, refreshToken]) => {
                window.localStorage.setItem('access_token', accessToken);
                window.localStorage.setItem('refresh_token', refreshToken);
            }
            """,
            new[] { auth.AccessToken, auth.RefreshToken });
    }

    public async Task<AuthResponse> AuthenticateAsync(TestUser user, bool rememberMe = false, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsJsonAsync("api/v1/users/login", new LoginRequest
        {
            Email = user.Email,
            Password = user.Password,
            RememberMe = rememberMe
        }, cancellationToken);

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
        auth.Should().NotBeNull();
        return auth!;
    }

    public async Task<HttpClient> CreateAuthenticatedApiClientAsync(
        TestUser user,
        CancellationToken cancellationToken = default)
    {
        var auth = await AuthenticateAsync(user, cancellationToken: cancellationToken);
        var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return httpClient;
    }

    public async Task ForceAdminRoleAsync(
        string email,
        AdminRole role,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @userId uniqueidentifier = (
                SELECT TOP 1 [Id]
                FROM [Users]
                WHERE [Email] = @email
            );

            IF @userId IS NULL
                THROW 51020, 'User not found for E2E admin role seed.', 1;

            IF NOT EXISTS (
                SELECT 1
                FROM [AdminRoleAssignments]
                WHERE [UserId] = @userId
                  AND [Role] = @role
            )
            BEGIN
                INSERT INTO [AdminRoleAssignments] ([Id], [UserId], [Role], [AssignedAt])
                VALUES (NEWID(), @userId, @role, SYSUTCDATETIME());
            END
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@role", role.ToString());

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThanOrEqualTo(0, "admin role seed must complete");
    }

    public async Task<Guid> SeedNotificationAsync(
        string email,
        NotificationType type,
        string title,
        string message,
        NotificationSeverity severity = NotificationSeverity.Info,
        bool isRead = false,
        DateTime? createdAtUtc = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @userId uniqueidentifier = (
                SELECT TOP 1 [Id]
                FROM [Users]
                WHERE [Email] = @email
            );

            IF @userId IS NULL
                THROW 51016, 'User not found for E2E notification seed.', 1;

            DECLARE @notificationId uniqueidentifier = NEWID();
            DECLARE @createdAt datetime2 = COALESCE(@createdAtUtc, SYSUTCDATETIME());

            INSERT INTO [Notifications]
                ([Id], [UserId], [Type], [Title], [Message], [Severity], [IsRead], [ReadAt], [CreatedAt], [ActionUrl])
            VALUES
                (@notificationId, @userId, @type, @title, @message, @severity, @isRead,
                 CASE WHEN @isRead = 1 THEN @createdAt ELSE NULL END,
                 @createdAt, @actionUrl);

            SELECT @notificationId;
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@type", type.ToString());
        command.Parameters.AddWithValue("@title", title);
        command.Parameters.AddWithValue("@message", message);
        command.Parameters.AddWithValue("@severity", severity.ToString());
        command.Parameters.AddWithValue("@isRead", isRead);
        command.Parameters.AddWithValue("@createdAtUtc", createdAtUtc ?? DateTime.UtcNow);
        command.Parameters.AddWithValue("@actionUrl", (object?)actionUrl ?? DBNull.Value);

        var notificationId = await command.ExecuteScalarAsync(cancellationToken);
        notificationId.Should().BeOfType<Guid>("notification seed must return inserted id");
        return (Guid)notificationId!;
    }

    public async Task<int> GetPushSubscriptionCountAsync(
        string email,
        string? endpoint = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM [PushSubscriptions] ps
            INNER JOIN [Users] u ON u.[Id] = ps.[UserId]
            WHERE u.[Email] = @email
              AND (@endpoint IS NULL OR ps.[Endpoint] = @endpoint);
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@endpoint", (object?)endpoint ?? DBNull.Value);

        var count = await command.ExecuteScalarAsync(cancellationToken);
        count.Should().BeOfType<int>();
        return (int)count!;
    }

    public async Task<ScrambledWordDto> StartGameViaApiAsync(
        TestUser user,
        StartGameRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = await CreateAuthenticatedApiClientAsync(user, cancellationToken);

        using var response = await httpClient.PostAsJsonAsync("api/v1/game/start", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var game = await response.Content.ReadFromJsonAsync<ScrambledWordDto>(cancellationToken: cancellationToken);
        game.Should().NotBeNull();
        return game!;
    }

    public async Task<TeamDto> CreateTeamViaApiAsync(
        TestUser user,
        CreateTeamRequest request,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = await CreateAuthenticatedApiClientAsync(user, cancellationToken);

        using var response = await httpClient.PostAsJsonAsync("api/v1/teams", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var team = await response.Content.ReadFromJsonAsync<TeamDto>(cancellationToken: cancellationToken);
        team.Should().NotBeNull();
        return team!;
    }

    public async Task<GameRoundResult> SubmitAnswerViaApiAsync(
        HttpClient httpClient,
        Guid sessionId,
        string answer,
        int timeSpentMs,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"api/v1/game/{sessionId}/answer",
            new SubmitAnswerRequest
            {
                SessionId = sessionId,
                Answer = answer,
                TimeSpentMs = timeSpentMs
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GameRoundResult>(cancellationToken: cancellationToken);
        result.Should().NotBeNull();
        return result!;
    }

    public async Task<UserStatsSummaryDto> GetUserStatsViaApiAsync(
        TestUser user,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = await CreateAuthenticatedApiClientAsync(user, cancellationToken);
        using var response = await httpClient.GetAsync("api/v1/stats/user", cancellationToken);
        response.EnsureSuccessStatusCode();

        var stats = await response.Content.ReadFromJsonAsync<UserStatsSummaryDto>(cancellationToken: cancellationToken);
        stats.Should().NotBeNull();
        return stats!;
    }

    public async Task<GuestStartApiResult> StartGuestGameViaApiAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsync("api/v1/game/guest/start", null, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        var game = response.IsSuccessStatusCode
            ? JsonSerializer.Deserialize<GuestStartResponse>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            : null;

        return new GuestStartApiResult(response.StatusCode, game, body);
    }

    public async Task AdvanceE2ETimeAsync(TimeSpan offset, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/e2e/time/advance",
            new { Seconds = (int)offset.TotalSeconds },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task SetQuickMatchTimeLimitAsync(int seconds, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/e2e/multiplayer/quick-match-time-limit",
            new { Seconds = seconds },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task SetFixedCorrectAnswerXpAsync(int amount, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/e2e/xp/fixed-correct-answer",
            new { Amount = amount },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task FailNextUserStatsRequestAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsync("api/v1/e2e/stats/fail-next-user-request", null, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DelayNextUserStatsRequestAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsync("api/v1/e2e/stats/delay-next-user-request", null, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task ReleaseDelayedUserStatsRequestAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsync("api/v1/e2e/stats/release-user-request", null, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DelayNextApiRequestAsync(string path, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/e2e/http/delay-next",
            new { Path = path },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task ReleaseDelayedApiRequestsAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsync("api/v1/e2e/http/release", null, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task ExpirePrivateRoomAsync(string roomCode, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/e2e/multiplayer/expire-room",
            new { RoomCode = roomCode },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task RunRoomCleanupAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsync("api/v1/e2e/multiplayer/cleanup-rooms", null, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private async Task ResetE2EStateAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ApiBaseUrl))
        {
            return;
        }

        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsync("api/v1/e2e/state/reset", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetWordOriginalsAsync(
        IEnumerable<Guid> wordIds,
        CancellationToken cancellationToken = default)
    {
        var uniqueIds = wordIds.Distinct().ToArray();
        var originals = new Dictionary<Guid, string>();

        if (uniqueIds.Length == 0)
        {
            return originals;
        }

        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var wordId in uniqueIds)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT [Original] FROM [Words] WHERE [Id] = @wordId";
            command.Parameters.AddWithValue("@wordId", wordId);

            var original = await command.ExecuteScalarAsync(cancellationToken);
            original.Should().NotBeNull($"word {wordId} must exist in the E2E seed");
            originals[wordId] = (string)original!;
        }

        return originals;
    }

    public async Task<string> GetBeginnerOriginalForScrambledWordAsync(
        string scrambled,
        CancellationToken cancellationToken = default)
    {
        return await GetOriginalForScrambledWordAsync(scrambled, DifficultyLevel.Beginner, cancellationToken);
    }

    public async Task EnsureWordAsync(
        string original,
        DifficultyLevel difficulty,
        WordCategory category,
        int frequencyRank = 1,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @normalized nvarchar(100) = LOWER(@original);

            IF EXISTS (SELECT 1 FROM [Words] WHERE [Normalized] = @normalized)
            BEGIN
                UPDATE [Words]
                SET [Original] = @original,
                    [Length] = LEN(@original),
                    [Difficulty] = @difficulty,
                    [Category] = @category,
                    [FrequencyRank] = @frequencyRank
                WHERE [Normalized] = @normalized;
            END
            ELSE
            BEGIN
                INSERT INTO [Words] ([Id], [Original], [Normalized], [Length], [Difficulty], [Category], [FrequencyRank])
                VALUES (NEWID(), @original, @normalized, LEN(@original), @difficulty, @category, @frequencyRank);
            END
            """;
        command.Parameters.AddWithValue("@original", original);
        command.Parameters.AddWithValue("@difficulty", (int)difficulty);
        command.Parameters.AddWithValue("@category", (int)category);
        command.Parameters.AddWithValue("@frequencyRank", frequencyRank);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0);
    }

    public async Task<string> GetOriginalForScrambledWordAsync(
        string scrambled,
        DifficultyLevel? difficulty = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = difficulty.HasValue
            ? "SELECT [Original] FROM [Words] WHERE [Difficulty] = @difficulty"
            : "SELECT [Original] FROM [Words]";
        if (difficulty.HasValue)
        {
            command.Parameters.AddWithValue("@difficulty", (int)difficulty.Value);
        }

        var originals = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            originals.Add(reader.GetString(0));
        }

        var scrambledKey = SortLetters(scrambled);
        var candidates = originals
            .Where(original => SortLetters(original) == scrambledKey)
            .ToArray();

        var difficultyLabel = difficulty?.ToString() ?? "seeded";
        candidates.Should().ContainSingle($"scrambled word '{scrambled}' must map to exactly one {difficultyLabel} word");
        return candidates[0];
    }

    public async Task ForceCurrentRoundAsync(
        Guid sessionId,
        string correctAnswer,
        string scrambledWord,
        int? timeLimitSeconds = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [GameRounds]
            SET [CorrectAnswer] = @correctAnswer,
                [ScrambledWord] = @scrambledWord,
                [TimeLimitSeconds] = COALESCE(@timeLimitSeconds, [TimeLimitSeconds])
            WHERE [SessionId] = @sessionId
              AND [IsCompleted] = 0
            """;
        command.Parameters.AddWithValue("@sessionId", sessionId);
        command.Parameters.AddWithValue("@correctAnswer", correctAnswer);
        command.Parameters.AddWithValue("@scrambledWord", scrambledWord);
        command.Parameters.AddWithValue("@timeLimitSeconds", timeLimitSeconds.HasValue
            ? timeLimitSeconds.Value
            : DBNull.Value);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, $"session {sessionId} must have an active round");
    }

    public async Task ForceActiveRoundStartedAtAsync(
        Guid sessionId,
        DateTime startedAtUtc,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [GameRounds]
            SET [StartedAt] = @startedAt
            WHERE [SessionId] = @sessionId
              AND [IsCompleted] = 0
            """;
        command.Parameters.AddWithValue("@sessionId", sessionId);
        command.Parameters.AddWithValue("@startedAt", startedAtUtc);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, $"session {sessionId} must have an active round");
    }

    public async Task<string> GetActiveRoundAnswerAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP 1 [CorrectAnswer]
            FROM [GameRounds]
            WHERE [SessionId] = @sessionId
              AND [IsCompleted] = 0
            ORDER BY [RoundNumber] DESC
            """;
        command.Parameters.AddWithValue("@sessionId", sessionId);

        var answer = await command.ExecuteScalarAsync(cancellationToken);
        answer.Should().NotBeNull($"session {sessionId} must have an active round");
        return (string)answer!;
    }

    public async Task ForceSessionLivesAsync(
        Guid sessionId,
        int livesRemaining,
        DateTime? nextLifeRegenAt = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [GameSessions]
            SET [LivesRemaining] = @livesRemaining
            WHERE [Id] = @sessionId;

            UPDATE [Users]
            SET [LivesRemaining] = @livesRemaining,
                [NextLifeRegenAt] = @nextLifeRegenAt
            WHERE [Id] = (
                SELECT TOP 1 [UserId]
                FROM [GameSessions]
                WHERE [Id] = @sessionId
            );
            """;
        command.Parameters.AddWithValue("@sessionId", sessionId);
        command.Parameters.AddWithValue("@livesRemaining", livesRemaining);
        command.Parameters.AddWithValue("@nextLifeRegenAt", nextLifeRegenAt.HasValue
            ? nextLifeRegenAt.Value
            : DBNull.Value);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, $"session {sessionId} must exist");
    }

    public async Task ForceSessionTotalRoundsAsync(
        Guid sessionId,
        int totalRounds,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [GameSessions]
            SET [TotalRounds] = @totalRounds
            WHERE [Id] = @sessionId
            """;
        command.Parameters.AddWithValue("@sessionId", sessionId);
        command.Parameters.AddWithValue("@totalRounds", totalRounds);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().Be(1, $"session {sessionId} must exist");
    }

    public async Task ForceUserStreakAsync(
        string email,
        int currentDays,
        int longestDays,
        DateTime? lastActivityUtc,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [Users]
            SET [Streak_CurrentDays] = @currentDays,
                [Streak_LongestDays] = @longestDays,
                [Streak_LastActivityDate] = @lastActivityUtc
            WHERE [Email] = @email
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@currentDays", currentDays);
        command.Parameters.AddWithValue("@longestDays", longestDays);
        command.Parameters.AddWithValue("@lastActivityUtc", lastActivityUtc.HasValue
            ? lastActivityUtc.Value
            : DBNull.Value);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, $"user {email} must exist");
    }

    public async Task ForceUserCoinsAsync(
        string email,
        int coinBalance,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [Users]
            SET [CoinBalance] = @coinBalance
            WHERE [Email] = @email
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@coinBalance", coinBalance);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, $"user {email} must exist");
    }

    public async Task ForceUserPremiumAsync(
        string email,
        bool isPremium = true,
        DateTime? expiresAtUtc = null,
        string premiumPlan = "E2E",
        string subscriptionPlan = "Monthly",
        string? stripeSubscriptionId = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @userId uniqueidentifier = (
                SELECT TOP 1 [Id]
                FROM [Users]
                WHERE [Email] = @email
            );

            IF @userId IS NULL
                THROW 51002, 'User not found for E2E premium seed.', 1;

            UPDATE [Users]
            SET [Premium_IsPremium] = @isPremium,
                [Premium_ExpiresAt] = @expiresAt,
                [Premium_Plan] = @premiumPlan
            WHERE [Id] = @userId;

            IF @isPremium = 1
            BEGIN
                MERGE [Subscriptions] AS target
                USING (SELECT @userId AS [UserId]) AS source
                ON target.[UserId] = source.[UserId]
                WHEN MATCHED THEN
                    UPDATE SET [Plan] = @subscriptionPlan,
                               [Status] = 'Active',
                               [StartedAt] = SYSUTCDATETIME(),
                               [ExpiresAt] = @expiresAt,
                               [CancelledAt] = NULL,
                               [StripeSubscriptionId] = @stripeSubscriptionId
                WHEN NOT MATCHED THEN
                    INSERT ([Id], [UserId], [Plan], [StartedAt], [ExpiresAt], [CancelledAt], [StripeSubscriptionId], [Status])
                    VALUES (NEWID(), @userId, @subscriptionPlan, SYSUTCDATETIME(), @expiresAt, NULL, @stripeSubscriptionId, 'Active');
            END
            ELSE
            BEGIN
                DELETE FROM [Subscriptions]
                WHERE [UserId] = @userId;
            END
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@isPremium", isPremium);
        command.Parameters.AddWithValue("@expiresAt", isPremium
            ? expiresAtUtc ?? DateTime.UtcNow.AddDays(30)
            : DBNull.Value);
        command.Parameters.AddWithValue("@premiumPlan", isPremium ? premiumPlan : DBNull.Value);
        command.Parameters.AddWithValue("@subscriptionPlan", subscriptionPlan);
        command.Parameters.AddWithValue("@stripeSubscriptionId", stripeSubscriptionId ?? $"sub_e2e_{Guid.NewGuid():N}");

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, $"user {email} must exist");
    }

    public async Task ForceUserLockoutAsync(
        string email,
        DateTime? lockoutEndUtc = null,
        int failedLoginAttempts = 5,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [Users]
            SET [FailedLoginAttempts] = @failedLoginAttempts,
                [LockoutEnd] = @lockoutEnd
            WHERE [Email] = @email
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@failedLoginAttempts", failedLoginAttempts);
        command.Parameters.AddWithValue("@lockoutEnd", lockoutEndUtc ?? DateTime.UtcNow.AddMinutes(15));

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().Be(1, $"user {email} must exist");
    }

    public async Task SeedTeamMemberAsync(
        Guid teamId,
        string email,
        TeamRole role,
        int weeklyXp = 0,
        long allTimeXp = 0,
        int wins = 0,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @userId uniqueidentifier = (
                SELECT TOP 1 [Id]
                FROM [Users]
                WHERE [Email] = @email
            );

            IF @userId IS NULL
                THROW 51006, 'User not found for E2E team member seed.', 1;

            IF NOT EXISTS (SELECT 1 FROM [Teams] WHERE [Id] = @teamId)
                THROW 51007, 'Team not found for E2E team member seed.', 1;

            MERGE [TeamMembers] AS target
            USING (SELECT @userId AS [UserId], @teamId AS [TeamId]) AS source
            ON target.[UserId] = source.[UserId]
               AND target.[TeamId] = source.[TeamId]
            WHEN MATCHED THEN
                UPDATE SET [Role] = @role,
                           [WeeklyXP] = @weeklyXp,
                           [AllTimeXP] = @allTimeXp,
                           [Wins] = @wins
            WHEN NOT MATCHED THEN
                INSERT ([Id], [UserId], [TeamId], [Role], [JoinedAt], [WeeklyXP], [AllTimeXP], [Wins])
                VALUES (NEWID(), @userId, @teamId, @role, SYSUTCDATETIME(), @weeklyXp, @allTimeXp, @wins);

            IF @role = 'Leader'
            BEGIN
                UPDATE [Teams]
                SET [LeaderId] = @userId
                WHERE [Id] = @teamId;
            END
            """;
        command.Parameters.AddWithValue("@teamId", teamId);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@role", role.ToString());
        command.Parameters.AddWithValue("@weeklyXp", weeklyXp);
        command.Parameters.AddWithValue("@allTimeXp", allTimeXp);
        command.Parameters.AddWithValue("@wins", wins);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, "team member seed must insert or update a row");
    }

    public async Task SetStripeCustomerIdAsync(
        string email,
        string stripeCustomerId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [Users]
            SET [StripeCustomerId] = @stripeCustomerId
            WHERE [Email] = @email
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@stripeCustomerId", stripeCustomerId);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, $"user {email} must exist");
    }

    public async Task ForceStreakProtectionAsync(
        string email,
        int shieldsRemaining = 0,
        bool hasActiveShield = false,
        bool freezeUsedThisWeek = false,
        DateTime? lastShieldActivatedAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @userId uniqueidentifier = (
                SELECT TOP 1 [Id]
                FROM [Users]
                WHERE [Email] = @email
            );

            IF @userId IS NULL
                THROW 51001, 'User not found for E2E streak protection seed.', 1;

            MERGE [StreakProtections] AS target
            USING (SELECT @userId AS [UserId]) AS source
            ON target.[UserId] = source.[UserId]
            WHEN MATCHED THEN
                UPDATE SET [ShieldsRemaining] = @shieldsRemaining,
                           [IsShieldActive] = @hasActiveShield,
                           [FreezeUsedThisWeek] = @freezeUsedThisWeek,
                           [LastShieldActivatedAt] = @lastShieldActivatedAt
            WHEN NOT MATCHED THEN
                INSERT ([Id], [UserId], [ShieldsRemaining], [IsShieldActive], [FreezeUsedThisWeek], [LastShieldActivatedAt])
                VALUES (NEWID(), @userId, @shieldsRemaining, @hasActiveShield, @freezeUsedThisWeek, @lastShieldActivatedAt);
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@shieldsRemaining", shieldsRemaining);
        command.Parameters.AddWithValue("@hasActiveShield", hasActiveShield);
        command.Parameters.AddWithValue("@freezeUsedThisWeek", freezeUsedThisWeek);
        command.Parameters.AddWithValue("@lastShieldActivatedAt", lastShieldActivatedAtUtc.HasValue
            ? lastShieldActivatedAtUtc.Value
            : DBNull.Value);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, "streak protection seed must insert or update a row");
    }

    public async Task ForceUserStatsAsync(
        string email,
        int totalXp,
        int level,
        int totalWordsSolved = 0,
        double accuracy = 0,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var columnsCommand = connection.CreateCommand())
        {
            columnsCommand.CommandText = """
                SELECT [COLUMN_NAME]
                FROM [INFORMATION_SCHEMA].[COLUMNS]
                WHERE [TABLE_SCHEMA] = 'dbo'
                  AND [TABLE_NAME] = 'Users'
                """;

            await using var reader = await columnsCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(reader.GetString(0));
            }
        }

        var totalXpColumn = ResolveColumn(columns, "TotalXP", "Stats_TotalXP");
        var levelColumn = ResolveColumn(columns, "Level", "Stats_Level");
        var wordsColumn = ResolveColumn(columns, "TotalWordsSolved", "Stats_TotalWordsSolved");
        var accuracyColumn = ResolveColumn(columns, "Accuracy", "Stats_Accuracy");
        var averageTimeColumn = ResolveColumn(columns, "AverageResponseTime", "Stats_AverageResponseTime");

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            UPDATE [Users]
            SET [{totalXpColumn}] = @totalXp,
                [{levelColumn}] = @level,
                [{wordsColumn}] = @totalWordsSolved,
                [{accuracyColumn}] = @accuracy,
                [{averageTimeColumn}] = @averageResponseTime
            WHERE [Email] = @email
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@totalXp", totalXp);
        command.Parameters.AddWithValue("@level", level);
        command.Parameters.AddWithValue("@totalWordsSolved", totalWordsSolved);
        command.Parameters.AddWithValue("@accuracy", accuracy);
        command.Parameters.AddWithValue("@averageResponseTime", TimeSpan.Zero);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().Be(1, $"user {email} must exist");
    }

    public async Task ForceLeagueWeeklyXpAsync(
        params (string Email, int WeeklyXp)[] entries)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        foreach (var (email, weeklyXp) in entries)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (SqlTransaction)transaction;
            command.CommandText = """
                UPDATE lp
                SET lp.[WeeklyXP] = @weeklyXp,
                    lp.[Rank] = 0,
                    lp.[IsPromoted] = 0,
                    lp.[IsDemoted] = 0
                FROM [LeagueParticipants] lp
                INNER JOIN [Users] u ON u.[Id] = lp.[UserId]
                INNER JOIN [Leagues] l ON l.[Id] = lp.[LeagueId]
                WHERE u.[Email] = @email
                  AND l.[IsActive] = 1
                """;
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@weeklyXp", weeklyXp);

            var affected = await command.ExecuteNonQueryAsync();
            affected.Should().Be(1, $"league participant for {email} must exist");
        }

        await transaction.CommitAsync();
    }

    public async Task ForceLeagueWeekEndAsync(
        string email,
        DateTime weekEndUtc,
        CancellationToken cancellationToken = default)
    {
        var weekStartUtc = weekEndUtc.AddDays(-7);

        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE l
            SET l.[WeekStart] = @weekStart,
                l.[WeekEnd] = @weekEnd
            FROM [Leagues] l
            INNER JOIN [LeagueParticipants] lp ON lp.[LeagueId] = l.[Id]
            INNER JOIN [Users] u ON u.[Id] = lp.[UserId]
            WHERE u.[Email] = @email
              AND l.[IsActive] = 1
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@weekStart", weekStartUtc);
        command.Parameters.AddWithValue("@weekEnd", weekEndUtc);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().Be(1, $"active league for {email} must exist");
    }

    public async Task ForceUsersIntoActiveLeagueTierAsync(
        LeagueTier tier,
        params string[] emails)
    {
        emails.Should().NotBeEmpty("at least one user must be moved to a league tier");

        var leagueId = Guid.NewGuid();
        var weekStart = GetCurrentUtcWeekStart();
        var weekEnd = weekStart.AddDays(7);

        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var insertLeague = connection.CreateCommand())
        {
            insertLeague.Transaction = (SqlTransaction)transaction;
            insertLeague.CommandText = """
                INSERT INTO [Leagues] ([Id], [Tier], [WeekStart], [WeekEnd], [IsActive], [CreatedAt])
                VALUES (@leagueId, @tier, @weekStart, @weekEnd, 1, @createdAt)
                """;
            insertLeague.Parameters.AddWithValue("@leagueId", leagueId);
            insertLeague.Parameters.AddWithValue("@tier", tier.ToString());
            insertLeague.Parameters.AddWithValue("@weekStart", weekStart);
            insertLeague.Parameters.AddWithValue("@weekEnd", weekEnd);
            insertLeague.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
            await insertLeague.ExecuteNonQueryAsync();
        }

        foreach (var email in emails)
        {
            await using var moveParticipant = connection.CreateCommand();
            moveParticipant.Transaction = (SqlTransaction)transaction;
            moveParticipant.CommandText = """
                UPDATE lp
                SET lp.[LeagueId] = @leagueId,
                    lp.[WeeklyXP] = 0,
                    lp.[Rank] = 0,
                    lp.[IsPromoted] = 0,
                    lp.[IsDemoted] = 0
                FROM [LeagueParticipants] lp
                INNER JOIN [Users] u ON u.[Id] = lp.[UserId]
                INNER JOIN [Leagues] l ON l.[Id] = lp.[LeagueId]
                WHERE u.[Email] = @email
                  AND l.[IsActive] = 1
                """;
            moveParticipant.Parameters.AddWithValue("@email", email);
            moveParticipant.Parameters.AddWithValue("@leagueId", leagueId);

            var affected = await moveParticipant.ExecuteNonQueryAsync();
            affected.Should().Be(1, $"active league participant for {email} must exist");
        }

        await transaction.CommitAsync();
    }

    public async Task ForceActiveLeaguesToPreviousWeekAsync(CancellationToken cancellationToken = default)
    {
        var currentWeekStart = GetCurrentUtcWeekStart();
        var previousWeekStart = currentWeekStart.AddDays(-7);

        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [Leagues]
            SET [WeekStart] = @weekStart,
                [WeekEnd] = @weekEnd
            WHERE [IsActive] = 1
            """;
        command.Parameters.AddWithValue("@weekStart", previousWeekStart);
        command.Parameters.AddWithValue("@weekEnd", currentWeekStart);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, "there must be at least one active league to move to the previous week");
    }

    public async Task RunLeagueResetAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{ApiBaseUrl}/") };
        using var response = await httpClient.PostAsync("api/v1/e2e/leagues/reset", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SeedDailyChallengeCompletionAsync(
        string email,
        DateTime challengeDate,
        TimeSpan timeTaken,
        int xpEarned,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @userId uniqueidentifier = (
                SELECT TOP 1 [Id]
                FROM [Users]
                WHERE [Email] = @email
            );

            IF @userId IS NULL
                THROW 51004, 'User not found for E2E daily challenge seed.', 1;

            MERGE [DailyChallengeCompletions] AS target
            USING (SELECT @userId AS [UserId], @challengeDate AS [ChallengeDate]) AS source
            ON target.[UserId] = source.[UserId]
               AND target.[ChallengeDate] = source.[ChallengeDate]
            WHEN MATCHED THEN
                UPDATE SET [TimeTaken] = @timeTaken,
                           [XPEarned] = @xpEarned,
                           [CompletedAt] = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT ([Id], [UserId], [ChallengeDate], [TimeTaken], [XPEarned], [CompletedAt])
                VALUES (NEWID(), @userId, @challengeDate, @timeTaken, @xpEarned, SYSUTCDATETIME());
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@challengeDate", challengeDate.Date);
        command.Parameters.AddWithValue("@timeTaken", timeTaken);
        command.Parameters.AddWithValue("@xpEarned", xpEarned);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, "daily challenge completion seed must insert or update a row");
    }

    public async Task SeedUserAchievementAsync(
        string email,
        string achievementKey,
        int progress,
        bool isUnlocked,
        DateTime? unlockedAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @userId uniqueidentifier = (
                SELECT TOP 1 [Id]
                FROM [Users]
                WHERE [Email] = @email
            );

            DECLARE @achievementId uniqueidentifier = (
                SELECT TOP 1 [Id]
                FROM [Achievements]
                WHERE [Key] = @achievementKey
            );

            IF @userId IS NULL OR @achievementId IS NULL
                THROW 51005, 'User or achievement not found for E2E achievement seed.', 1;

            MERGE [UserAchievements] AS target
            USING (SELECT @userId AS [UserId], @achievementId AS [AchievementId]) AS source
            ON target.[UserId] = source.[UserId]
               AND target.[AchievementId] = source.[AchievementId]
            WHEN MATCHED THEN
                UPDATE SET [Progress] = @progress,
                           [IsUnlocked] = @isUnlocked,
                           [UnlockedAt] = @unlockedAt
            WHEN NOT MATCHED THEN
                INSERT ([Id], [UserId], [AchievementId], [Progress], [IsUnlocked], [UnlockedAt])
                VALUES (NEWID(), @userId, @achievementId, @progress, @isUnlocked, @unlockedAt);
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@achievementKey", achievementKey);
        command.Parameters.AddWithValue("@progress", progress);
        command.Parameters.AddWithValue("@isUnlocked", isUnlocked);
        command.Parameters.AddWithValue("@unlockedAt", isUnlocked
            ? unlockedAtUtc ?? DateTime.UtcNow
            : DBNull.Value);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, "achievement progress seed must insert or update a row");
    }

    public async Task<int> GetUserAchievementCountAsync(
        string email,
        string achievementKey,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM [UserAchievements] ua
            INNER JOIN [Users] u ON u.[Id] = ua.[UserId]
            INNER JOIN [Achievements] a ON a.[Id] = ua.[AchievementId]
            WHERE u.[Email] = @email
              AND a.[Key] = @achievementKey
              AND ua.[IsUnlocked] = 1
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@achievementKey", achievementKey);

        var count = await command.ExecuteScalarAsync(cancellationToken);
        return (int)count!;
    }

    public async Task ForcePathLevelProgressAsync(
        string email,
        Guid pathId,
        int levelNumber,
        bool isPerfect,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(DatabaseConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @userId uniqueidentifier = (
                SELECT TOP 1 [Id]
                FROM [Users]
                WHERE [Email] = @email
            );

            DECLARE @pathLevelId uniqueidentifier = (
                SELECT TOP 1 [Id]
                FROM [PathLevels]
                WHERE ([PathId] = @pathId OR [LearningPathId] = @pathId)
                  AND [LevelNumber] = @levelNumber
            );

            IF @userId IS NULL OR @pathLevelId IS NULL
                THROW 51000, 'User or path level not found for E2E progress seed.', 1;

            MERGE [UserPathLevelProgresses] AS target
            USING (SELECT @userId AS [UserId], @pathId AS [PathId], @levelNumber AS [LevelNumber]) AS source
            ON target.[UserId] = source.[UserId]
               AND target.[PathId] = source.[PathId]
               AND target.[LevelNumber] = source.[LevelNumber]
            WHEN MATCHED THEN
                UPDATE SET [PathLevelId] = @pathLevelId,
                           [Status] = @status,
                           [IsPerfect] = @isPerfect,
                           [CompletedAt] = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT ([Id], [UserId], [PathId], [PathLevelId], [LevelNumber], [Status], [IsPerfect], [CompletedAt])
                VALUES (NEWID(), @userId, @pathId, @pathLevelId, @levelNumber, @status, @isPerfect, SYSUTCDATETIME());
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@pathId", pathId);
        command.Parameters.AddWithValue("@levelNumber", levelNumber);
        command.Parameters.AddWithValue("@status", isPerfect ? 4 : 3);
        command.Parameters.AddWithValue("@isPerfect", isPerfect);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        affected.Should().BeGreaterThan(0, "path level progress seed must insert or update a row");
    }

    private static string ResolveColumn(IReadOnlySet<string> columns, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (columns.Contains(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            $"None of the expected columns exists: {string.Join(", ", candidates)}");
    }

    private static DateTime GetCurrentUtcWeekStart()
    {
        var today = DateTime.UtcNow.Date;
        var daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return today.AddDays(-daysSinceMonday);
    }

    public string CreateExpiredAccessToken(AuthResponse auth)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, auth.User.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, auth.User.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, auth.User.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "LexiQuest.E2E",
            audience: "LexiQuest.E2E.Client",
            claims: claims,
            notBefore: now.AddMinutes(-10),
            expires: now.AddMinutes(-5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task AssertNoConsoleErrorsAsync(IPage page)
    {
        var diagnostics = GetDiagnostics(page);
        diagnostics.ConsoleErrors.Should().BeEmpty();
        diagnostics.PageErrors.Should().BeEmpty();
        return Task.CompletedTask;
    }

    private static bool IsIgnoredConsoleError(string text)
    {
        return text.Contains("Error in mono_download_assets", StringComparison.OrdinalIgnoreCase)
            && text.Contains("TypeError: Failed to fetch", StringComparison.OrdinalIgnoreCase)
            && text.Contains("/_framework/", StringComparison.OrdinalIgnoreCase)
            && text.Contains(".wasm", StringComparison.OrdinalIgnoreCase);
    }

    public Task AssertNoFailedRequestsAsync(IPage page)
    {
        GetDiagnostics(page).FailedRequests.Should().BeEmpty();
        return Task.CompletedTask;
    }

    public async Task RunA11yCheckAsync(IPage page)
    {
        var issues = await page.EvaluateAsync<string[]>(
            """
            () => {
                const issues = [];
                const lang = document.documentElement.getAttribute('lang') || '';
                if (!lang.toLowerCase().startsWith('cs')) {
                    issues.push('Dokument nema cesky lang atribut.');
                }
                if (!document.title || !document.title.trim()) {
                    issues.push('Dokument nema title.');
                }
                for (const element of document.querySelectorAll('input, textarea, select')) {
                    if (element.type === 'hidden') continue;
                    const hasLabel = (element.labels && element.labels.length > 0)
                        || element.getAttribute('aria-label')
                        || element.getAttribute('aria-labelledby')
                        || element.getAttribute('placeholder');
                    if (!hasLabel) {
                        issues.push(`Pole bez labelu: ${element.outerHTML.slice(0, 120)}`);
                    }
                }
                for (const image of document.querySelectorAll('img')) {
                    if (!image.getAttribute('alt')) {
                        issues.push(`Obrazek bez alt: ${image.outerHTML.slice(0, 120)}`);
                    }
                }
                return issues;
            }
            """);

        issues.Should().BeEmpty();
    }

    public async Task<string> TakeCheckpointScreenshotAsync(
        IPage page,
        string area,
        string scenario,
        string state,
        string viewport,
        string theme,
        string? persona = null,
        bool fullPage = true,
        bool scrollToTop = true)
    {
        var directory = Path.Combine(
            RepositoryPaths.E2EScreenshots,
            Sanitize(area),
            Sanitize(scenario),
            Sanitize(viewport),
            Sanitize(theme));

        Directory.CreateDirectory(directory);

        var screenshotPath = Path.Combine(directory, $"{Sanitize(state)}.png");
        if (scrollToTop)
        {
            await page.EvaluateAsync(
                """
                () => new Promise(resolve => {
                    window.scrollTo({ top: 0, left: 0, behavior: 'instant' });
                    requestAnimationFrame(() => requestAnimationFrame(resolve));
                })
                """);
        }

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = screenshotPath,
            FullPage = fullPage,
            Animations = ScreenshotAnimations.Disabled
        });

        var metadataPath = Path.ChangeExtension(screenshotPath, ".json");
        var metadata = new
        {
            area,
            scenario,
            state,
            viewport,
            theme,
            persona,
            url = page.Url,
            takenAtUtc = DateTimeOffset.UtcNow,
            seed = "phase-9-e2e-v1",
            test = GetDiagnostics(page).TestName
        };

        await File.WriteAllTextAsync(
            metadataPath,
            JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));

        var approvedRelativePath = BuildScreenshotRelativePath(area, scenario, viewport, theme, state);
        if (ScreenshotApprovalManifest.IsApproved(approvedRelativePath))
        {
            await page.ToHaveScreenshotAsync(approvedRelativePath, fullPage);
        }

        return screenshotPath;
    }

    private static string BuildScreenshotRelativePath(
        string area,
        string scenario,
        string viewport,
        string theme,
        string state) =>
        Path.Combine(
                Sanitize(area),
                Sanitize(scenario),
                Sanitize(viewport),
                Sanitize(theme),
                $"{Sanitize(state)}.png")
            .Replace(Path.DirectorySeparatorChar, '/');

    public async Task TakeFailureArtifactsAsync(IPage page, string area, string scenario)
    {
        var safeArea = Sanitize(area);
        var safeScenario = Sanitize(scenario);
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var directory = Path.Combine(RepositoryPaths.Artifacts, "failures", safeArea, safeScenario);
        Directory.CreateDirectory(directory);

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(directory, $"{timestamp}.png"),
            FullPage = true,
            Animations = ScreenshotAnimations.Disabled
        });

        var diagnostics = GetDiagnostics(page);
        await File.WriteAllLinesAsync(Path.Combine(directory, $"{timestamp}-console.log"), diagnostics.AllLines());

        if (ShouldTrace())
        {
            await page.Context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = Path.Combine(RepositoryPaths.E2ETraces, $"{safeArea}-{safeScenario}-{timestamp}.zip")
            });
        }
    }

    public async Task WriteEnvironmentLogsAsync(string suffix, CancellationToken cancellationToken = default)
    {
        if (_api is not null)
        {
            await _api.WriteLogsAsync(suffix, cancellationToken);
        }

        if (_web is not null)
        {
            await _web.WriteLogsAsync(suffix, cancellationToken);
        }

        if (_database is not null)
        {
            await WriteContainerLogsAsync(_database, $"mssql-{suffix}", cancellationToken);
        }

        if (_smtp4Dev is not null)
        {
            await WriteContainerLogsAsync(_smtp4Dev, $"smtp4dev-{suffix}", cancellationToken);
        }
    }

    private PageDiagnostics GetDiagnostics(IPage page)
    {
        _pageDiagnostics.TryGetValue(page, out var diagnostics).Should().BeTrue("page diagnostics must be registered");
        return diagnostics!;
    }

    private static async Task WriteContainerLogsAsync(IContainer container, string fileName, CancellationToken cancellationToken)
    {
        var (stdout, stderr) = await container.GetLogsAsync(
            since: DateTime.UtcNow.AddHours(-1),
            until: DateTime.UtcNow.AddMinutes(1),
            timestampsEnabled: true,
            ct: cancellationToken);

        await File.WriteAllTextAsync(Path.Combine(RepositoryPaths.E2ELogs, $"{fileName}-stdout.log"), stdout, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(RepositoryPaths.E2ELogs, $"{fileName}-stderr.log"), stderr, cancellationToken);
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(ch => invalid.Contains(ch) || char.IsWhiteSpace(ch) ? '-' : char.ToLowerInvariant(ch)));
    }

    private static string SortLetters(string value) =>
        new(value.Trim().ToUpperInvariant().OrderBy(ch => ch).ToArray());

    private static bool ShouldTrace() =>
        Environment.GetEnvironmentVariable("E2E_TRACE") is not ("off" or "0" or "false");
}

public sealed record TestUser(string Email, string Username, string Password);

public sealed record E2EGuestBrowserProfile(string Name);

public sealed record E2EPersonaSet(
    TestUser FreeUser,
    TestUser PremiumUser,
    TestUser LockedOutUser,
    TestUser AdminUser,
    TestUser ContentManagerUser,
    TestUser TeamLeader,
    TestUser TeamOfficer,
    TestUser TeamMember,
    TestUser MultiplayerUserA,
    TestUser MultiplayerUserB,
    TestUser NoProgressUser,
    E2EGuestBrowserProfile GuestBrowserProfile,
    Guid TeamId);

public sealed record GuestStartApiResult(HttpStatusCode StatusCode, GuestStartResponse? Game, string Body);

internal sealed class PageDiagnostics
{
    private readonly List<string> _consoleErrors = [];
    private readonly List<string> _pageErrors = [];
    private readonly List<string> _failedRequests = [];

    public PageDiagnostics(string testName) => TestName = testName;

    public string TestName { get; }

    public IReadOnlyList<string> ConsoleErrors
    {
        get
        {
            lock (_consoleErrors)
            {
                return _consoleErrors.ToList();
            }
        }
    }

    public IReadOnlyList<string> PageErrors
    {
        get
        {
            lock (_pageErrors)
            {
                return _pageErrors.ToList();
            }
        }
    }

    public IReadOnlyList<string> FailedRequests
    {
        get
        {
            lock (_failedRequests)
            {
                return _failedRequests.ToList();
            }
        }
    }

    public void AddConsoleError(string message)
    {
        lock (_consoleErrors)
        {
            _consoleErrors.Add(message);
        }
    }

    public void AddPageError(string message)
    {
        lock (_pageErrors)
        {
            _pageErrors.Add(message);
        }
    }

    public void AddFailedRequest(string message)
    {
        lock (_failedRequests)
        {
            _failedRequests.Add(message);
        }
    }

    public IEnumerable<string> AllLines()
    {
        yield return $"Test: {TestName}";
        yield return "";
        yield return "Console errors:";
        foreach (var line in ConsoleErrors)
        {
            yield return line;
        }

        yield return "";
        yield return "Page errors:";
        foreach (var line in PageErrors)
        {
            yield return line;
        }

        yield return "";
        yield return "Failed requests:";
        foreach (var line in FailedRequests)
        {
            yield return line;
        }
    }
}
