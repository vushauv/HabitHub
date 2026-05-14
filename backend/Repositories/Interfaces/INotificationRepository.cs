using backend.Enums;
using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetVisibleNotificationsForUserAsync(Guid userId, UserType userType);
        Task<int> GetUnreadNotificationsCountForUserAsync(Guid userId, UserType userType);
        Task<Notification?> GetNotificationByIdAsync(Guid notificationId);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
        Task MarkNotificationAsDeletedAsync(Guid notificationId);
    }
}
