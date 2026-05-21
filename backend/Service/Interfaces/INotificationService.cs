using backend.Dtos.NotificationDtos;
using backend.Enums;

namespace backend.Service.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotifications(Guid userId, UserType userType, NotificationType? type);
        Task<NotificationCountDto> GetUnreadCount(Guid userId, UserType userType, NotificationType? type);
        Task MarkAsRead(Guid userId, UserType userType, Guid notificationId);
        Task DeleteNotification(Guid userId, UserType userType, Guid notificationId);
        Task MarkAllAsRead(Guid userId, UserType userType, NotificationType? type);
    }
}