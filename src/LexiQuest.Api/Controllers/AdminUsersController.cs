using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<AdminUserDto>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] bool? isSuspended,
        [FromQuery] bool? isPremium,
        [FromQuery] int? minLevel,
        [FromQuery] int? maxLevel,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var request = new AdminUserListRequest(search, isSuspended, isPremium, minLevel, maxLevel, page, pageSize);
        var result = await _adminUserService.GetUsersAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _adminUserService.GetUserByIdAsync(id, cancellationToken);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendUser(Guid id, CancellationToken cancellationToken)
    {
        var success = await _adminUserService.SuspendUserAsync(id, cancellationToken);
        if (!success) return NotFound();
        return Ok();
    }

    [HttpPost("{id:guid}/unsuspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnsuspendUser(Guid id, CancellationToken cancellationToken)
    {
        var success = await _adminUserService.UnsuspendUserAsync(id, cancellationToken);
        if (!success) return NotFound();
        return Ok();
    }

    [HttpPost("{id:guid}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
    {
        var success = await _adminUserService.ResetPasswordAsync(id, cancellationToken);
        if (!success) return NotFound();
        return Ok();
    }
}
