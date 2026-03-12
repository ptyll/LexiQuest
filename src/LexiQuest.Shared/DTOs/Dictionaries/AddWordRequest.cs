using System.ComponentModel.DataAnnotations;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Dictionaries;

public record AddWordRequest(
    [Required] string Word,
    DifficultyLevel Difficulty
);
