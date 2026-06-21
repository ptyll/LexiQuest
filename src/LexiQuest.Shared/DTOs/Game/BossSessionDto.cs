using System.Text.Json.Serialization;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// DTO for boss level game session state.
/// </summary>
public class BossSessionDto
{
    public Guid Id { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BossType BossType { get; set; }

    public int CurrentRound { get; set; }
    public int TotalRounds { get; set; }
    public int LivesRemaining { get; set; }
    public GameMode Mode { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public bool IsGameOver { get; set; }
    public bool IsCompleted { get; set; }
    public int TotalXP { get; set; }
    public int CorrectAnswers { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string CurrentScrambledWord { get; set; } = string.Empty;
    public int WordLength { get; set; }
    
    // Condition boss specific
    public string? ForbiddenLetters { get; set; }
    
    // Twist boss specific
    public int RevealedLettersCount { get; set; }
    public List<int>? RevealedPositions { get; set; }
    public List<RevealedLetterDto>? RevealedLetters { get; set; }
    public TimeSpan? TimeUntilNextReveal { get; set; }
    public int CurrentEarlyGuessBonus { get; set; }
}
