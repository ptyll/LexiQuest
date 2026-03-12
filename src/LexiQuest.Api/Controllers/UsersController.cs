using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Models;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILoginService _loginService;
    private readonly IPasswordResetService _passwordResetService;

    public UsersController(IUserService userService, ILoginService loginService, IPasswordResetService passwordResetService)
    {
        _userService = userService;
        _loginService = loginService;
        _passwordResetService = passwordResetService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.RegisterAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Email.AlreadyExists" or "Username.AlreadyExists" => Conflict(new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = result.Error.Message,
                    Status = StatusCodes.Status409Conflict
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = result.Error.Message,
                    Status = StatusCodes.Status400BadRequest
                })
            };
        }

        return Created($"/api/v1/users/{result.Value!.User.Id}", result.Value);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status423Locked)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _loginService.LoginAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Authentication.AccountLocked" => StatusCode(StatusCodes.Status423Locked, new ProblemDetails
                {
                    Title = "Account Locked",
                    Detail = result.Error.Message,
                    Status = StatusCodes.Status423Locked
                }),
                "Authentication.InvalidCredentials" => Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = result.Error.Message,
                    Status = StatusCodes.Status401Unauthorized
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = result.Error.Message,
                    Status = StatusCodes.Status400BadRequest
                })
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("password-reset/request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetDto request, CancellationToken cancellationToken)
    {
        var result = await _passwordResetService.RequestResetAsync(request, cancellationToken);
        
        // Vždy vracíme 200 OK, abychom neodhalili existenci emailu
        return Ok();
    }

    [HttpPost("password-reset/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto request, CancellationToken cancellationToken)
    {
        var result = await _passwordResetService.ResetPasswordAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Bad Request",
                Detail = result.Error.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok();
    }
}
