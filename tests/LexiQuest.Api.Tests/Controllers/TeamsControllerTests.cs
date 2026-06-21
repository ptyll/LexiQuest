using System.Security.Claims;
using FluentAssertions;
using LexiQuest.Api.Controllers;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Teams;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace LexiQuest.Api.Tests.Controllers;

public class TeamsControllerTests
{
    private readonly ITeamService _teamService;
    private readonly TeamsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public TeamsControllerTests()
    {
        _teamService = Substitute.For<ITeamService>();
        _controller = new TeamsController(_teamService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                        [
                            new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
                        ],
                        "TestAuth"))
                }
            }
        };
    }

    [Fact]
    public async Task GetMyTeamForUi_NoTeam_ReturnsNoContent()
    {
        _teamService.GetUserTeamAsync(_userId, Arg.Any<CancellationToken>())
            .Returns((TeamDto?)null);

        var result = await _controller.GetMyTeamForUi(CancellationToken.None);

        result.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetMyTeam_NoTeam_KeepsNotFoundApiContract()
    {
        _teamService.GetUserTeamAsync(_userId, Arg.Any<CancellationToken>())
            .Returns((TeamDto?)null);

        var result = await _controller.GetMyTeam(CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetMyTeamForUi_ExistingTeam_ReturnsTeam()
    {
        var team = new TeamDto(
            Guid.NewGuid(),
            "Test tým",
            "TST",
            "Popis",
            null,
            _userId,
            "leader",
            DateTime.UtcNow,
            1,
            new TeamStatsDto(10, 100, 1, 2, 3, 67));

        _teamService.GetUserTeamAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(team);

        var result = await _controller.GetMyTeamForUi(CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result.Result!;
        ok.Value.Should().BeEquivalentTo(team);
    }
}
