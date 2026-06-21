using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Game;

public record BossStartRequest(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    BossType BossType,
    DifficultyLevel Difficulty);

public class BossAnswerRequest
{
    [Required]
    [MaxLength(50)]
    public string Answer { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int TimeSpentMs { get; set; } = 1000;
}

public class BossRoundResultDto
{
    public bool IsCorrect { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public int XPGained { get; set; }
    public int BaseXP { get; set; }
    public int BonusXP { get; set; }
    public int CompletionBonus { get; set; }
    public int SpeedBonus { get; set; }
    public int PerfectBonus { get; set; }
    public string? ForbiddenLetterPenalty { get; set; }
    public int ForbiddenLetterPenaltyXP { get; set; }
    public int EarlyGuessBonus { get; set; }
    public int LivesRemaining { get; set; }
    public int CurrentRound { get; set; }
    public int TotalRounds { get; set; }
    public int TotalXP { get; set; }
    public bool IsGameOver { get; set; }
    public bool IsCompleted { get; set; }
    public string? NextScrambledWord { get; set; }
    public int? NextRoundNumber { get; set; }
    public int? WordLength { get; set; }
    public string? ForbiddenLetters { get; set; }
    public int RevealedLettersCount { get; set; }
    public List<int>? RevealedPositions { get; set; }
    public List<RevealedLetterDto>? RevealedLetters { get; set; }
    public DateTime? EndedAt { get; set; }
}

public class TwistRevealStateDto
{
    public List<int> RevealedPositions { get; set; } = [];
    public List<RevealedLetterDto> RevealedLetters { get; set; } = [];
    public DateTime NextRevealAt { get; set; }
    public TimeSpan TimeUntilNextReveal { get; set; }
    public int CurrentBonusXP { get; set; }
    public int RevealedLettersCount { get; set; }
}

public record RevealedLetterDto(int Position, string Letter);
