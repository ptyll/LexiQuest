namespace LexiQuest.Core.Interfaces.Services;

public interface IAdminAuthorizationService
{
    Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsModeratorAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsContentManagerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasRoleAsync(Guid userId, Shared.Enums.AdminRole role, CancellationToken cancellationToken = default);
}
