namespace LexiQuest.Core.Domain.Entities;

public class PushSubscription
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Endpoint { get; private set; } = null!;
    public string P256dh { get; private set; } = null!;
    public string Auth { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private PushSubscription() { }

    public static PushSubscription Create(Guid userId, string endpoint, string p256dh, string auth)
    {
        return new PushSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Endpoint = endpoint,
            P256dh = p256dh,
            Auth = auth,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateKeys(string endpoint, string p256dh, string auth)
    {
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
    }
}
