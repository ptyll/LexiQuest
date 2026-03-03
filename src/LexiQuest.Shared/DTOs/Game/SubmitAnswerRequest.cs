using System.ComponentModel.DataAnnotations;

namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// Request to submit an answer for a game round.
/// </summary>
public class SubmitAnswerRequest
{
    /// <summary>
    /// Game session ID.
    /// </summary>
    [Required]
    public Guid SessionId { get; set; }

    /// <summary>
    /// User's answer.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// Time spent answering in milliseconds.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TimeSpentMs { get; set; }
}
