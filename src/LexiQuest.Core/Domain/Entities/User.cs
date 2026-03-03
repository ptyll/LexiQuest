using LexiQuest.Core.Domain.ValueObjects;

namespace LexiQuest.Core.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string Username { get; private set; } = null!;
    public UserStats Stats { get; private set; } = null!;
    public UserPreferences Preferences { get; private set; } = null!;
    public Streak Streak { get; private set; } = null!;
    public PremiumStatus Premium { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private User() { }

    public static User Create(string email, string username)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = username,
            Stats = UserStats.CreateDefault(),
            Preferences = UserPreferences.CreateDefault(),
            Streak = Streak.CreateDefault(),
            Premium = PremiumStatus.CreateDefault(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateEmail(string email)
    {
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateUsername(string username)
    {
        Username = username;
        UpdatedAt = DateTime.UtcNow;
    }
}
