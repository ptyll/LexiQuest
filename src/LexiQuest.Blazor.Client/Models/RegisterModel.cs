namespace LexiQuest.Blazor.Models;

public class RegisterModel
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool AcceptTerms { get; set; }
    public string? GuestProgressToken { get; set; }
    public int GuestTransferredXp { get; set; }
    public int GuestTransferredWordsSolved { get; set; }
}
