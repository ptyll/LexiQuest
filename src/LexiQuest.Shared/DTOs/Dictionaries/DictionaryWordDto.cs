using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Dictionaries;

public class DictionaryWordDto
{
    public Guid Id { get; set; }
    public Guid DictionaryId { get; set; }
    public string Word { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
}
