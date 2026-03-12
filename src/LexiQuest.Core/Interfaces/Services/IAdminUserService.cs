using LexiQuest.Shared.DTOs.Admin;

namespace LexiQuest.Core.Interfaces.Services;

public interface IAdminUserService
{
    Task<PaginatedResult<AdminUserDto>> GetUsersAsync(AdminUserListRequest request, CancellationToken cancellationToken = default);
    Task<AdminUserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SuspendUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UnsuspendUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(Guid id, CancellationToken cancellationToken = default);
}
