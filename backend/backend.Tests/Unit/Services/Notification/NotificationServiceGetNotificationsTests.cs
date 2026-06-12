using backend.Enums;
using static backend.Tests.Unit.Services.Notification.NotificationTestIds;

namespace backend.Tests.Unit.Services.Notification;

[Trait("Category", "Unit")]
public class NotificationServiceGetNotificationsTests : NotificationServiceTestBase
{
    [Fact]
    public async Task GetNotifications_NoType_ReturnsSystemAndReminderNotifications()
    {
        SeedNotification(
            notificationId: NotificationId,
            type: NotificationType.System,
            content: "System notification");

        SeedNotification(
            notificationId: OtherNotificationId,
            type: NotificationType.Reminder,
            content: "Reminder notification");

        var result = await Sut().GetNotifications(UserId, UserType.Member, null);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, n => n.NotificationId == NotificationId);
        Assert.Contains(result, n => n.NotificationId == OtherNotificationId);
        Assert.Contains(_notifications.VisibleCalls, c => c.Type == NotificationType.System);
        Assert.Contains(_notifications.VisibleCalls, c => c.Type == NotificationType.Reminder);
    }

    [Fact]
    public async Task GetNotifications_WithType_ReturnsOnlyNotificationsOfThatType()
    {
        SeedNotification(
            notificationId: NotificationId,
            type: NotificationType.System);

        SeedNotification(
            notificationId: OtherNotificationId,
            type: NotificationType.Reminder);

        var result = await Sut().GetNotifications(UserId, UserType.Member, NotificationType.System);

        Assert.Single(result);
        Assert.Equal(NotificationId, result[0].NotificationId);
        Assert.Equal(NotificationType.System, result[0].Type);
        Assert.Single(_notifications.VisibleCalls);
        Assert.Equal(NotificationType.System, _notifications.VisibleCalls[0].Type);
    }
    [Fact]
    public async Task GetNotifications_ReturnsNewestFirst()
    {
        var olderDate = DateTime.UtcNow.AddMinutes(-10);
        var newerDate = DateTime.UtcNow;

        SeedNotification(
            notificationId: NotificationId,
            createdAt: olderDate,
            content: "Older");

        SeedNotification(
            notificationId: OtherNotificationId,
            createdAt: newerDate,
            content: "Newer");

        var result = await Sut().GetNotifications(UserId, UserType.Member, NotificationType.System);

        Assert.Equal(2, result.Count);
        Assert.Equal(OtherNotificationId, result[0].NotificationId);
        Assert.Equal(NotificationId, result[1].NotificationId);
    }
    [Fact]
    public async Task GetNotifications_DeletedNotification_IsNotReturned()
    {
        SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Deleted);

        var result = await Sut().GetNotifications(UserId, UserType.Member, NotificationType.System);
        Assert.Empty(result);
    }
    [Fact]
    public async Task GetNotifications_OtherUserNotification_IsNotReturned()
    {
        SeedNotification(
            notificationId: NotificationId,
            userId: OtherUserId);

        var result = await Sut().GetNotifications(UserId, UserType.Member, NotificationType.System);
        Assert.Empty(result);
    }
}