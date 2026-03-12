namespace LexiQuest.Core.Domain.Entities;

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsUsed { get; set; }
    public bool IsValid => !IsExpired && !IsUsed;

    public void MarkAsUsed()
    {
        UsedAt = DateTime.UtcNow;
        IsUsed = true;
    }

    public static PasswordResetToken Create(Guid userId, string token, TimeSpan expiration)
    {
        return new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.Add(expiration),
            CreatedAt = DateTime.UtcNow
        };
    }
}
