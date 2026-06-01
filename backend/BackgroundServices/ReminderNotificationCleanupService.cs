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
            DateTime localTodayStart = localToday.ToDateTime(TimeOnly.MinValue);

            DateTime cutoffUtc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(localTodayStart, DateTimeKind.Unspecified),
                timezone
            );

            int deletedCount = await notificationRepository.MarkOldReminderNotificationsAsDeletedAsync(
                reminder.ReminderId,
                cutoffUtc
            );

            if (deletedCount == 0)
                return;

            logger.LogInformation(
                "Deleted {DeletedCount} old reminder notifications for reminder {ReminderId}, before local date {LocalDate}",
                deletedCount,
                reminder.ReminderId,
                localToday
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