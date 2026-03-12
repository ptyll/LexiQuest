namespace LexiQuest.Infrastructure.Services;

public class StripeSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string MonthlyPriceId { get; set; } = string.Empty;
    public string YearlyPriceId { get; set; } = string.Empty;
    public string LifetimePriceId { get; set; } = string.Empty;
}
