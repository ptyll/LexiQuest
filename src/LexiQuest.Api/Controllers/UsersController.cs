using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Models;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILoginService _loginService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public UsersController(
        IUserService userService,
        ILoginService loginService,
        IPasswordResetService passwordResetService,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings)
    {
        _userService = userService;
        _loginService = loginService;
        _passwordResetService = passwordResetService;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
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

        await StoreRefreshTokenAsync(result.Value!, cancellationToken);

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

        await StoreRefreshTokenAsync(result.Value!, cancellationToken);

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Refresh token is invalid.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (storedToken is null || !storedToken.IsActive)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Refresh token is invalid or expired.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null)
        {
            storedToken.Revoke();
            _refreshTokenRepository.Update(storedToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Refresh token is invalid.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        storedToken.Revoke(newRefreshToken);
        _refreshTokenRepository.Update(storedToken);

        var response = CreateAuthResponse(user, newRefreshToken);
        await StoreRefreshTokenAsync(response, cancellationToken);

        return Ok(response);
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

    private async Task StoreRefreshTokenAsync(AuthResponse response, CancellationToken cancellationToken)
    {
        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
        await _refreshTokenRepository.AddAsync(
            RefreshToken.Create(response.RefreshToken, response.User.Id, expiresAt),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private AuthResponse CreateAuthResponse(User user, string refreshToken)
    {
        return new AuthResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                CurrentStreak = user.Streak.CurrentDays,
                TotalXP = user.Stats.TotalXP,
                League = user.Stats.League
            }
        };
    }
}
