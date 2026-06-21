using System.Net;
using FluentAssertions;
using LexiQuest.E2E.Tests.Infrastructure;
using Microsoft.Data.SqlClient;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Smoke")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class InfrastructureE2ETests
{
    private readonly E2EEnvironmentFixture _fixture;

    public InfrastructureE2ETests(E2EEnvironmentFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task E2EEnvironment_StartsApiWebDatabaseAndSmtp4Dev()
    {
        using var httpClient = new HttpClient();

        var apiHealth = await httpClient.GetAsync($"{_fixture.ApiBaseUrl}/health/live");
        var webHome = await httpClient.GetAsync(_fixture.BaseUrl);
        var smtpHome = await httpClient.GetAsync(_fixture.Smtp4DevWebUrl);

        apiHealth.StatusCode.Should().Be(HttpStatusCode.OK);
        webHome.StatusCode.Should().Be(HttpStatusCode.OK);
        smtpHome.StatusCode.Should().Be(HttpStatusCode.OK);
        (_fixture.DatabaseConnectionString.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || _fixture.DatabaseConnectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue("SQL Server Testcontainer must expose only a local host endpoint");
        _fixture.SmtpPort.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task E2EEnvironment_AppliesEfMigrationsAndReadyHealth()
    {
        using var httpClient = new HttpClient();
        var readyHealth = await httpClient.GetAsync($"{_fixture.ApiBaseUrl}/health/ready");

        readyHealth.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var connection = new SqlConnection(_fixture.DatabaseConnectionString);
        await connection.OpenAsync();

        var migrationCount = await ExecuteScalarAsync<int>(
            connection,
            "SELECT COUNT(*) FROM [__EFMigrationsHistory]");
        var advancedTableCount = await ExecuteScalarAsync<int>(
            connection,
            """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = 'dbo'
              AND TABLE_NAME IN (
                'LearningPaths',
                'PathLevels',
                'PasswordResetTokens',
                'Leagues',
                'DailyChallenges',
                'Achievements',
                'Subscriptions',
                'ShopItems',
                'CustomDictionaries',
                'MatchResults',
                'Teams',
                'Notifications',
                'AdminRoleAssignments')
            """);

        migrationCount.Should().BeGreaterThan(0);
        advancedTableCount.Should().Be(13, "E2E DB must be created by migrations that cover the current model");
    }

    [Fact]
    public async Task E2EPersonaSet_CreatesNamedUsersRolesAndGuestProfile()
    {
        await _fixture.ResetDatabaseAsync();

        var personas = await _fixture.CreatePersonaSetAsync();

        new[]
        {
            personas.FreeUser.Email,
            personas.PremiumUser.Email,
            personas.LockedOutUser.Email,
            personas.AdminUser.Email,
            personas.ContentManagerUser.Email,
            personas.TeamLeader.Email,
            personas.TeamOfficer.Email,
            personas.TeamMember.Email,
            personas.MultiplayerUserA.Email,
            personas.MultiplayerUserB.Email,
            personas.NoProgressUser.Email
        }.Distinct(StringComparer.OrdinalIgnoreCase).Should().HaveCount(11);

        personas.GuestBrowserProfile.Name.Should().Be("guestBrowserProfile");

        await using var connection = new SqlConnection(_fixture.DatabaseConnectionString);
        await connection.OpenAsync();

        var premiumCount = await ExecuteScalarAsync<int>(
            connection,
            "SELECT COUNT(*) FROM [Users] WHERE [Email] = @email AND [Premium_IsPremium] = 1",
            new SqlParameter("@email", personas.PremiumUser.Email));
        premiumCount.Should().Be(1);

        var lockedCount = await ExecuteScalarAsync<int>(
            connection,
            "SELECT COUNT(*) FROM [Users] WHERE [Email] = @email AND [LockoutEnd] > SYSUTCDATETIME()",
            new SqlParameter("@email", personas.LockedOutUser.Email));
        lockedCount.Should().Be(1);

        var adminRoleCount = await ExecuteScalarAsync<int>(
            connection,
            """
            SELECT COUNT(*)
            FROM [AdminRoleAssignments] ar
            INNER JOIN [Users] u ON u.[Id] = ar.[UserId]
            WHERE u.[Email] = @email AND ar.[Role] = 'Admin'
            """,
            new SqlParameter("@email", personas.AdminUser.Email));
        adminRoleCount.Should().Be(1);

        var contentRoleCount = await ExecuteScalarAsync<int>(
            connection,
            """
            SELECT COUNT(*)
            FROM [AdminRoleAssignments] ar
            INNER JOIN [Users] u ON u.[Id] = ar.[UserId]
            WHERE u.[Email] = @email AND ar.[Role] = 'ContentManager'
            """,
            new SqlParameter("@email", personas.ContentManagerUser.Email));
        contentRoleCount.Should().Be(1);

        var teamRoleCount = await ExecuteScalarAsync<int>(
            connection,
            """
            SELECT COUNT(*)
            FROM [TeamMembers]
            WHERE [TeamId] = @teamId
              AND [Role] IN ('Leader', 'Officer', 'Member')
            """,
            new SqlParameter("@teamId", personas.TeamId));
        teamRoleCount.Should().Be(3);

        var todayChallengeCount = await ExecuteScalarAsync<int>(
            connection,
            """
            SELECT COUNT(*)
            FROM [DailyChallenges]
            WHERE CONVERT(date, [Date]) = CONVERT(date, SYSUTCDATETIME())
            """);
        todayChallengeCount.Should().Be(1);

        var activeLeagueTierCount = await ExecuteScalarAsync<int>(
            connection,
            """
            SELECT COUNT(DISTINCT [Tier])
            FROM [Leagues]
            WHERE [IsActive] = 1
            """);
        activeLeagueTierCount.Should().Be(5);

        var userAStats = await _fixture.GetUserStatsViaApiAsync(personas.MultiplayerUserA);
        var userBStats = await _fixture.GetUserStatsViaApiAsync(personas.MultiplayerUserB);
        var noProgressStats = await _fixture.GetUserStatsViaApiAsync(personas.NoProgressUser);

        userAStats.CurrentLevel.Should().Be(4);
        userBStats.CurrentLevel.Should().Be(4);
        noProgressStats.TotalXP.Should().Be(0);
        noProgressStats.TotalWordsSolved.Should().Be(0);
    }

    [Fact]
    public void ScreenshotApprovalManifest_ReferencesOnlyExistingPngBaselines()
    {
        var approved = ScreenshotApprovalManifest.GetApprovedRelativePaths();

        approved.Should().OnlyContain(path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase));

        foreach (var relativePath in approved)
        {
            var baselinePath = Path.Combine(
                RepositoryPaths.E2EApprovedScreenshots,
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            File.Exists(baselinePath).Should().BeTrue($"approved screenshot baseline must exist: {relativePath}");
        }
    }

    private static async Task<T> ExecuteScalarAsync<T>(
        SqlConnection connection,
        string sql,
        params SqlParameter[] parameters)
    {
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        var result = await command.ExecuteScalarAsync();
        result.Should().NotBeNull();
        return (T)Convert.ChangeType(result, typeof(T));
    }
}
