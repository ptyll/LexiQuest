namespace LexiQuest.Infrastructure.Services;

public class EmailSettings
{
    public string Host { get; set; } = null!;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
    public bool UseSsl { get; set; } = true;
    public string BaseUrl { get; set; } = "https://lexiquest.cz";
}
