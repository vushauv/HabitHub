using backend.Enums;
using backend.Exceptions;
using static backend.Tests.Unit.Services.Notification.NotificationTestIds;

namespace backend.Tests.Unit.Services.Notification;

[Trait("Category", "Unit")]
public class NotificationServiceDeleteNotificationTests : NotificationServiceTestBase
{
    [Fact]
    public async Task DeleteNotification_OwnNotification_MarksNotificationAsDeleted()
    {
        var notification = SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Unread);

        await Sut().DeleteNotification(UserId, UserType.Member, NotificationId);

        Assert.Equal(NotificationId, _notifications.LastDeletedId);
        Assert.Equal(NotificationStatus.Deleted, notification.Status);
    }

    [Fact]
    public async Task DeleteNotification_NotificationNotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().DeleteNotification(UserId, UserType.Member, NotificationId)
        );

        Assert.Null(_notifications.LastDeletedId);
    }

    [Fact]
    public async Task DeleteNotification_DeletedNotification_ThrowsNotFound()
    {
        SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Deleted);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().DeleteNotification(UserId, UserType.Member, NotificationId)
        );

        Assert.Null(_notifications.LastDeletedId);
    }

    [Fact]
    public async Task DeleteNotification_OtherUserNotification_ThrowsForbidden()
    {
        SeedNotification(
            notificationId: NotificationId,
            userId: OtherUserId);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().DeleteNotification(UserId, UserType.Member, NotificationId)
        );

        Assert.Null(_notifications.LastDeletedId);
    }

    [Fact]
    public async Task DeleteNotification_OtherUserTypeNotification_ThrowsForbidden()
    {
        SeedNotification(
            notificationId: NotificationId,
            userId: UserId,
            userType: UserType.Creator);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Sut().DeleteNotification(UserId, UserType.Member, NotificationId)
        );

        Assert.Null(_notifications.LastDeletedId);
    }

    [Fact]
    public async Task DeleteNotification_RepositoryReturnsFalse_ThrowsNotFound()
    {
        SeedNotification(
            notificationId: NotificationId,
            status: NotificationStatus.Unread);

        _notifications.DeleteResult = false;

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Sut().DeleteNotification(UserId, UserType.Member, NotificationId)
        );

        Assert.Equal(NotificationId, _notifications.LastDeletedId);
    }
}