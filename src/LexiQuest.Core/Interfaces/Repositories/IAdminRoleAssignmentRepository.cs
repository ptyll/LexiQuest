using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface IAdminRoleAssignmentRepository
{
    Task<AdminRoleAssignment?> GetByUserIdAndRoleAsync(Guid userId, AdminRole role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminRoleAssignment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(AdminRoleAssignment assignment, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
