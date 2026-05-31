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
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
        private const int DaysBackToCheck = 7;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("ReminderNotificationCleanupService started, interval {IntervalHours}h", Interval.TotalHours);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();

                    IReminderRepository reminderRepository =
                        scope.ServiceProvider.GetRequiredService<IReminderRepository>();

                    INotificationRepository notificationRepository =
                        scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                    List<Reminder> reminders = await reminderRepository.GetEnabledRemindersWithHabitAndMemberAsync();

                    foreach (Reminder reminder in reminders)
                    {
                        await DeleteOldReminderNotifications(reminder, notificationRepository);
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

        private async Task DeleteOldReminderNotifications(
            Reminder reminder,
            INotificationRepository notificationRepository)
        {
            TimeZoneInfo timezone = GetTimezone(reminder.Member.Timezone);

            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);
            DateOnly localToday = DateOnly.FromDateTime(localNow);

            for (int i = 1; i <= DaysBackToCheck; i++)
            {
                DateOnly dateToClean = localToday.AddDays(-i);

                Notification? notification = await notificationRepository.GetReminderNotificationForLocalDateAsync(
                    reminder.ReminderId,
                    dateToClean,
                    timezone
                );

                if (notification == null)
                    continue;

                if (notification.Status == NotificationStatus.Deleted)
                    continue;

                bool updated = await notificationRepository.ChangeReminderNotificationStatusAsync(
                    notification.NotificationId,
                    NotificationStatus.Deleted
                );

                if (!updated)
                {
                    logger.LogWarning(
                        "Failed to delete old reminder notification {NotificationId} for reminder {ReminderId}",
                        notification.NotificationId,
                        reminder.ReminderId
                    );

                    continue;
                }

                logger.LogInformation(
                    "Deleted old reminder notification {NotificationId} for reminder {ReminderId}, local date {LocalDate}",
                    notification.NotificationId,
                    reminder.ReminderId,
                    dateToClean
                );
            }
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