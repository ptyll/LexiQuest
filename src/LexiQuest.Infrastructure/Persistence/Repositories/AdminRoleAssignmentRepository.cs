using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class AdminRoleAssignmentRepository : IAdminRoleAssignmentRepository
{
    private readonly LexiQuestDbContext _context;

    public AdminRoleAssignmentRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<AdminRoleAssignment?> GetByUserIdAndRoleAsync(Guid userId, AdminRole role, CancellationToken cancellationToken = default)
    {
        return await _context.AdminRoleAssignments
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Role == role, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminRoleAssignment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AdminRoleAssignments
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AdminRoleAssignment assignment, CancellationToken cancellationToken = default)
    {
        await _context.AdminRoleAssignments.AddAsync(assignment, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
