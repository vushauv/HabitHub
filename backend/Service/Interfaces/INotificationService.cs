using backend.Dtos.NotificationDtos;
using backend.Enums;

namespace backend.Service.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotifications(Guid userId, NotificationType? type);
        Task<NotificationCountDto> GetUnreadCount(Guid userId, NotificationType? type);
        Task MarkAsRead(Guid userId, Guid notificationId);
        Task DeleteNotification(Guid userId, Guid notificationId);
        Task MarkAllAsRead(Guid userId, NotificationType? type);
    }
}