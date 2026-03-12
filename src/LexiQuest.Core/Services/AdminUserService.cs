using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Admin;
using LexiQuest.Shared.DTOs.Auth;

namespace LexiQuest.Core.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordResetService _passwordResetService;

    public AdminUserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordResetService passwordResetService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordResetService = passwordResetService;
    }

    public async Task<PaginatedResult<AdminUserDto>> GetUsersAsync(AdminUserListRequest request, CancellationToken cancellationToken = default)
    {
        var allUsers = await _userRepository.GetActiveUsersAsync(cancellationToken);
        // Also include inactive users - get all users via broader query
        var inactiveUsers = await _userRepository.GetInactiveUsersAsync(0, cancellationToken);

        var users = allUsers.Union(inactiveUsers).DistinctBy(u => u.Id).AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLowerInvariant();
            users = users.Where(u =>
                u.Username.ToLowerInvariant().Contains(search) ||
                u.Email.ToLowerInvariant().Contains(search));
        }

        if (request.IsSuspended.HasValue)
        {
            users = request.IsSuspended.Value
                ? users.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTime.UtcNow)
                : users.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTime.UtcNow);
        }

        if (request.IsPremium.HasValue)
        {
            users = request.IsPremium.Value
                ? users.Where(u => u.Premium != null && u.Premium.IsPremium)
                : users.Where(u => u.Premium == null || !u.Premium.IsPremium);
        }

        if (request.MinLevel.HasValue)
            users = users.Where(u => u.Stats != null && u.Stats.Level >= request.MinLevel.Value);

        if (request.MaxLevel.HasValue)
            users = users.Where(u => u.Stats != null && u.Stats.Level <= request.MaxLevel.Value);

        var totalCount = users.Count();

        var paged = users
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = paged.Select(u => new AdminUserDto(
            u.Id,
            u.Username,
            u.Email,
            u.Stats?.Level ?? 1,
            u.Stats?.TotalXP ?? 0,
            u.Streak?.CurrentDays ?? 0,
            u.IsLockedOut(),
            u.Premium?.IsPremium ?? false,
            u.CreatedAt,
            u.LastLoginAt
        )).ToList();

        return new PaginatedResult<AdminUserDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null) return null;

        return new AdminUserDto(
            user.Id,
            user.Username,
            user.Email,
            user.Stats?.Level ?? 1,
            user.Stats?.TotalXP ?? 0,
            user.Streak?.CurrentDays ?? 0,
            user.IsLockedOut(),
            user.Premium?.IsPremium ?? false,
            user.CreatedAt,
            user.LastLoginAt
        );
    }

    public async Task<bool> SuspendUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null) return false;

        user.LockAccountUntil(DateTime.UtcNow.AddYears(100));
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnsuspendUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null) return false;

        user.RecordSuccessfulLogin(); // Clears lockout
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null) return false;

        await _passwordResetService.RequestResetAsync(
            new RequestPasswordResetDto { Email = user.Email },
            cancellationToken);

        return true;
    }
}
