using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Notifications;
using LexiQuest.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using LexiQuest.Blazor.Tests.Helpers;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class NotificationBellTests : BunitContext
{
    private readonly INotificationService _notificationService;
    private readonly IStringLocalizer<NotificationBell> _localizer;

    public NotificationBellTests()
    {
        _notificationService = Substitute.For<INotificationService>();
        _localizer = Substitute.For<IStringLocalizer<NotificationBell>>();

        var tmLocalizer = Substitute.For<ITmLocalizer>();
        tmLocalizer[Arg.Any<string>()].Returns(ci => ci.Arg<string>());
        Services.AddSingleton(tmLocalizer);

        _localizer["Title"].Returns(new LocalizedString("Title", "Notifikace"));
        _localizer["NoNotifications"].Returns(new LocalizedString("NoNotifications", "Žádné notifikace"));
        _localizer["MarkAllRead"].Returns(new LocalizedString("MarkAllRead", "Označit vše jako přečtené"));
        _localizer["Today"].Returns(new LocalizedString("Today", "Dnes"));
        _localizer["Yesterday"].Returns(new LocalizedString("Yesterday", "Včera"));
        _localizer["ThisWeek"].Returns(new LocalizedString("ThisWeek", "Tento týden"));
        _localizer["Older"].Returns(new LocalizedString("Older", "Starší"));
        _localizer["UnreadCount"].Returns(new LocalizedString("UnreadCount", "{0} nepřečtených"));

        Services.AddSingleton(_localizer);
        Services.AddSingleton(_notificationService);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager());
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void NotificationBell_ShowsUnreadCount()
    {
        // Arrange
        _notificationService.GetUnreadCountAsync().Returns(5);

        // Act
        var cut = Render<NotificationBell>();

        // Assert
        var badge = cut.Find(".unread-badge");
        badge.Should().NotBeNull();
        badge.TextContent.Should().Contain("5");
    }

    [Fact]
    public void NotificationBell_Click_ShowsDropdown()
    {
        // Arrange
        _notificationService.GetUnreadCountAsync().Returns(0);
        _notificationService.GetNotificationsAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(new List<NotificationDto>());

        var cut = Render<NotificationBell>();

        // Act
        cut.Find(".bell-button").Click();

        // Assert
        cut.Find(".notification-dropdown").Should().NotBeNull();
        cut.Find(".dropdown-title").TextContent.Should().Be("Notifikace");
    }

    [Fact]
    public async Task NotificationBell_MarkAllRead_ClearsCount()
    {
        // Arrange
        _notificationService.GetUnreadCountAsync().Returns(3);
        _notificationService.GetNotificationsAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(new List<NotificationDto>
            {
                new NotificationDto(
                    Guid.NewGuid(),
                    NotificationType.AchievementUnlocked,
                    "Úspěch",
                    "Odemkl jsi úspěch!",
                    NotificationSeverity.Success,
                    false,
                    null,
                    DateTime.UtcNow,
                    null)
            });

        var cut = Render<NotificationBell>();

        // Open dropdown
        cut.Find(".bell-button").Click();

        // Act - click mark all read
        cut.Find(".mark-all-read-button").Click();

        // Assert
        await _notificationService.Received(1).MarkAllReadAsync();
        var badges = cut.FindAll(".unread-badge");
        badges.Count.Should().Be(0);
    }
}
