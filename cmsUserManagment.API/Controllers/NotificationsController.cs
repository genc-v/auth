using cmsUserManagment.Application.Common;
using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsUserManagment.Controllers;

[Route("api/notifications")]   
[ApiController]
[Authorize]
public class NotificationsController(
    INotificationService notificationService,
    JwtDecoder jwtDecoder,
    HeadersManager headersManager) : ControllerBase
{
    private readonly INotificationService _notificationService = notificationService;
    private readonly JwtDecoder _jwtDecoder = jwtDecoder;
    private readonly HeadersManager _headersManager = headersManager;

    /// <summary>Gets all notifications for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications()
    {
        string token = _headersManager.GetJwtFromHeader(Request.Headers);
        Guid userId = _jwtDecoder.GetUserid(token);
        var notifications = await _notificationService.GetUserNotifications(userId);
        return Ok(notifications);
    }

    /// <summary>Creates a notification for a user (admin only).</summary>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        var notification = await _notificationService.CreateNotification(dto);
        return CreatedAtAction(nameof(GetMyNotifications), notification);
    }

    /// <summary>Marks a notification as read.</summary>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        string token = _headersManager.GetJwtFromHeader(Request.Headers);
        Guid userId = _jwtDecoder.GetUserid(token);
        await _notificationService.MarkAsRead(id, userId);
        return NoContent();
    }

    /// <summary>Marks all notifications as read for the current user.</summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        string token = _headersManager.GetJwtFromHeader(Request.Headers);
        Guid userId = _jwtDecoder.GetUserid(token);
        await _notificationService.MarkAllAsRead(userId);
        return NoContent();
    }

    /// <summary>Deletes a notification.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        string token = _headersManager.GetJwtFromHeader(Request.Headers);
        Guid userId = _jwtDecoder.GetUserid(token);
        await _notificationService.DeleteNotification(id, userId);
        return NoContent();
    }
}
