using LexiQuest.Api.Extensions;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IPushSubscriptionRepository _pushSubscriptionRepository;
    private readonly Core.Interfaces.IUnitOfWork _unitOfWork;

    public NotificationsController(
        INotificationService notificationService,
        IPushSubscriptionRepository pushSubscriptionRepository,
        Core.Interfaces.IUnitOfWork unitOfWork)
    {
        _notificationService = notificationService;
        _pushSubscriptionRepository = pushSubscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var notifications = await _notificationService.GetByUserIdAsync(userId, skip, take, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(count);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _notificationService.MarkReadAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _notificationService.MarkAllReadAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferenceDto>> GetPreferences(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var preferences = await _notificationService.GetPreferencesAsync(userId, cancellationToken);
        return Ok(preferences);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdatePreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _notificationService.UpdatePreferencesAsync(userId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("push-subscription")]
    public async Task<IActionResult> SavePushSubscription(
        [FromBody] PushSubscriptionDto request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();

        var existing = await _pushSubscriptionRepository.GetByEndpointAsync(request.Endpoint, cancellationToken);
        if (existing != null)
        {
            existing.UpdateKeys(request.Endpoint, request.P256dh, request.Auth);
            _pushSubscriptionRepository.Update(existing);
        }
        else
        {
            var subscription = Core.Domain.Entities.PushSubscription.Create(
                userId, request.Endpoint, request.P256dh, request.Auth);
            await _pushSubscriptionRepository.AddAsync(subscription, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

}
