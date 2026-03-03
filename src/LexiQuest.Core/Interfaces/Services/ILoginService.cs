using LexiQuest.Core.Models;
using LexiQuest.Shared.DTOs.Auth;

namespace LexiQuest.Core.Interfaces.Services;

public interface ILoginService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
