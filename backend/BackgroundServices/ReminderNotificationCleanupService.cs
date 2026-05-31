using backend.Enums;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.BackgroundServices
{
    public class ReminderNotificationCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReminderNotificationCleanupService> logger
    ) : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
        private static readonly TimeOnly CleanupTime = new TimeOnly(23, 59);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("ReminderNotificationCleanupService started, interval {IntervalMinutes}m", Interval.TotalMinutes);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();

                    IReminderRepository reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
                    INotificationRepository notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                    List<Reminder> reminders = await reminderRepository.GetEnabledRemindersWithHabitAndMemberAsync();

                    foreach (Reminder reminder in reminders)
                    {
                        await TryDeleteTodayReminderNotification(reminder, notificationRepository);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ReminderNotificationCleanupService cycle failed");
                }

                await Task.Delay(Interval, cancellationToken);
            }

            logger.LogInformation("ReminderNotificationCleanupService stopped");
        }

        private async Task TryDeleteTodayReminderNotification(Reminder reminder, INotificationRepository notificationRepository)
        {
            TimeZoneInfo timezone = GetTimezone(reminder.Member.Timezone);

            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);
            TimeOnly localTimeNow = TimeOnly.FromDateTime(localNow);

            if (localTimeNow < CleanupTime)
                return;

            DateOnly localToday = DateOnly.FromDateTime(localNow);

            Notification? notification = await notificationRepository.GetReminderNotificationForLocalDateAsync(
                reminder.ReminderId,
                localToday,
                timezone
            );

            if (notification == null)
                return;

            if (notification.Status == NotificationStatus.Deleted)
                return;

            bool updated = await notificationRepository.ChangeReminderNotificationStatusAsync(
                notification.NotificationId,
                NotificationStatus.Deleted
            );

            if (!updated)
            {
                logger.LogWarning(
                    "Failed to delete reminder notification {NotificationId} for reminder {ReminderId}",
                    notification.NotificationId,
                    reminder.ReminderId
                );

                return;
            }

            logger.LogInformation(
                "Deleted expired reminder notification {NotificationId} for reminder {ReminderId}",
                notification.NotificationId,
                reminder.ReminderId
            );
        }

        private TimeZoneInfo GetTimezone(string timezoneId)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            }
            catch
            {
                logger.LogWarning("Invalid timezone {TimezoneId}, falling back to UTC", timezoneId);
                return TimeZoneInfo.Utc;
            }
        }
    }
}