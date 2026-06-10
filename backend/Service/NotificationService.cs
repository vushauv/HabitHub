using backend.Dtos.NotificationDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Service.Interfaces;

namespace backend.Service
{
    public class NotificationService(
        INotificationRepository notifications,
        ILogger<NotificationService> logger
    ) : INotificationService
    {
        public async Task<List<NotificationDto>> GetNotifications(Guid userId, NotificationType? type)
        {
            List<Notification> userNotifications = new List<Notification>();

            if (type == null)
            {
                userNotifications.AddRange(await notifications.GetVisibleNotificationsForUserByTypeAsync(userId, NotificationType.System));
                userNotifications.AddRange(await notifications.GetVisibleNotificationsForUserByTypeAsync(userId, NotificationType.Reminder));
            }
            else
            {
                userNotifications.AddRange(await notifications.GetVisibleNotificationsForUserByTypeAsync(userId, type.Value));
            }

            return userNotifications
                .OrderByDescending(n => n.CreatedAt)
                .Select(ToDto)
                .ToList();
        }

        public async Task<NotificationCountDto> GetUnreadCount(Guid userId, NotificationType? type)
        {
            int count;

            if (type == null)
            {
                int systemCount = await notifications.GetUnreadNotificationsCountForUserByTypeAsync(userId, NotificationType.System);
                int reminderCount = await notifications.GetUnreadNotificationsCountForUserByTypeAsync(userId, NotificationType.Reminder);

                count = systemCount + reminderCount;
            }
            else
            {
                count = await notifications.GetUnreadNotificationsCountForUserByTypeAsync(userId, type.Value);
            }

            return new NotificationCountDto(count);
        }

        public async Task MarkAsRead(Guid userId, Guid notificationId)
        {
            Notification notification = await GetOwnedNotificationOrThrow(userId, notificationId);

            bool updated = await notifications.MarkNotificationAsReadAsync(notification.NotificationId);
            if (!updated)
            {
                logger.LogWarning("Mark as read rejected: notification {NotificationId} not found", notificationId);
                throw new NotFoundException();
            }

            logger.LogInformation("Marked notification {NotificationId} as read for user {UserId}", notificationId, userId);
        }

        public async Task DeleteNotification(Guid userId, Guid notificationId)
        {
            Notification notification = await GetOwnedNotificationOrThrow(userId, notificationId);

            bool updated = await notifications.MarkNotificationAsDeletedAsync(notification.NotificationId);
            if (!updated)
            {
                logger.LogWarning("Delete notification rejected: notification {NotificationId} not found", notificationId);
                throw new NotFoundException();
            }

            logger.LogInformation("Deleted notification {NotificationId} for user {UserId}", notificationId, userId);
        }

        private async Task<Notification> GetOwnedNotificationOrThrow(Guid userId, Guid notificationId)
        {
            Notification? notification = await notifications.GetNotificationByIdAsync(notificationId);

            if (notification == null || notification.Status == NotificationStatus.Deleted)
                throw new NotFoundException();

            if (notification.UserId != userId)
                throw new ForbiddenException();

            return notification;
        }

        private static NotificationDto ToDto(Notification notification)
        {
            return new NotificationDto(notification.NotificationId, notification.Content, notification.CreatedAt, notification.Status, notification.Type);
        }

        public async Task MarkAllAsRead(Guid userId, NotificationType? type)
        {
            await notifications.MarkAllUnreadNotificationsAsReadAsync(userId, type);

            logger.LogInformation("Marked all unread notifications as read for user {UserId}", userId);
        }
    }
}