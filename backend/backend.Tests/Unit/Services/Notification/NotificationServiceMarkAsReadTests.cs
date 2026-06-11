using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Notification.NotificationTestIds;

namespace backend.Tests.Unit.Services.Notification;

[Trait("Category", "Unit")]
public class NotificationServiceMarkAsReadTests : NotificationServiceTestBase
{
    [Fact]
    public async Task MarkAsRead_OwnNotification_MarksNotificationAsRead()
    {
        var notification = SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Unread);

        await Sut().MarkAsRead(UserId, UserType.Member, NotificationId);

        Assert.Equal(NotificationId, _notifications.LastMarkedAsReadId);
        Assert.Equal(NotificationStatus.Read, notification.Status);
    }

    [Fact]
    public async Task MarkAsRead_NotificationNotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().MarkAsRead(UserId, UserType.Member, NotificationId)
        );

        Assert.Null(_notifications.LastMarkedAsReadId);
    }

    [Fact]
    public async Task MarkAsRead_DeletedNotification_ThrowsNotFound()
    {
        SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Deleted);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().MarkAsRead(UserId, UserType.Member, NotificationId)
        );

        Assert.Null(_notifications.LastMarkedAsReadId);
    }

    [Fact]
    public async Task MarkAsRead_OtherUserNotification_ThrowsForbidden()
    {
        SeedNotification(
            notificationId: NotificationId,
            userId: OtherUserId);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().MarkAsRead(UserId, UserType.Member, NotificationId)
        );

        Assert.Null(_notifications.LastMarkedAsReadId);
    }

    [Fact]
    public async Task MarkAsRead_OtherUserTypeNotification_ThrowsForbidden()
    {
        SeedNotification(
            notificationId: NotificationId,
            userId: UserId,
            userType: UserType.Creator);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().MarkAsRead(UserId, UserType.Member, NotificationId)
        );

        Assert.Null(_notifications.LastMarkedAsReadId);
    }

    [Fact]
    public async Task MarkAsRead_RepositoryReturnsFalse_ThrowsNotFound()
    {
        SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Unread);

        _notifications.MarkAsReadResult = false;

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().MarkAsRead(UserId, UserType.Member, NotificationId)
        );

        Assert.Equal(NotificationId, _notifications.LastMarkedAsReadId);
    }
}