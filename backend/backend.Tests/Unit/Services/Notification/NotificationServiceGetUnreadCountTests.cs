using backend.Enums;
using static backend.Tests.Unit.Services.Notification.NotificationTestIds;

namespace backend.Tests.Unit.Services.Notification;

[Trait("Category", "Unit")]
public class NotificationServiceGetUnreadCountTests : NotificationServiceTestBase
{
    [Fact]
    public async Task GetUnreadCount_NoType_ReturnsSystemAndReminderUnreadCount()
    {
        SeedNotification(
            notificationId: NotificationId,
            type: NotificationType.System,
            status: NotificationStatus.Unread);

        SeedNotification(
            notificationId: OtherNotificationId,
            type: NotificationType.Reminder,
            status: NotificationStatus.Unread);

        var result = await Sut().GetUnreadCount(UserId, UserType.Member, null);

        Assert.Equal(2, result.Count);
        Assert.Contains(_notifications.CountCalls, c => c.Type == NotificationType.System);
        Assert.Contains(_notifications.CountCalls, c => c.Type == NotificationType.Reminder);
    }

    [Fact]
    public async Task GetUnreadCount_WithType_ReturnsOnlyCountForThatType()
    {
        SeedNotification(
            notificationId: NotificationId,
            type: NotificationType.System,
            status: NotificationStatus.Unread);

        SeedNotification(
            notificationId: OtherNotificationId,
            type: NotificationType.Reminder,
            status: NotificationStatus.Unread);

        var result = await Sut().GetUnreadCount(UserId, UserType.Member, NotificationType.System);

        Assert.Equal(1, result.Count);
        Assert.Single(_notifications.CountCalls);
        Assert.Equal(NotificationType.System, _notifications.CountCalls[0].Type);
    }

    [Fact]
    public async Task GetUnreadCount_ReadNotifications_AreNotCounted()
    {
        SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Read);

        var result = await Sut().GetUnreadCount(UserId, UserType.Member, NotificationType.System);

        Assert.Equal(0, result.Count);
    }

    [Fact]
    public async Task GetUnreadCount_DeletedNotifications_AreNotCounted()
    {
        SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Deleted);

        var result = await Sut().GetUnreadCount(UserId, UserType.Member, NotificationType.System);

        Assert.Equal(0, result.Count);
    }

    [Fact]
    public async Task GetUnreadCount_OtherUserNotifications_AreNotCounted()
    {
        SeedNotification(
            notificationId: NotificationId,
            userId: OtherUserId,
            status: NotificationStatus.Unread);

        var result = await Sut().GetUnreadCount(UserId, UserType.Member, NotificationType.System);

        Assert.Equal(0, result.Count);
    }
}