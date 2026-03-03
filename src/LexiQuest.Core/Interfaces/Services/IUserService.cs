using LexiQuest.Core.Models;
using LexiQuest.Shared.DTOs.Auth;

namespace LexiQuest.Core.Interfaces.Services;

public interface IUserService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
