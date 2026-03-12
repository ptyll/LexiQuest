using LexiQuest.Core.Domain.ValueObjects;

namespace LexiQuest.Core.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserStats Stats { get; private set; } = null!;
    public UserPreferences Preferences { get; private set; } = null!;
    public Streak Streak { get; private set; } = null!;
    public PremiumStatus Premium { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public int LivesRemaining { get; private set; } = 5;
    public int MaxLives { get; private set; } = 5;
    public DateTime? LastLifeLostAt { get; private set; }
    public DateTime? NextLifeRegenAt { get; private set; }
    public string? AvatarUrl { get; private set; }
    public PrivacySettings Privacy { get; private set; } = null!;
    public int CoinBalance { get; private set; }
    public string? StripeCustomerId { get; private set; }

    private readonly List<CoinTransaction> _coinTransactions = new();
    public IReadOnlyCollection<CoinTransaction> CoinTransactions => _coinTransactions.AsReadOnly();

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
            Privacy = PrivacySettings.CreateDefault(),
            CreatedAt = DateTime.UtcNow,
            FailedLoginAttempts = 0
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

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    public void IncrementFailedLoginAttempts()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockAccountUntil(DateTime.UtcNow.AddMinutes(15));
        }
    }

    public void LockAccountUntil(DateTime lockoutEnd)
    {
        LockoutEnd = lockoutEnd;
    }

    public bool IsLockedOut()
    {
        if (LockoutEnd == null)
            return false;
        
        if (LockoutEnd <= DateTime.UtcNow)
        {
            LockoutEnd = null;
            return false;
        }
        
        return true;
    }

    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
    }

    // Test helper methods
    public void SetId(Guid id) => Id = id;
    
    public void SetLives(int current, int max)
    {
        LivesRemaining = current;
        MaxLives = max;
    }
    
    public void SetNextLifeRegenAt(DateTime? dateTime) => NextLifeRegenAt = dateTime;

    // Lives management
    public void ResetLives(int lives, int maxLives)
    {
        LivesRemaining = lives;
        MaxLives = maxLives;
    }

    public void LoseLife()
    {
        if (LivesRemaining > 0 && LivesRemaining < int.MaxValue)
        {
            LivesRemaining--;
            LastLifeLostAt = DateTime.UtcNow;
        }
    }

    public void RegenerateLife()
    {
        if (LivesRemaining < MaxLives && LivesRemaining < int.MaxValue)
        {
            LivesRemaining++;
            if (LivesRemaining >= MaxLives)
            {
                NextLifeRegenAt = null;
            }
            else
            {
                NextLifeRegenAt = DateTime.UtcNow.AddMinutes(GetRegenMinutes());
            }
        }
    }

    public void ScheduleNextRegen(int minutes)
    {
        if (LivesRemaining < MaxLives)
        {
            NextLifeRegenAt = DateTime.UtcNow.AddMinutes(minutes);
        }
    }

    public void RefillLives()
    {
        LivesRemaining = MaxLives;
        NextLifeRegenAt = null;
    }

    private int GetRegenMinutes()
    {
        return MaxLives switch
        {
            3 => 60,  // Expert/Hard: 60 min
            4 => 30,  // Medium: 30 min
            5 => 20,  // Easy: 20 min
            _ => 30
        };
    }

    // Profile management
    public void UpdateProfile(string username, string email)
    {
        Username = username;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePreferences(UserPreferences preferences)
    {
        Preferences = preferences;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrivacySettings(PrivacySettings privacy)
    {
        Privacy = privacy;
        UpdatedAt = DateTime.UtcNow;
    }

    // Coin management
    public void AddCoins(int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));
        CoinBalance += amount;
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));
        if (CoinBalance < amount) return false;
        
        CoinBalance -= amount;
        return true;
    }

    public void SetStripeCustomerId(string stripeCustomerId)
    {
        StripeCustomerId = stripeCustomerId;
    }

    public CoinTransaction AddCoinTransaction(int amount, string type, string description)
    {
        var transaction = CoinTransaction.Create(Id, amount, type, description, CoinBalance + amount);
        _coinTransactions.Add(transaction);
        
        if (amount > 0)
            AddCoins(amount);
        else
            SpendCoins(Math.Abs(amount));
            
        return transaction;
    }
}

public class CoinTransaction
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int Amount { get; private set; }
    public string Type { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public int BalanceAfter { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CoinTransaction() { }

    public static CoinTransaction Create(Guid userId, int amount, string type, string description, int balanceAfter)
    {
        return new CoinTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Type = type,
            Description = description,
            BalanceAfter = balanceAfter,
            CreatedAt = DateTime.UtcNow
        };
    }
}
