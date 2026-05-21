using backend.Dtos.NotificationDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Service.Interfaces;

namespace backend.Service
{
    public class NotificationService(INotificationRepository notifications) : INotificationService
    {
        public async Task<List<NotificationDto>> GetNotifications(Guid userId, UserType userType, NotificationType? type)
        {
            List<Notification> userNotifications = new List<Notification>();

            if (type == null)
            {
                userNotifications.AddRange(await notifications.GetVisibleNotificationsForUserByTypeAsync(userId, userType, NotificationType.System));
                userNotifications.AddRange(await notifications.GetVisibleNotificationsForUserByTypeAsync(userId, userType, NotificationType.Reminder));
            }
            else
            {
                userNotifications.AddRange(await notifications.GetVisibleNotificationsForUserByTypeAsync(userId, userType, type.Value));
            }

            return userNotifications
                .OrderByDescending(n => n.CreatedAt)
                .Select(ToDto)
                .ToList();
        }

        public async Task<NotificationCountDto> GetUnreadCount(Guid userId, UserType userType, NotificationType? type)
        {
            int count;

            if (type == null)
            {
                int systemCount = await notifications.GetUnreadNotificationsCountForUserByTypeAsync(userId, userType, NotificationType.System);
                int reminderCount = await notifications.GetUnreadNotificationsCountForUserByTypeAsync(userId, userType, NotificationType.Reminder);

                count = systemCount + reminderCount;
            }
            else
            {
                count = await notifications.GetUnreadNotificationsCountForUserByTypeAsync(userId, userType, type.Value);
            }

            return new NotificationCountDto(count);
        }

        public async Task MarkAsRead(Guid userId, UserType userType, Guid notificationId)
        {
            Notification notification = await GetOwnedNotificationOrThrow(userId, userType, notificationId);

            bool updated = await notifications.MarkNotificationAsReadAsync(notification.NotificationId);
            if (!updated)
                throw new NotFoundException();
        }

        public async Task DeleteNotification(Guid userId, UserType userType, Guid notificationId)
        {
            Notification notification = await GetOwnedNotificationOrThrow(userId, userType, notificationId);

            bool updated = await notifications.MarkNotificationAsDeletedAsync(notification.NotificationId);
            if (!updated)
                throw new NotFoundException();
        }

        private async Task<Notification> GetOwnedNotificationOrThrow(Guid userId, UserType userType, Guid notificationId)
        {
            Notification? notification = await notifications.GetNotificationByIdAsync(notificationId);

            if (notification == null || notification.Status == NotificationStatus.Deleted)
                throw new NotFoundException();

            if (notification.UserId != userId || notification.UserType != userType)
                throw new ForbiddenException();

            return notification;
        }
        private static NotificationDto ToDto(Notification notification)
        {
            return new NotificationDto(notification.NotificationId, notification.Content, notification.CreatedAt, notification.Status, notification.Type);
        }
        public async Task MarkAllAsRead(Guid userId, UserType userType, NotificationType? type)
        {
            List<Notification> userNotifications = new List<Notification>();

            if (type == null)
            {
                userNotifications.AddRange(await notifications.GetVisibleNotificationsForUserByTypeAsync(userId, userType, NotificationType.System));
                userNotifications.AddRange(await notifications.GetVisibleNotificationsForUserByTypeAsync(userId, userType, NotificationType.Reminder));
            }
            else
            {
                userNotifications.AddRange(await notifications.GetVisibleNotificationsForUserByTypeAsync(userId, userType, type.Value));
            }
            
            List<Notification> unreadNotifications = userNotifications
                .Where(n => n.Status == NotificationStatus.Unread)
                .ToList();

            foreach (Notification notification in unreadNotifications)
            {
                await notifications.MarkNotificationAsReadAsync(notification.NotificationId);
            }
        }
    }
}