using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components;
using LexiQuest.Blazor.Layout;
using LexiQuest.Blazor.Services;
using LexiQuest.Blazor.Tests.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Services;

namespace LexiQuest.Blazor.Tests.Layout;

public class MainLayoutTests : BunitContext
{
    private readonly IAuthService _authService;
    private readonly IStringLocalizer<MainLayout> _localizer;

    public MainLayoutTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        _authService = Substitute.For<IAuthService>();
        _authService.IsAuthenticatedAsync().Returns(Task.FromResult(true));

        _localizer = Substitute.For<IStringLocalizer<MainLayout>>();
        _localizer[Arg.Any<string>()].Returns(call =>
        {
            var key = call.Arg<string>();
            return new LocalizedString(key, key);
        });

        Services.AddSingleton(_authService);
        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<IStringLocalizer<OfflineBanner>>());
        Services.AddSingleton(Substitute.For<IStringLocalizer<InstallPrompt>>());
        Services.AddSingleton(Substitute.For<IStringLocalizer<NotificationBell>>());
        Services.AddSingleton(Substitute.For<IStringLocalizer<ErrorBoundary>>());
        Services.AddSingleton(Substitute.For<IGameService>());
        Services.AddSingleton(Substitute.For<INotificationService>());
        Services.AddSingleton(Substitute.For<IErrorLoggingService>());
        Services.AddSingleton(new NotificationRefreshService());
        Services.AddSingleton(new TestNavigationManager());
        Services.AddSingleton<NavigationManager>(services => services.GetRequiredService<TestNavigationManager>());
        Services.AddSingleton(new ThemeService());
        Services.AddSingleton(new ToastService());
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void MainLayout_MobileNavigation_IncludesDiscoverableFeatureLinks()
    {
        // Arrange & Act
        var cut = Render<MainLayout>(parameters =>
            parameters.Add(layout => layout.Body, builder => builder.AddContent(0, "Obsah")));

        cut.WaitForElement("[data-testid='mobile-nav-toggle']");
        cut.Find("[data-testid='mobile-nav-toggle']").Click();

        // Assert
        AssertMobileNavLink(cut, "multiplayer", "/multiplayer");
        AssertMobileNavLink(cut, "team", "/team");
        AssertMobileNavLink(cut, "dictionaries", "/dictionaries");
        AssertMobileNavLink(cut, "shop", "/shop");
    }

    private static void AssertMobileNavLink(IRenderedComponent<MainLayout> cut, string id, string href)
    {
        var link = cut.Find($"[data-testid='mobile-nav-link-{id}']");
        link.GetAttribute("href").Should().Be(href);
    }
}
