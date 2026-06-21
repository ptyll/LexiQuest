namespace LexiQuest.Blazor.Services;

public class NotificationRefreshService
{
    public event Func<Task>? RefreshRequested;

    public async Task RequestRefreshAsync()
    {
        var handlers = RefreshRequested;
        if (handlers == null)
        {
            return;
        }

        foreach (Func<Task> handler in handlers.GetInvocationList())
        {
            await handler();
        }
    }
}
