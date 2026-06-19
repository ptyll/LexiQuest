using LexiQuest.Shared.DTOs.Admin;

namespace LexiQuest.Blazor.Services;

public interface IAdminService
{
    Task<AdminDashboardStatsDto?> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default);

    // Words
    Task<PaginatedResult<AdminWordDto>> GetWordsAsync(AdminWordListRequest request, CancellationToken cancellationToken = default);
    Task<AdminWordDto?> CreateWordAsync(AdminWordCreateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteWordAsync(Guid id, CancellationToken cancellationToken = default);

    // Users
    Task<PaginatedResult<AdminUserDto>> GetUsersAsync(AdminUserListRequest request, CancellationToken cancellationToken = default);
    Task<bool> SuspendUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UnsuspendUserAsync(Guid id, CancellationToken cancellationToken = default);
}
