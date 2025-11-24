using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Notification;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get current user's notifications
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetNotifications(
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int limit = 50,
            CancellationToken ct = default)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found in token");

            var notifications = await _notificationService.GetByUserIdAsync(userId.Value, unreadOnly, limit, ct);
            return Ok(notifications);
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount(CancellationToken ct = default)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found in token");

            var count = await _notificationService.GetUnreadCountAsync(userId.Value, ct);
            return Ok(count);
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPost("{notificationId}/mark-read")]
        public async Task<ActionResult> MarkAsRead(Guid notificationId, CancellationToken ct = default)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found in token");

            try
            {
                await _notificationService.MarkAsReadAsync(notificationId, userId.Value, ct);
                return Ok(new { message = "Notification marked as read" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPost("mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead(CancellationToken ct = default)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found in token");

            await _notificationService.MarkAllAsReadAsync(userId.Value, ct);
            return Ok(new { message = "All notifications marked as read" });
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            if (Guid.TryParse(userIdClaim, out Guid userId))
                return userId;

            return null;
        }
    }
}