using Microsoft.AspNetCore.Components;

namespace LexiQuest.Blazor.Tests;

public class TestNavigationManager : NavigationManager
{
    public TestNavigationManager()
    {
        Initialize("https://localhost/", "https://localhost/");
    }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        Uri = new Uri(new Uri(BaseUri), uri).ToString();
    }
}
