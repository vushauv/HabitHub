using backend.Auth;
using backend.Dtos.NotificationDtos;
using backend.Enums;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    public class NotificationController(INotificationService notificationService) : ControllerBase
    {
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] NotificationType? type)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            List<NotificationDto> response = await notificationService.GetNotifications(currentUser.UserId, currentUser.UserType, type);

            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpGet("notifications/unread-count")]
        public async Task<IActionResult> GetUnreadCount([FromQuery] NotificationType? type)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            NotificationCountDto response = await notificationService.GetUnreadCount(currentUser.UserId, currentUser.UserType, type);

            return StatusCode(StatusCodes.Status200OK, response);
        }
        [HttpPatch("notifications/read-all")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] NotificationType? type)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await notificationService.MarkAllAsRead(currentUser.UserId, currentUser.UserType, type);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPatch("notifications/{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await notificationService.MarkAsRead(currentUser.UserId, currentUser.UserType, notificationId);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpDelete("notifications/{notificationId}")]
        public async Task<IActionResult> DeleteNotification(Guid notificationId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await notificationService.DeleteNotification(currentUser.UserId, currentUser.UserType, notificationId);

            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}