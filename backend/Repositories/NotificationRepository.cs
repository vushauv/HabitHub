using backend.Data;
using backend.Enums;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class NotificationRepository(AppDbContext db): INotificationRepository
    {
        private static readonly TimeSpan VisibleNotificationTime = TimeSpan.FromDays(10);
        public async Task<List<Notification>> GetVisibleNotificationsForUserByTypeAsync(Guid userId, UserType userType, NotificationType type)
        {
            DateTime timeLimit = DateTime.UtcNow.Subtract(VisibleNotificationTime);

            return await db.Notifications
                .Where(n =>
                    n.UserId == userId &&
                    n.UserType == userType &&
                    n.Status != NotificationStatus.Deleted &&
                    n.Type == type &&
                    n.CreatedAt >= timeLimit)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
        public async Task<int> GetUnreadNotificationsCountForUserByTypeAsync(Guid userId, UserType userType, NotificationType type)
        {
            DateTime timeLimit = DateTime.UtcNow.Subtract(VisibleNotificationTime);

            return await db.Notifications
                .CountAsync(n =>
                    n.UserId == userId &&
                    n.UserType == userType &&
                    n.Status == NotificationStatus.Unread &&
                    n.Type == type &&
                    n.CreatedAt >= timeLimit);
        }
        public async Task<Notification?> GetNotificationByIdAsync(Guid notificationId) => 
            await db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            db.Notifications.Add(notification);
            await db.SaveChangesAsync();

            return notification;
        }
        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId)
        {
            Notification? notification = await db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);
            if (notification == null)
                return false;
            if (notification.Status == NotificationStatus.Deleted)
                return false;

            notification.Status = NotificationStatus.Read;
            await db.SaveChangesAsync();
            return true;
        }
        public async Task MarkAllUnreadNotificationsAsReadAsync(Guid userId, UserType userType, NotificationType? type)
        {
            IQueryable<Notification> query = db.Notifications.Where(n =>
                n.UserId == userId &&
                n.UserType == userType &&
                n.Status == NotificationStatus.Unread
            );

            if(type != null)
            {
                query = query.Where(n => n.Type == type.Value);
            }

            List<Notification> unreadNotifications = await query.ToListAsync();

            if (unreadNotifications.Count == 0)
                return;
            foreach(Notification notification in unreadNotifications)
            {
                notification.Status = NotificationStatus.Read;
            }

            await db.SaveChangesAsync();
        }
        public async Task<bool> MarkNotificationAsDeletedAsync(Guid notificationId)
        {
            Notification? notification = await db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);
            if (notification == null)
                return false;

            notification.Status = NotificationStatus.Deleted;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeReminderNotificationStatusAsync(Guid notificationId, NotificationStatus status)
        {
            Notification? notification = await db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);
            if (notification == null)
                return false;

            if (notification.Type != NotificationType.Reminder)
                return false;
            // if (notification.Status == NotificationStatus.Deleted)
            //     return false;

            notification.Status = status;
            await db.SaveChangesAsync();

            return true;
        }
        public async Task<int> MarkOldReminderNotificationsAsDeletedAsync(Guid reminderId, DateTime cutoffUtc)
        {
            List<Notification> notifications = await db.Notifications
                .Where(n =>
                    n.ReminderId == reminderId &&
                    n.Type == NotificationType.Reminder &&
                    n.Status != NotificationStatus.Deleted &&
                    n.CreatedAt < cutoffUtc)
                .ToListAsync();

            foreach(Notification notification in notifications)
            {
                notification.Status = NotificationStatus.Deleted;
            }

            await db.SaveChangesAsync();
            return notifications.Count;
        }
        public async Task<Notification?> GetReminderNotificationForLocalDateAsync(Guid reminderId, DateOnly localDate, TimeZoneInfo timezone)
        {
            DateTime localStart = localDate.ToDateTime(TimeOnly.MinValue);
            DateTime localEnd = localDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

            DateTime utcStart = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(localStart, DateTimeKind.Unspecified),
                timezone
            );

            DateTime utcEnd = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(localEnd, DateTimeKind.Unspecified),
                timezone
            );

            return await db.Notifications
                .FirstOrDefaultAsync(n =>
                    n.ReminderId == reminderId &&
                    n.Type == NotificationType.Reminder &&
                    n.CreatedAt >= utcStart &&
                    n.CreatedAt < utcEnd);
        }
    }
}
