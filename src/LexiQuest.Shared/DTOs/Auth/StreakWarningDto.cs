namespace LexiQuest.Shared.DTOs.Auth;

public class StreakWarningDto
{
    public int HoursRemaining { get; set; }
    public string Message { get; set; } = string.Empty;
}
