using LexiQuest.Core.Models;
using LexiQuest.Shared.DTOs.Auth;

namespace LexiQuest.Core.Interfaces.Services;

public interface IPasswordResetService
{
    Task<Result> RequestResetAsync(RequestPasswordResetDto request, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default);
}
