using backend.Enums;
using backend.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend.Tests.Unit.Services.Notification;

public sealed class FakeNotificationRepository : INotificationRepository
{
    public List<backend.Models.Notification> Notifications { get; } = new();
    public List<(Guid UserId, UserType UserType, NotificationType Type)> VisibleCalls { get; } = new();
    public List<(Guid UserId, UserType UserType, NotificationType Type)> CountCalls { get; } = new();
    public Guid? LastMarkedAsReadId { get; private set; }
    public Guid? LastDeletedId { get; private set; }
    public (Guid UserId, UserType UserType, NotificationType? Type)? LastMarkAllAsReadCall { get; private set; }
    public bool MarkAsReadResult { get; set; } = true;
    public bool DeleteResult { get; set; } = true;
    public Task<List<backend.Models.Notification>> GetVisibleNotificationsForUserByTypeAsync(Guid userId, UserType userType, NotificationType type)
    {
        VisibleCalls.Add((userId, userType, type));

        return Task.FromResult(Notifications
            .Where(n =>
                n.UserId == userId &&
                n.UserType == userType &&
                n.Type == type &&
                n.Status != NotificationStatus.Deleted)
            .ToList());
    }
    public Task<int> GetUnreadNotificationsCountForUserByTypeAsync(Guid userId, UserType userType, NotificationType type)
    {
        CountCalls.Add((userId, userType, type));

        return Task.FromResult(Notifications.Count(n =>
            n.UserId == userId &&
            n.UserType == userType &&
            n.Type == type &&
            n.Status == NotificationStatus.Unread));
    }
    public Task<backend.Models.Notification?> GetNotificationByIdAsync(Guid notificationId)
        => Task.FromResult(Notifications.FirstOrDefault(n => n.NotificationId == notificationId));
    public Task<backend.Models.Notification> CreateNotificationAsync(backend.Models.Notification notification)
    {
        Notifications.Add(notification);
        return Task.FromResult(notification);
    }
    public Task<bool> MarkNotificationAsReadAsync(Guid notificationId)
    {
        LastMarkedAsReadId = notificationId;

        if (MarkAsReadResult)
        {
            var notification = Notifications.FirstOrDefault(n => n.NotificationId == notificationId);
            if (notification != null)
                notification.Status = NotificationStatus.Read;
        }

        return Task.FromResult(MarkAsReadResult);
    }
    public Task MarkAllUnreadNotificationsAsReadAsync(Guid userId, UserType userType, NotificationType? type)
    {
        LastMarkAllAsReadCall = (userId, userType, type);

        foreach (var notification in Notifications.Where(n =>
            n.UserId == userId &&
            n.UserType == userType &&
            n.Status == NotificationStatus.Unread &&
            (type == null || n.Type == type.Value)))
        {
            notification.Status = NotificationStatus.Read;
        }

        return Task.CompletedTask;
    }
    public Task<bool> MarkNotificationAsDeletedAsync(Guid notificationId)
    {
        LastDeletedId = notificationId;

        if (DeleteResult)
        {
            var notification = Notifications.FirstOrDefault(n => n.NotificationId == notificationId);
            if (notification != null)
                notification.Status = NotificationStatus.Deleted;
        }

        return Task.FromResult(DeleteResult);
    }
    public Task<bool> ChangeReminderNotificationStatusAsync(Guid notificationId, NotificationStatus status) => throw new NotImplementedException();
    public Task<int> MarkOldReminderNotificationsAsDeletedAsync(Guid reminderId, DateTime cutoffUtc) => throw new NotImplementedException();
    public Task<backend.Models.Notification?> GetReminderNotificationForLocalDateAsync(Guid reminderId, DateOnly localDate, TimeZoneInfo timezone) => throw new NotImplementedException();
}
public sealed class FakeLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;
    public bool IsEnabled(LogLevel logLevel)
        => false;
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    { }
}
public static class NotificationTestIds
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid NotificationId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid OtherNotificationId = Guid.Parse("44444444-4444-4444-4444-444444444444");
}