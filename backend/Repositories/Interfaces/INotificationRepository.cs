using backend.Enums;
using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetVisibleNotificationsForUserByTypeAsync(Guid userId, UserType userType, NotificationType type);
        Task<int> GetUnreadNotificationsCountForUserByTypeAsync(Guid userId, UserType userType, NotificationType type);
        Task<Notification?> GetNotificationByIdAsync(Guid notificationId);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
        Task<bool> MarkNotificationAsDeletedAsync(Guid notificationId);
    }
}
