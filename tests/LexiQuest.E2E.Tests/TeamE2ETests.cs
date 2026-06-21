using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.DTOs.Shop;
using LexiQuest.Shared.DTOs.Teams;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class TeamE2ETests : E2ETestBase
{
    public TeamE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Teams_NoTeamState_ShowsEmptyActions()
    {
        await RunScenarioAsync("teams", "no-team-state", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("teamnone");

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            using var apiResponse = await apiClient.GetAsync("api/v1/teams");
            apiResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.EmptyState)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.EmptyTitle)).ToContainTextAsync("Nemáš tým");
            await Expect(page.GetByTestId(Selectors.Teams.EmptyDescription)).ToContainTextAsync("Vytvoř si tým nebo se připoj k existujícímu");
            await Expect(page.GetByTestId(Selectors.Teams.CreateTeam)).ToContainTextAsync("Vytvořit tým");
            await Expect(page.GetByTestId(Selectors.Teams.SearchTeam)).ToContainTextAsync("Hledat tým");
            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "no-team-state",
                state: "empty-state",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_PremiumUser_CreatesTeamForFree()
    {
        await RunScenarioAsync("teams", "premium-create-free", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("teampremium");
            await Fixture.ForceUserPremiumAsync(user.Email);
            await Fixture.ForceUserCoinsAsync(user.Email, 0);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.EmptyState)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId(Selectors.Teams.CreateTeam).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Teams.CreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.CreateCost)).ToContainTextAsync("Zdarma pro Premium");

            await page.GetByTestId(Selectors.Teams.CreateName).FillAsync("Prémiový tým");
            await page.GetByTestId(Selectors.Teams.CreateTag).FillAsync("VIP1");
            await page.GetByTestId(Selectors.Teams.CreateDescription).FillAsync("Tým vytvořený přes E2E test.");
            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.DashboardName)).ToContainTextAsync("Prémiový tým");
            await Expect(page.GetByTestId(Selectors.Teams.DashboardTag)).ToContainTextAsync("VIP1");
            await Expect(page.GetByTestId(Selectors.Teams.MembersTable)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.MemberRow)).ToContainTextAsync(user.Username);
            await Expect(page.GetByTestId(Selectors.Teams.MemberRow)).ToContainTextAsync("Vůdce");

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var team = await apiClient.GetFromJsonAsync<LexiQuest.Shared.DTOs.Teams.TeamDto>("api/v1/teams");
            team.Should().NotBeNull();
            team!.Name.Should().Be("Prémiový tým");
            team.Tag.Should().Be("VIP1");
            team.MemberCount.Should().Be(1);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "premium-create-free",
                state: "dashboard-after-create",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_FreeUser_CreatesTeamForCoinsAndDeductsBalance()
    {
        await RunScenarioAsync("teams", "free-create-coins", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("teamcoins");
            await Fixture.ForceUserCoinsAsync(user.Email, 1_000);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.EmptyState)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId(Selectors.Teams.CreateTeam).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Teams.CreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.CreateCost)).ToContainTextAsync("1000 mincí");
            await Expect(page.GetByTestId(Selectors.Teams.CreateCost)).ToContainTextAsync("1000");

            await page.GetByTestId(Selectors.Teams.CreateName).FillAsync("Mincový tým");
            await page.GetByTestId(Selectors.Teams.CreateTag).FillAsync("COIN");
            await page.GetByTestId(Selectors.Teams.CreateDescription).FillAsync("Tým zaplacený mincemi.");
            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.DashboardName)).ToContainTextAsync("Mincový tým");
            await Expect(page.GetByTestId(Selectors.Teams.DashboardTag)).ToContainTextAsync("COIN");

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var coins = await apiClient.GetFromJsonAsync<CoinBalanceDto>("api/v1/shop/coins");
            coins.Should().NotBeNull();
            coins!.Balance.Should().Be(0);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "free-create-coins",
                state: "dashboard-after-coin-create",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_FreeUserWithoutCoins_CreateTeamIsRejected()
    {
        await RunScenarioAsync("teams", "free-create-insufficient-coins", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("teamnocoin");
            await Fixture.ForceUserCoinsAsync(user.Email, 0);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.EmptyState)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId(Selectors.Teams.CreateTeam).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Teams.CreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.CreateCost)).ToContainTextAsync("1000 mincí");
            await Expect(page.GetByTestId(Selectors.Teams.CreateCost)).ToContainTextAsync("0");

            await page.GetByTestId(Selectors.Teams.CreateName).FillAsync("Bez mincí");
            await page.GetByTestId(Selectors.Teams.CreateTag).FillAsync("NOPE");
            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Teams.CreateError)).ToContainTextAsync("Premium nebo 1000 mincí", new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).Not.ToBeVisibleAsync();

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            using var apiResponse = await apiClient.GetAsync("api/v1/teams");
            apiResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var coins = await apiClient.GetFromJsonAsync<CoinBalanceDto>("api/v1/shop/coins");
            coins.Should().NotBeNull();
            coins!.Balance.Should().Be(0);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "free-create-insufficient-coins",
                state: "modal-error",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_CreateTeamNameValidation_RequiresThreeToThirtyCharacters()
    {
        await RunScenarioAsync("teams", "create-name-validation", async page =>
        {
            var maxNameUser = await Fixture.RegisterUniqueUserAsync("teamnamemax");
            await Fixture.ForceUserPremiumAsync(maxNameUser.Email);
            using var maxNameApi = await Fixture.CreateAuthenticatedApiClientAsync(maxNameUser);

            var maxName = new string('M', 30);
            using var maxNameResponse = await maxNameApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest(maxName, "M30", null, null));
            maxNameResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var maxTeam = await maxNameResponse.Content.ReadFromJsonAsync<TeamDto>();
            maxTeam.Should().NotBeNull();
            maxTeam!.Name.Should().Be(maxName);

            var user = await Fixture.RegisterUniqueUserAsync("teamnameui");
            await Fixture.ForceUserPremiumAsync(user.Email);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await page.GetByTestId(Selectors.Teams.CreateTeam).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.CreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.CreateNameError)).ToContainTextAsync("Název týmu je povinný");

            await page.GetByTestId(Selectors.Teams.CreateName).FillAsync("ab");
            await page.GetByTestId(Selectors.Teams.CreateTag).FillAsync("NM1");
            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.CreateNameError)).ToContainTextAsync("3 až 30 znaků");

            await page.GetByTestId(Selectors.Teams.CreateName).FillAsync(new string('A', 31));
            var clippedName = await page.GetByTestId(Selectors.Teams.CreateName).InputValueAsync();
            clippedName.Should().HaveLength(30);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "create-name-validation",
                state: "name-length-error",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser");

            await page.GetByTestId(Selectors.Teams.CreateName).FillAsync("Tým");
            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.DashboardName)).ToContainTextAsync("Tým");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_CreateTeamTagValidation_RequiresTwoToFourUppercaseAlphanumericCharacters()
    {
        await RunScenarioAsync("teams", "create-tag-validation", async page =>
        {
            var maxTagUser = await Fixture.RegisterUniqueUserAsync("teamtagmax");
            await Fixture.ForceUserPremiumAsync(maxTagUser.Email);
            using var maxTagApi = await Fixture.CreateAuthenticatedApiClientAsync(maxTagUser);

            using var maxTagResponse = await maxTagApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Tag Maximum", "T4G9", null, null));
            maxTagResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var maxTagTeam = await maxTagResponse.Content.ReadFromJsonAsync<TeamDto>();
            maxTagTeam.Should().NotBeNull();
            maxTagTeam!.Tag.Should().Be("T4G9");

            var user = await Fixture.RegisterUniqueUserAsync("teamtagui");
            await Fixture.ForceUserPremiumAsync(user.Email);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await page.GetByTestId(Selectors.Teams.CreateTeam).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.CreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId(Selectors.Teams.CreateName).FillAsync("Tag Test");

            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.CreateTagError)).ToContainTextAsync("Tag týmu je povinný");

            await page.GetByTestId(Selectors.Teams.CreateTag).FillAsync("A");
            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.CreateTagError)).ToContainTextAsync("2 až 4 znaky");

            await page.GetByTestId(Selectors.Teams.CreateTag).FillAsync("ab");
            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.CreateTagError)).ToContainTextAsync("velká písmena A-Z");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "create-tag-validation",
                state: "tag-format-error",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser");

            await page.GetByTestId(Selectors.Teams.CreateTag).FillAsync("OK");
            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.DashboardTag)).ToContainTextAsync("OK");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_CreateTeamDuplicateNameAndTag_ShowSpecificErrors()
    {
        await RunScenarioAsync("teams", "create-duplicate-name-tag", async page =>
        {
            var existingOwner = await Fixture.RegisterUniqueUserAsync("teamdupeowner");
            await Fixture.ForceUserPremiumAsync(existingOwner.Email);
            using var ownerApi = await Fixture.CreateAuthenticatedApiClientAsync(existingOwner);

            using var existingResponse = await ownerApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Duplicitní tým", "DUPE", null, null));
            existingResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var user = await Fixture.RegisterUniqueUserAsync("teamdupeui");
            await Fixture.ForceUserPremiumAsync(user.Email);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await page.GetByTestId(Selectors.Teams.CreateTeam).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.CreateModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await page.GetByTestId(Selectors.Teams.CreateName).FillAsync("Duplicitní tým");
            await page.GetByTestId(Selectors.Teams.CreateTag).FillAsync("UNIQ");
            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.CreateError)).ToContainTextAsync("názvem už existuje", new() { Timeout = 10_000 });

            await page.GetByTestId(Selectors.Teams.CreateName).FillAsync("Jiný tým");
            await page.GetByTestId(Selectors.Teams.CreateTag).FillAsync("DUPE");
            await page.GetByTestId(Selectors.Teams.CreateSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.CreateError)).ToContainTextAsync("tagem už existuje", new() { Timeout = 10_000 });

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).Not.ToBeVisibleAsync();

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            using var myTeamResponse = await apiClient.GetAsync("api/v1/teams");
            myTeamResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "create-duplicate-name-tag",
                state: "duplicate-tag-error",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_Dashboard_ShowsStatsDescriptionMembersAndRoles()
    {
        await RunScenarioAsync("teams", "dashboard-stats-members", async page =>
        {
            var leader = await Fixture.RegisterUniqueUserAsync("teamdashlead");
            var officer = await Fixture.RegisterUniqueUserAsync("teamdashoff");
            var member = await Fixture.RegisterUniqueUserAsync("teamdashmem");
            await Fixture.ForceUserPremiumAsync(leader.Email);

            using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
            using var createResponse = await leaderApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Dashboard tým", "DASH", "Týmový dashboard pro E2E audit.", null));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdTeam = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
            createdTeam.Should().NotBeNull();

            await Fixture.SeedTeamMemberAsync(createdTeam!.Id, leader.Email, TeamRole.Leader, weeklyXp: 150, allTimeXp: 1_200, wins: 2);
            await Fixture.SeedTeamMemberAsync(createdTeam.Id, officer.Email, TeamRole.Officer, weeklyXp: 80, allTimeXp: 900, wins: 1);
            await Fixture.SeedTeamMemberAsync(createdTeam.Id, member.Email, TeamRole.Member, weeklyXp: 20, allTimeXp: 300, wins: 0);

            var team = await leaderApi.GetFromJsonAsync<TeamDto>("api/v1/teams");
            team.Should().NotBeNull();
            team!.MemberCount.Should().Be(3);
            team.Stats.WeeklyXP.Should().Be(250);
            team.Stats.AllTimeXP.Should().Be(2_400);
            team.Stats.TotalWins.Should().Be(3);

            var members = await leaderApi.GetFromJsonAsync<List<TeamMemberDto>>($"api/v1/teams/{team.Id}/members");
            members.Should().NotBeNull();
            members!.Should().HaveCount(3);
            members.Should().Contain(m => m.Username == leader.Username && m.Role == TeamRoleDto.Leader && m.WeeklyXP == 150);
            members.Should().Contain(m => m.Username == officer.Username && m.Role == TeamRoleDto.Officer && m.WeeklyXP == 80);
            members.Should().Contain(m => m.Username == member.Username && m.Role == TeamRoleDto.Member && m.WeeklyXP == 20);

            await Fixture.LoginAsAsync(page, leader);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.DashboardName)).ToContainTextAsync("Dashboard tým");
            await Expect(page.GetByTestId(Selectors.Teams.DashboardTag)).ToContainTextAsync("DASH");
            await Expect(page.GetByTestId(Selectors.Teams.DashboardDescription)).ToContainTextAsync("Týmový dashboard pro E2E audit.");
            await Expect(page.GetByTestId(Selectors.Teams.StatsWeeklyXp)).ToContainTextAsync("250");
            await Expect(page.GetByTestId(Selectors.Teams.StatsAllTimeXp)).ToContainTextAsync("2400");
            await Expect(page.GetByTestId(Selectors.Teams.StatsRank)).ToContainTextAsync("#0");
            await Expect(page.GetByTestId(Selectors.Teams.StatsWins)).ToContainTextAsync("3");

            var rows = page.GetByTestId(Selectors.Teams.MemberRow);
            await Expect(rows).ToHaveCountAsync(3);

            var leaderRow = rows.Filter(new() { HasTextString = leader.Username });
            await Expect(leaderRow).ToContainTextAsync("Vůdce");
            await Expect(leaderRow).ToContainTextAsync("150");

            var officerRow = rows.Filter(new() { HasTextString = officer.Username });
            await Expect(officerRow).ToContainTextAsync("Důstojník");
            await Expect(officerRow).ToContainTextAsync("80");

            var memberRow = rows.Filter(new() { HasTextString = member.Username });
            await Expect(memberRow).ToContainTextAsync("Člen");
            await Expect(memberRow).ToContainTextAsync("20");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "dashboard-stats-members",
                state: "dashboard",
                viewport: "1366x900",
                theme: "light",
                persona: "teamLeader");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_LeaderAndOfficer_CanInviteMemberByUsername()
    {
        await RunScenarioAsync("teams", "invite-member-leader-officer", async page =>
        {
            var leader = await Fixture.RegisterUniqueUserAsync("teaminvlead");
            var officer = await Fixture.RegisterUniqueUserAsync("teaminvoff");
            var leaderInvitee = await Fixture.RegisterUniqueUserAsync("teaminvfirst");
            var officerInvitee = await Fixture.RegisterUniqueUserAsync("teaminvsecond");
            await Fixture.ForceUserPremiumAsync(leader.Email);

            using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
            using var createResponse = await leaderApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Pozvánkový tým", "INVT", "Tým pro ověření pozvánek.", null));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
            team.Should().NotBeNull();

            await Fixture.SeedTeamMemberAsync(team!.Id, leader.Email, TeamRole.Leader, weeklyXp: 40, allTimeXp: 100, wins: 1);
            await Fixture.SeedTeamMemberAsync(team.Id, officer.Email, TeamRole.Officer, weeklyXp: 30, allTimeXp: 80, wins: 0);

            await Fixture.LoginAsAsync(page, leader);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");
            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.InviteOpen)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Teams.InviteOpen).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.InviteModal)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Teams.InviteUsername).FillAsync(leaderInvitee.Username);
            await page.GetByTestId(Selectors.Teams.InviteSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.InviteSuccess)).ToContainTextAsync("Pozvánka odeslána", new() { Timeout = 10_000 });

            using var leaderInviteeApi = await Fixture.CreateAuthenticatedApiClientAsync(leaderInvitee);
            var leaderInviteeInvites = await leaderInviteeApi.GetFromJsonAsync<List<TeamInviteDto>>("api/v1/teams/invites/my");
            leaderInviteeInvites.Should().NotBeNull();
            leaderInviteeInvites!.Should().ContainSingle(i => i.TeamTag == "INVT" && i.TeamName == "Pozvánkový tým");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "invite-member-leader-officer",
                state: "leader-invite-success",
                viewport: "1366x900",
                theme: "light",
                persona: "teamLeader");

            await Fixture.LoginAsAsync(page, officer);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");
            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.InviteOpen)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Teams.InviteOpen).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.InviteModal)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Teams.InviteUsername).FillAsync(officerInvitee.Username);
            await page.GetByTestId(Selectors.Teams.InviteSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.InviteSuccess)).ToContainTextAsync("Pozvánka odeslána", new() { Timeout = 10_000 });

            using var officerInviteeApi = await Fixture.CreateAuthenticatedApiClientAsync(officerInvitee);
            var officerInviteeInvites = await officerInviteeApi.GetFromJsonAsync<List<TeamInviteDto>>("api/v1/teams/invites/my");
            officerInviteeInvites.Should().NotBeNull();
            officerInviteeInvites!.Should().ContainSingle(i => i.TeamTag == "INVT" && i.TeamName == "Pozvánkový tým");

            await Fixture.LoginAsAsync(page, officerInvitee);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");
            await Expect(page.GetByTestId(Selectors.Teams.EmptyState)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.InvitesSection)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.InviteRow)).ToContainTextAsync("Pozvánkový tým");
            await Expect(page.GetByTestId(Selectors.Teams.InviteRow)).ToContainTextAsync("INVT");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "invite-member-leader-officer",
                state: "invitee-visible-invite",
                viewport: "1366x900",
                theme: "light",
                persona: "teamInvitee");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_RegularMember_CannotInviteMember()
    {
        await RunScenarioAsync("teams", "regular-member-invite-rejected", async page =>
        {
            var leader = await Fixture.RegisterUniqueUserAsync("teamreglead");
            var regularMember = await Fixture.RegisterUniqueUserAsync("teamregmem");
            var invitee = await Fixture.RegisterUniqueUserAsync("teamreginvitee");
            await Fixture.ForceUserPremiumAsync(leader.Email);

            using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
            using var createResponse = await leaderApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Členský tým", "MEMB", "Tým pro ověření práv člena.", null));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
            team.Should().NotBeNull();

            await Fixture.SeedTeamMemberAsync(team!.Id, leader.Email, TeamRole.Leader, weeklyXp: 20, allTimeXp: 60);
            await Fixture.SeedTeamMemberAsync(team.Id, regularMember.Email, TeamRole.Member, weeklyXp: 10, allTimeXp: 30);

            await Fixture.LoginAsAsync(page, regularMember);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.InviteOpen)).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("Předat vedení")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("Rozpustit tým")).Not.ToBeVisibleAsync();

            using var memberApi = await Fixture.CreateAuthenticatedApiClientAsync(regularMember);
            using var rejectedInvite = await memberApi.PostAsJsonAsync(
                $"api/v1/teams/{team.Id}/invite-by-username",
                new InviteMemberByUsernameRequest(invitee.Username));
            rejectedInvite.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var rejectedBody = await rejectedInvite.Content.ReadAsStringAsync();
            rejectedBody.Should().Contain("Only team leaders and officers");

            using var inviteeApi = await Fixture.CreateAuthenticatedApiClientAsync(invitee);
            var inviteeInvites = await inviteeApi.GetFromJsonAsync<List<TeamInviteDto>>("api/v1/teams/invites/my");
            inviteeInvites.Should().NotBeNull();
            inviteeInvites.Should().BeEmpty();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "regular-member-invite-rejected",
                state: "member-dashboard-no-invite",
                viewport: "1366x900",
                theme: "light",
                persona: "teamMember");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_NoTeamUser_CanCreateJoinRequestFromRanking()
    {
        await RunScenarioAsync("teams", "join-request-from-ranking", async page =>
        {
            var leader = await Fixture.RegisterUniqueUserAsync("teamjoinlead");
            var applicant = await Fixture.RegisterUniqueUserAsync("teamjoinapp");
            await Fixture.ForceUserPremiumAsync(leader.Email);

            using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
            using var createResponse = await leaderApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Žádostní tým", "JOIN", "Tým otevřený žádostem.", null));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
            team.Should().NotBeNull();
            await Fixture.SeedTeamMemberAsync(team!.Id, leader.Email, TeamRole.Leader, weeklyXp: 90, allTimeXp: 200, wins: 2);

            await Fixture.LoginAsAsync(page, applicant);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.EmptyState)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId(Selectors.Teams.SearchTeam).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.RankingTable)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var rankingRow = page.GetByTestId(Selectors.Teams.RankingRow).Filter(new() { HasTextString = "Žádostní tým" });
            await Expect(rankingRow).ToContainTextAsync("JOIN");

            await rankingRow.GetByTestId(Selectors.Teams.JoinRequestOpen).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.JoinModal)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Teams.JoinMessage).FillAsync("Rád bych se přidal do týmu.");
            await page.GetByTestId(Selectors.Teams.JoinSubmit).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.JoinSuccess)).ToContainTextAsync("Žádost odeslána", new() { Timeout = 10_000 });

            var requests = await leaderApi.GetFromJsonAsync<List<TeamJoinRequestDto>>($"api/v1/teams/{team.Id}/join-requests");
            requests.Should().NotBeNull();
            requests!.Should().ContainSingle(r =>
                r.Username == applicant.Username &&
                r.TeamId == team.Id &&
                r.Status == JoinRequestStatusDto.Pending &&
                r.Message == "Rád bych se přidal do týmu.");

            using var applicantApi = await Fixture.CreateAuthenticatedApiClientAsync(applicant);
            using var myTeamResponse = await applicantApi.GetAsync("api/v1/teams");
            myTeamResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "join-request-from-ranking",
                state: "join-request-success",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_Leader_CanApproveAndRejectJoinRequests()
    {
        await RunScenarioAsync("teams", "approve-reject-join-requests", async page =>
        {
            var leader = await Fixture.RegisterUniqueUserAsync("teamreqlead");
            var approveUser = await Fixture.RegisterUniqueUserAsync("teamreqok");
            var rejectUser = await Fixture.RegisterUniqueUserAsync("teamreqno");
            await Fixture.ForceUserPremiumAsync(leader.Email);

            using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
            using var createResponse = await leaderApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Schvalovací tým", "REQS", "Tým pro žádosti.", null));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
            team.Should().NotBeNull();
            await Fixture.SeedTeamMemberAsync(team!.Id, leader.Email, TeamRole.Leader, weeklyXp: 50, allTimeXp: 150);

            using var approveApi = await Fixture.CreateAuthenticatedApiClientAsync(approveUser);
            using var approveRequest = await approveApi.PostAsJsonAsync(
                $"api/v1/teams/{team.Id}/join-request",
                new CreateJoinRequest("Prosím o schválení."));
            approveRequest.EnsureSuccessStatusCode();

            using var rejectApi = await Fixture.CreateAuthenticatedApiClientAsync(rejectUser);
            using var rejectRequest = await rejectApi.PostAsJsonAsync(
                $"api/v1/teams/{team.Id}/join-request",
                new CreateJoinRequest("Tohle bude zamítnuto."));
            rejectRequest.EnsureSuccessStatusCode();

            await Fixture.LoginAsAsync(page, leader);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.JoinRequestsSection)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var approveRow = page.GetByTestId(Selectors.Teams.JoinRequestRow).Filter(new() { HasTextString = approveUser.Username });
            await Expect(approveRow).ToContainTextAsync("Prosím o schválení.");
            var rejectRow = page.GetByTestId(Selectors.Teams.JoinRequestRow).Filter(new() { HasTextString = rejectUser.Username });
            await Expect(rejectRow).ToContainTextAsync("Tohle bude zamítnuto.");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "approve-reject-join-requests",
                state: "pending-requests",
                viewport: "1366x900",
                theme: "light",
                persona: "teamLeader");

            await approveRow.GetByTestId(Selectors.Teams.JoinRequestAccept).ClickAsync();
            var approvedMemberRow = page.GetByTestId(Selectors.Teams.MemberRow).Filter(new() { HasTextString = approveUser.Username });
            await Expect(approvedMemberRow).ToContainTextAsync("Člen", new() { Timeout = 10_000 });

            rejectRow = page.GetByTestId(Selectors.Teams.JoinRequestRow).Filter(new() { HasTextString = rejectUser.Username });
            await rejectRow.GetByTestId(Selectors.Teams.JoinRequestReject).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.JoinRequestsSection)).Not.ToBeVisibleAsync(new() { Timeout = 10_000 });

            var approvedTeam = await approveApi.GetFromJsonAsync<TeamDto>("api/v1/teams");
            approvedTeam.Should().NotBeNull();
            approvedTeam!.Id.Should().Be(team.Id);

            using var rejectedTeamResponse = await rejectApi.GetAsync("api/v1/teams");
            rejectedTeamResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "approve-reject-join-requests",
                state: "after-approve-reject",
                viewport: "1366x900",
                theme: "light",
                persona: "teamLeader");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_Officer_CanKickRegularMember()
    {
        await RunScenarioAsync("teams", "officer-kick-member", async page =>
        {
            var leader = await Fixture.RegisterUniqueUserAsync("teamkicklead");
            var officer = await Fixture.RegisterUniqueUserAsync("teamkickoff");
            var member = await Fixture.RegisterUniqueUserAsync("teamkickmem");
            await Fixture.ForceUserPremiumAsync(leader.Email);

            using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
            using var createResponse = await leaderApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Vyhazovací tým", "KICK", "Tým pro kontrolu vyhození.", null));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
            team.Should().NotBeNull();
            await Fixture.SeedTeamMemberAsync(team!.Id, leader.Email, TeamRole.Leader, weeklyXp: 120, allTimeXp: 240);
            await Fixture.SeedTeamMemberAsync(team.Id, officer.Email, TeamRole.Officer, weeklyXp: 80, allTimeXp: 160);
            await Fixture.SeedTeamMemberAsync(team.Id, member.Email, TeamRole.Member, weeklyXp: 30, allTimeXp: 60);

            await Fixture.LoginAsAsync(page, officer);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var targetRow = page.GetByTestId(Selectors.Teams.MemberRow).Filter(new() { HasTextString = member.Username });
            await Expect(targetRow).ToContainTextAsync("Člen");
            await Expect(page.GetByTestId(Selectors.Teams.MemberRow).Filter(new() { HasTextString = leader.Username })).ToContainTextAsync("Vůdce");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "officer-kick-member",
                state: "before-kick",
                viewport: "1366x900",
                theme: "light",
                persona: "teamOfficer");

            await targetRow.GetByTestId(Selectors.Teams.MemberKick).ClickAsync();
            await Expect(targetRow).Not.ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.MemberRow)).ToHaveCountAsync(2);
            await Expect(page.GetByTestId(Selectors.Teams.MembersHeading)).ToContainTextAsync("(2/20)");
            await Expect(page.GetByTestId(Selectors.Teams.StatsWeeklyXp)).ToHaveTextAsync("200");
            await Expect(page.GetByTestId(Selectors.Teams.StatsAllTimeXp)).ToHaveTextAsync("400");

            var membersAfterKick = await leaderApi.GetFromJsonAsync<List<TeamMemberDto>>($"api/v1/teams/{team.Id}/members");
            membersAfterKick.Should().NotBeNull();
            membersAfterKick!.Should().Contain(m => m.Username == leader.Username && m.Role == TeamRoleDto.Leader);
            membersAfterKick.Should().Contain(m => m.Username == officer.Username && m.Role == TeamRoleDto.Officer);
            membersAfterKick.Should().NotContain(m => m.Username == member.Username);

            using var kickedMemberApi = await Fixture.CreateAuthenticatedApiClientAsync(member);
            using var kickedMemberTeamResponse = await kickedMemberApi.GetAsync("api/v1/teams");
            kickedMemberTeamResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "officer-kick-member",
                state: "after-kick",
                viewport: "1366x900",
                theme: "light",
                persona: "teamOfficer");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_LeaderCannotBeKicked()
    {
        await RunScenarioAsync("teams", "leader-cannot-be-kicked", async page =>
        {
            var leader = await Fixture.RegisterUniqueUserAsync("teamnoleadkick");
            var officer = await Fixture.RegisterUniqueUserAsync("teamnoleadoff");
            await Fixture.ForceUserPremiumAsync(leader.Email);

            using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
            using var createResponse = await leaderApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Chráněný tým", "SAFE", "Tým kontroluje ochranu vůdce.", null));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
            team.Should().NotBeNull();
            await Fixture.SeedTeamMemberAsync(team!.Id, leader.Email, TeamRole.Leader, weeklyXp: 100, allTimeXp: 500);
            await Fixture.SeedTeamMemberAsync(team.Id, officer.Email, TeamRole.Officer, weeklyXp: 70, allTimeXp: 200);

            await Fixture.LoginAsAsync(page, officer);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var leaderRow = page.GetByTestId(Selectors.Teams.MemberRow).Filter(new() { HasTextString = leader.Username });
            await Expect(leaderRow).ToContainTextAsync("Vůdce");
            await Expect(leaderRow.GetByTestId(Selectors.Teams.MemberKick)).ToHaveCountAsync(0);

            using var officerApi = await Fixture.CreateAuthenticatedApiClientAsync(officer);
            using var officerKickLeaderResponse = await officerApi.PostAsync($"api/v1/teams/{team.Id}/kick/{team.LeaderId}", null);
            officerKickLeaderResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var officerKickLeaderError = await officerKickLeaderResponse.Content.ReadAsStringAsync();
            officerKickLeaderError.ToLowerInvariant().Should().Contain("officers can only kick regular members");

            using var leaderKickSelfResponse = await leaderApi.PostAsync($"api/v1/teams/{team.Id}/kick/{team.LeaderId}", null);
            leaderKickSelfResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var leaderKickSelfError = await leaderKickSelfResponse.Content.ReadAsStringAsync();
            leaderKickSelfError.ToLowerInvariant().Should().Contain("leader cannot be removed");

            var membersAfterAttempts = await leaderApi.GetFromJsonAsync<List<TeamMemberDto>>($"api/v1/teams/{team.Id}/members");
            membersAfterAttempts.Should().NotBeNull();
            membersAfterAttempts!.Should().Contain(m => m.Username == leader.Username && m.Role == TeamRoleDto.Leader);
            membersAfterAttempts.Should().Contain(m => m.Username == officer.Username && m.Role == TeamRoleDto.Officer);
            membersAfterAttempts.Should().HaveCount(2);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "leader-cannot-be-kicked",
                state: "leader-protected",
                viewport: "1366x900",
                theme: "light",
                persona: "teamOfficer");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_MemberCanLeaveTeam()
    {
        await RunScenarioAsync("teams", "member-leave-team", async page =>
        {
            var leader = await Fixture.RegisterUniqueUserAsync("teamleavelead");
            var member = await Fixture.RegisterUniqueUserAsync("teamleavemem");
            await Fixture.ForceUserPremiumAsync(leader.Email);

            using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
            using var createResponse = await leaderApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Odchodový tým", "LEAV", "Tým pro kontrolu odchodu.", null));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
            team.Should().NotBeNull();
            await Fixture.SeedTeamMemberAsync(team!.Id, leader.Email, TeamRole.Leader, weeklyXp: 100, allTimeXp: 500);
            await Fixture.SeedTeamMemberAsync(team.Id, member.Email, TeamRole.Member, weeklyXp: 40, allTimeXp: 80);

            await Fixture.LoginAsAsync(page, member);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.MembersHeading)).ToContainTextAsync("(2/20)");
            await Expect(page.GetByTestId(Selectors.Teams.MemberRow).Filter(new() { HasTextString = member.Username })).ToContainTextAsync("Člen");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "member-leave-team",
                state: "before-leave",
                viewport: "1366x900",
                theme: "light",
                persona: "teamMember");

            await page.GetByTestId(Selectors.Teams.LeaveTeam).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.EmptyState)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).Not.ToBeVisibleAsync();

            using var memberApi = await Fixture.CreateAuthenticatedApiClientAsync(member);
            using var memberTeamResponse = await memberApi.GetAsync("api/v1/teams");
            memberTeamResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var membersAfterLeave = await leaderApi.GetFromJsonAsync<List<TeamMemberDto>>($"api/v1/teams/{team.Id}/members");
            membersAfterLeave.Should().NotBeNull();
            membersAfterLeave!.Should().ContainSingle(m => m.Username == leader.Username && m.Role == TeamRoleDto.Leader);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "member-leave-team",
                state: "after-leave-empty",
                viewport: "1366x900",
                theme: "light",
                persona: "teamMember");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_LastMemberDisbandsTeam()
    {
        await RunScenarioAsync("teams", "last-member-disbands-team", async page =>
        {
            var leader = await Fixture.RegisterUniqueUserAsync("teamlastlead");
            await Fixture.ForceUserPremiumAsync(leader.Email);

            using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
            using var createResponse = await leaderApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Solo tým", "SOLO", "Tým s posledním členem.", null));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
            team.Should().NotBeNull();
            await Fixture.SeedTeamMemberAsync(team!.Id, leader.Email, TeamRole.Leader, weeklyXp: 90, allTimeXp: 180);

            await Fixture.LoginAsAsync(page, leader);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.MembersHeading)).ToContainTextAsync("(1/20)");
            await Expect(page.GetByTestId(Selectors.Teams.MemberRow)).ToHaveCountAsync(1);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "last-member-disbands-team",
                state: "before-disband",
                viewport: "1366x900",
                theme: "light",
                persona: "teamLeader");

            await page.GetByTestId(Selectors.Teams.DisbandTeam).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.EmptyState)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            using var myTeamResponse = await leaderApi.GetAsync("api/v1/teams");
            myTeamResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            using var deletedTeamResponse = await leaderApi.GetAsync($"api/v1/teams/{team.Id}");
            deletedTeamResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "last-member-disbands-team",
                state: "after-disband-empty",
                viewport: "1366x900",
                theme: "light",
                persona: "teamLeader");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_LeaderCanTransferLeadershipToMember()
    {
        await RunScenarioAsync("teams", "transfer-leadership", async page =>
        {
            var leader = await Fixture.RegisterUniqueUserAsync("teamtransferlead");
            var officer = await Fixture.RegisterUniqueUserAsync("teamtransferoff");
            var member = await Fixture.RegisterUniqueUserAsync("teamtransfermem");
            await Fixture.ForceUserPremiumAsync(leader.Email);

            using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
            using var createResponse = await leaderApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Předávací tým", "TRAN", "Tým pro předání vedení.", null));
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
            team.Should().NotBeNull();
            await Fixture.SeedTeamMemberAsync(team!.Id, leader.Email, TeamRole.Leader, weeklyXp: 100, allTimeXp: 300);
            await Fixture.SeedTeamMemberAsync(team.Id, officer.Email, TeamRole.Officer, weeklyXp: 70, allTimeXp: 200);
            await Fixture.SeedTeamMemberAsync(team.Id, member.Email, TeamRole.Member, weeklyXp: 30, allTimeXp: 90);

            await Fixture.LoginAsAsync(page, leader);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");

            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.TransferOpen)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Teams.TransferOpen).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Teams.TransferModal)).ToBeVisibleAsync();
            var officerOption = page.GetByTestId(Selectors.Teams.TransferMember).Filter(new() { HasTextString = officer.Username });
            await Expect(officerOption).ToContainTextAsync("Důstojník");
            var memberOption = page.GetByTestId(Selectors.Teams.TransferMember).Filter(new() { HasTextString = member.Username });
            await Expect(memberOption).ToContainTextAsync("Člen");
            await Expect(page.GetByTestId(Selectors.Teams.TransferMember).Filter(new() { HasTextString = leader.Username })).ToHaveCountAsync(0);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "transfer-leadership",
                state: "modal-open",
                viewport: "1366x900",
                theme: "light",
                persona: "teamLeader");

            await memberOption.ClickAsync();
            await page.GetByTestId(Selectors.Teams.TransferSubmit).ClickAsync();

            var newLeaderRow = page.GetByTestId(Selectors.Teams.MemberRow).Filter(new() { HasTextString = member.Username });
            await Expect(newLeaderRow).ToContainTextAsync("Vůdce", new() { Timeout = 10_000 });
            var oldLeaderRow = page.GetByTestId(Selectors.Teams.MemberRow).Filter(new() { HasTextString = leader.Username });
            await Expect(oldLeaderRow).ToContainTextAsync("Důstojník");
            await Expect(page.GetByTestId(Selectors.Teams.TransferOpen)).Not.ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.DisbandTeam)).Not.ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.LeaveTeam)).ToBeVisibleAsync();

            var teamAfterTransfer = await leaderApi.GetFromJsonAsync<TeamDto>($"api/v1/teams/{team.Id}");
            teamAfterTransfer.Should().NotBeNull();
            teamAfterTransfer!.LeaderId.Should().NotBe(team.LeaderId);

            var membersAfterTransfer = await leaderApi.GetFromJsonAsync<List<TeamMemberDto>>($"api/v1/teams/{team.Id}/members");
            membersAfterTransfer.Should().NotBeNull();
            membersAfterTransfer!.Should().Contain(m => m.Username == member.Username && m.Role == TeamRoleDto.Leader);
            membersAfterTransfer.Should().Contain(m => m.Username == leader.Username && m.Role == TeamRoleDto.Officer);
            membersAfterTransfer.Should().Contain(m => m.Username == officer.Username && m.Role == TeamRoleDto.Officer);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "transfer-leadership",
                state: "after-transfer",
                viewport: "1366x900",
                theme: "light",
                persona: "teamLeader");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_WeeklyRankingOrdersTeamsByWeeklyXp()
    {
        await RunScenarioAsync("teams", "weekly-ranking", async page =>
        {
            var lowLeader = await Fixture.RegisterUniqueUserAsync("teamranklow");
            var midLeader = await Fixture.RegisterUniqueUserAsync("teamrankmid");
            var topLeader = await Fixture.RegisterUniqueUserAsync("teamranktop");
            var topMember = await Fixture.RegisterUniqueUserAsync("teamranktopmem");
            var viewer = await Fixture.RegisterUniqueUserAsync("teamrankviewer");

            await Fixture.ForceUserPremiumAsync(lowLeader.Email);
            await Fixture.ForceUserPremiumAsync(midLeader.Email);
            await Fixture.ForceUserPremiumAsync(topLeader.Email);

            using var lowApi = await Fixture.CreateAuthenticatedApiClientAsync(lowLeader);
            using var lowCreate = await lowApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Bronzový ranking tým", "BRNZ", "Tým s nízkým týdenním XP.", null));
            lowCreate.StatusCode.Should().Be(HttpStatusCode.Created);
            var lowTeam = await lowCreate.Content.ReadFromJsonAsync<TeamDto>();
            lowTeam.Should().NotBeNull();
            await Fixture.SeedTeamMemberAsync(lowTeam!.Id, lowLeader.Email, TeamRole.Leader, weeklyXp: 25, allTimeXp: 250);

            using var midApi = await Fixture.CreateAuthenticatedApiClientAsync(midLeader);
            using var midCreate = await midApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Stříbrný ranking tým", "SILV", "Tým se středním týdenním XP.", null));
            midCreate.StatusCode.Should().Be(HttpStatusCode.Created);
            var midTeam = await midCreate.Content.ReadFromJsonAsync<TeamDto>();
            midTeam.Should().NotBeNull();
            await Fixture.SeedTeamMemberAsync(midTeam!.Id, midLeader.Email, TeamRole.Leader, weeklyXp: 150, allTimeXp: 500);

            using var topApi = await Fixture.CreateAuthenticatedApiClientAsync(topLeader);
            using var topCreate = await topApi.PostAsJsonAsync(
                "api/v1/teams",
                new CreateTeamRequest("Zlatý ranking tým", "GOLD", "Tým s nejvyšším týdenním XP.", null));
            topCreate.StatusCode.Should().Be(HttpStatusCode.Created);
            var topTeam = await topCreate.Content.ReadFromJsonAsync<TeamDto>();
            topTeam.Should().NotBeNull();
            await Fixture.SeedTeamMemberAsync(topTeam!.Id, topLeader.Email, TeamRole.Leader, weeklyXp: 220, allTimeXp: 800);
            await Fixture.SeedTeamMemberAsync(topTeam.Id, topMember.Email, TeamRole.Member, weeklyXp: 80, allTimeXp: 120);

            using var viewerApi = await Fixture.CreateAuthenticatedApiClientAsync(viewer);
            var ranking = await viewerApi.GetFromJsonAsync<List<TeamRankingDto>>("api/v1/teams/ranking");
            ranking.Should().NotBeNull();
            ranking!.Take(3).Select(team => team.Name).Should().Equal(
                "Zlatý ranking tým",
                "Stříbrný ranking tým",
                "Bronzový ranking tým");
            ranking[0].Rank.Should().Be(1);
            ranking[0].WeeklyXP.Should().Be(300);
            ranking[0].MemberCount.Should().Be(2);

            await Fixture.LoginAsAsync(page, viewer);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");
            await page.GetByTestId(Selectors.Teams.SearchTeam).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Teams.RankingTable)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var rows = page.GetByTestId(Selectors.Teams.RankingRow);
            await Expect(rows.Nth(0)).ToContainTextAsync("#1");
            await Expect(rows.Nth(0)).ToContainTextAsync("Zlatý ranking tým");
            await Expect(rows.Nth(0)).ToContainTextAsync("300");
            await Expect(rows.Nth(0)).ToContainTextAsync("2");
            await Expect(rows.Nth(1)).ToContainTextAsync("#2");
            await Expect(rows.Nth(1)).ToContainTextAsync("Stříbrný ranking tým");
            await Expect(rows.Nth(1)).ToContainTextAsync("150");
            await Expect(rows.Nth(2)).ToContainTextAsync("#3");
            await Expect(rows.Nth(2)).ToContainTextAsync("Bronzový ranking tým");
            await Expect(rows.Nth(2)).ToContainTextAsync("25");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "weekly-ranking",
                state: "ordered-by-weekly-xp",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_LeaderRole_ShowsLeaderManagementOptions()
    {
        await RunScenarioAsync("teams", "role-based-management-options-leader", async page =>
        {
            var roles = await SeedRoleTeamAsync("trl", "RLDR", "Rolový leader tým");

            await Fixture.LoginAsAsync(page, roles.Leader);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");
            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.InviteOpen)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.TransferOpen)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.DisbandTeam)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.LeaveTeam)).ToHaveCountAsync(0);
            await Expect(page.GetByTestId(Selectors.Teams.MemberKick)).ToHaveCountAsync(2);

            var kickTargets = page.GetByTestId(Selectors.Teams.MemberRow).Filter(new() { Has = page.GetByTestId(Selectors.Teams.MemberKick) });
            var kickTargetText = string.Join(" ", await kickTargets.AllTextContentsAsync());
            kickTargetText.Should().Contain(roles.Officer.Username);
            kickTargetText.Should().Contain(roles.Member.Username);
            kickTargetText.Should().NotContain(roles.Leader.Username);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "role-based-management-options",
                state: "leader-actions",
                viewport: "1366x900",
                theme: "light",
                persona: "teamLeader");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_OfficerRole_ShowsOfficerManagementOptions()
    {
        await RunScenarioAsync("teams", "role-based-management-options-officer", async page =>
        {
            var roles = await SeedRoleTeamAsync("tro", "ROFC", "Rolový officer tým");

            await Fixture.LoginAsAsync(page, roles.Officer);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");
            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.InviteOpen)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.TransferOpen)).ToHaveCountAsync(0);
            await Expect(page.GetByTestId(Selectors.Teams.DisbandTeam)).ToHaveCountAsync(0);
            await Expect(page.GetByTestId(Selectors.Teams.LeaveTeam)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.MemberKick)).ToHaveCountAsync(1);

            var kickTarget = page.GetByTestId(Selectors.Teams.MemberRow).Filter(new() { Has = page.GetByTestId(Selectors.Teams.MemberKick) });
            await Expect(kickTarget).ToContainTextAsync(roles.Member.Username);
            await Expect(kickTarget).Not.ToContainTextAsync(roles.Leader.Username);
            await Expect(kickTarget).Not.ToContainTextAsync(roles.Officer.Username);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "role-based-management-options",
                state: "officer-actions",
                viewport: "1366x900",
                theme: "light",
                persona: "teamOfficer");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Teams_MemberRole_ShowsMemberManagementOptions()
    {
        await RunScenarioAsync("teams", "role-based-management-options-member", async page =>
        {
            var roles = await SeedRoleTeamAsync("trm", "RMEM", "Rolový member tým");

            await Fixture.LoginAsAsync(page, roles.Member);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/team");
            await Expect(page.GetByTestId(Selectors.Teams.Dashboard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Teams.InviteOpen)).ToHaveCountAsync(0);
            await Expect(page.GetByTestId(Selectors.Teams.TransferOpen)).ToHaveCountAsync(0);
            await Expect(page.GetByTestId(Selectors.Teams.DisbandTeam)).ToHaveCountAsync(0);
            await Expect(page.GetByTestId(Selectors.Teams.LeaveTeam)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Teams.MemberKick)).ToHaveCountAsync(0);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "teams",
                scenario: "role-based-management-options",
                state: "member-actions",
                viewport: "1366x900",
                theme: "light",
                persona: "teamMember");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    private async Task<(TestUser Leader, TestUser Officer, TestUser Member, TeamDto Team)> SeedRoleTeamAsync(
        string userPrefix,
        string tag,
        string teamName)
    {
        var leader = await Fixture.RegisterUniqueUserAsync($"{userPrefix}lead");
        var officer = await Fixture.RegisterUniqueUserAsync($"{userPrefix}off");
        var member = await Fixture.RegisterUniqueUserAsync($"{userPrefix}mem");
        await Fixture.ForceUserPremiumAsync(leader.Email);

        using var leaderApi = await Fixture.CreateAuthenticatedApiClientAsync(leader);
        using var createResponse = await leaderApi.PostAsJsonAsync(
            "api/v1/teams",
            new CreateTeamRequest(teamName, tag, "Tým pro kontrolu rolí.", null));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>();
        team.Should().NotBeNull();

        await Fixture.SeedTeamMemberAsync(team!.Id, leader.Email, TeamRole.Leader, weeklyXp: 100, allTimeXp: 300);
        await Fixture.SeedTeamMemberAsync(team.Id, officer.Email, TeamRole.Officer, weeklyXp: 70, allTimeXp: 200);
        await Fixture.SeedTeamMemberAsync(team.Id, member.Email, TeamRole.Member, weeklyXp: 30, allTimeXp: 90);

        return (leader, officer, member, team);
    }
}
