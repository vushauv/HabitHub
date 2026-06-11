using backend.Enums;
using backend.Service;
using static backend.Tests.Unit.Services.Notification.NotificationTestIds;

namespace backend.Tests.Unit.Services.Notification;

public abstract class NotificationServiceTestBase
{
    protected readonly FakeNotificationRepository _notifications = new();
    protected readonly FakeLogger<NotificationService> _logger = new();
    protected NotificationService Sut() => new(_notifications, _logger);
    protected static backend.Models.Notification MakeNotification(
        Guid? notificationId = null,
        Guid? userId = null,
        UserType userType = UserType.Member,
        string content = "Test notification",
        DateTime? createdAt = null,
        NotificationStatus status = NotificationStatus.Unread,
        NotificationType type = NotificationType.System)
        => new()
        {
            NotificationId = notificationId ?? NotificationId,
            UserId = userId ?? UserId,
            UserType = userType,
            Content = content,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Status = status,
            Type = type
        };
    protected backend.Models.Notification SeedNotification(
        Guid? notificationId = null,
        Guid? userId = null,
        UserType userType = UserType.Member,
        string content = "Test notification",
        DateTime? createdAt = null,
        NotificationStatus status = NotificationStatus.Unread,
        NotificationType type = NotificationType.System)
    {
        var notification = MakeNotification(notificationId, userId, userType, content, createdAt, status, type);
        _notifications.Notifications.Add(notification);
        return notification;
    }
}