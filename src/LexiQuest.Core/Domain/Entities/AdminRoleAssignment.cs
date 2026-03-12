namespace LexiQuest.Core.Domain.Entities;

public class AdminRoleAssignment
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Shared.Enums.AdminRole Role { get; private set; }
    public DateTime AssignedAt { get; private set; }

    private AdminRoleAssignment() { }

    public static AdminRoleAssignment Create(Guid userId, Shared.Enums.AdminRole role)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Role = role,
            AssignedAt = DateTime.UtcNow
        };
}
