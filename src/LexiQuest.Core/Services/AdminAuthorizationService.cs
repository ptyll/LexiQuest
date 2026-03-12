using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Services;

public class AdminAuthorizationService : IAdminAuthorizationService
{
    private readonly IAdminRoleAssignmentRepository _roleRepository;

    public AdminAuthorizationService(IAdminRoleAssignmentRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await HasRoleAsync(userId, AdminRole.Admin, cancellationToken);
    }

    public async Task<bool> IsModeratorAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetByUserIdAsync(userId, cancellationToken);
        return roles.Any(r => r.Role == AdminRole.Admin || r.Role == AdminRole.Moderator);
    }

    public async Task<bool> IsContentManagerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetByUserIdAsync(userId, cancellationToken);
        return roles.Any(r => r.Role == AdminRole.Admin || r.Role == AdminRole.ContentManager);
    }

    public async Task<bool> HasRoleAsync(Guid userId, AdminRole role, CancellationToken cancellationToken = default)
    {
        var assignment = await _roleRepository.GetByUserIdAndRoleAsync(userId, role, cancellationToken);
        return assignment != null;
    }
}
