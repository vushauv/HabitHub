using backend.Enums;
using static backend.Tests.Unit.Services.Notification.NotificationTestIds;

namespace backend.Tests.Unit.Services.Notification;

[Trait("Category", "Unit")]
public class NotificationServiceMarkAllAsReadTests : NotificationServiceTestBase
{
    [Fact]
    public async Task MarkAllAsRead_NoType_MarksAllUnreadNotificationsAsRead()
    {
        var systemNotification = SeedNotification(
            notificationId: NotificationId,
            type: NotificationType.System,
            status: NotificationStatus.Unread);

        var reminderNotification = SeedNotification(
            notificationId: OtherNotificationId,
            type: NotificationType.Reminder,
            status: NotificationStatus.Unread);

        await Sut().MarkAllAsRead(UserId, UserType.Member, null);

        Assert.NotNull(_notifications.LastMarkAllAsReadCall);
        Assert.Equal(UserId, _notifications.LastMarkAllAsReadCall.Value.UserId);
        Assert.Equal(UserType.Member, _notifications.LastMarkAllAsReadCall.Value.UserType);
        Assert.Null(_notifications.LastMarkAllAsReadCall.Value.Type);
        Assert.Equal(NotificationStatus.Read, systemNotification.Status);
        Assert.Equal(NotificationStatus.Read, reminderNotification.Status);
    }

    [Fact]
    public async Task MarkAllAsRead_WithType_MarksOnlyThatTypeAsRead()
    {
        var systemNotification = SeedNotification(
            notificationId: NotificationId,
            type: NotificationType.System,
            status: NotificationStatus.Unread);

        var reminderNotification = SeedNotification(
            notificationId: OtherNotificationId,
            type: NotificationType.Reminder,
            status: NotificationStatus.Unread);

        await Sut().MarkAllAsRead(UserId, UserType.Member, NotificationType.System);

        Assert.NotNull(_notifications.LastMarkAllAsReadCall);
        Assert.Equal(NotificationType.System, _notifications.LastMarkAllAsReadCall.Value.Type);
        Assert.Equal(NotificationStatus.Read, systemNotification.Status);
        Assert.Equal(NotificationStatus.Unread, reminderNotification.Status);
    }

    [Fact]
    public async Task MarkAllAsRead_OtherUserNotifications_AreNotChanged()
    {
        var ownNotification = SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Unread);

        var otherUserNotification = SeedNotification(
            notificationId: OtherNotificationId,
            userId: OtherUserId,
            status: NotificationStatus.Unread);

        await Sut().MarkAllAsRead(UserId, UserType.Member, null);

        Assert.Equal(NotificationStatus.Read, ownNotification.Status);
        Assert.Equal(NotificationStatus.Unread, otherUserNotification.Status);
    }

    [Fact]
    public async Task MarkAllAsRead_ReadNotifications_RemainRead()
    {
        var notification = SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Read);

        await Sut().MarkAllAsRead(UserId, UserType.Member, null);

        Assert.Equal(NotificationStatus.Read, notification.Status);
    }
}