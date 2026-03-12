namespace LexiQuest.Core.Domain.Entities;

/// <summary>
/// Represents a custom dictionary created by a user (Premium feature).
/// </summary>
public class CustomDictionary
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public bool IsPublic { get; private set; }
    public int WordCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private CustomDictionary() { }

    public static CustomDictionary Create(Guid userId, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));

        return new CustomDictionary
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Description = description ?? string.Empty,
            IsPublic = false,
            WordCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        
        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        Description = description ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPublicStatus(bool isPublic)
    {
        IsPublic = isPublic;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddWord()
    {
        WordCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveWord()
    {
        if (WordCount > 0)
            WordCount--;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanBeAccessedBy(Guid userId)
    {
        return UserId == userId || IsPublic;
    }

    public bool CanBeModifiedBy(Guid userId)
    {
        return UserId == userId;
    }
}
