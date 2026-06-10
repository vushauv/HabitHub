using backend.Enums;
using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetVisibleNotificationsForUserByTypeAsync(Guid userId, NotificationType type);
        Task<int> GetUnreadNotificationsCountForUserByTypeAsync(Guid userId, NotificationType type);
        Task<Notification?> GetNotificationByIdAsync(Guid notificationId);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
        Task MarkAllUnreadNotificationsAsReadAsync(Guid userId, NotificationType? type);
        Task<bool> MarkNotificationAsDeletedAsync(Guid notificationId);
        Task<bool> ChangeReminderNotificationStatusAsync(Guid notificationId, NotificationStatus status);
        Task<int> MarkOldReminderNotificationsAsDeletedAsync(Guid reminderId, DateTime cutoffUtc);
        Task<Notification?> GetReminderNotificationForLocalDateAsync(Guid reminderId, DateOnly localDate, TimeZoneInfo timezone);
    }
}
