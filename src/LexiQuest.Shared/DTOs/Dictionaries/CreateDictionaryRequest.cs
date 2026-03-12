using System.ComponentModel.DataAnnotations;

namespace LexiQuest.Shared.DTOs.Dictionaries;

public record CreateDictionaryRequest(
    [Required] string Name,
    string Description
);
