using backend.Enums;
using backend.Models;
using backend.Repositories.Interfaces;

namespace backend.BackgroundServices
{
    public class ReminderNotificationService(IServiceScopeFactory scopeFactory, ILogger<ReminderNotificationService> logger) : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("ReminderNotificationService started, interval {IntervalMinutes}m", Interval.TotalMinutes);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();

                    IReminderRepository reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
                    IHabitEntryRepository habitEntryRepository = scope.ServiceProvider.GetRequiredService<IHabitEntryRepository>();
                    INotificationRepository notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                    IMembershipRepository membershipRepository = scope.ServiceProvider.GetRequiredService<IMembershipRepository>();

                    List<Reminder> reminders = await reminderRepository.GetEnabledRemindersWithHabitAndMemberAsync();

                    foreach (Reminder reminder in reminders)
                    {
                        await TrySendReminderNotification(reminder, habitEntryRepository, notificationRepository, reminderRepository, membershipRepository);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ReminderNotificationService cycle failed");
                }

                await Task.Delay(Interval, cancellationToken);
            }

            logger.LogInformation("ReminderNotificationService stopped");
        }

        private async Task TrySendReminderNotification(Reminder reminder, IHabitEntryRepository habitEntryRepository, INotificationRepository notificationRepository, IReminderRepository reminderRepository, IMembershipRepository membershipRepository)
        {
            if (reminder.Habit.ReminderTime == null)
                return;

            bool isActiveMember = await membershipRepository.IsActiveMembershipAsync(
                reminder.Habit.TeamId,
                reminder.MemberId
            );

            if (!isActiveMember)
                return;

            TimeZoneInfo timezone = GetTimezone(reminder.Member.Timezone);

            DateTime nowUtc = DateTime.UtcNow;
            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone);

            DateOnly localToday = DateOnly.FromDateTime(localNow);
            TimeOnly localTimeNow = TimeOnly.FromDateTime(localNow);
            TimeOnly reminderTime = reminder.Habit.ReminderTime.Value;

            if (localTimeNow < reminderTime)
                return;

            if (WasAlreadySentToday(reminder.LastSentAt, localToday, timezone))
                return;

            bool alreadyLoggedToday = await habitEntryRepository.HasHabitEntryForLogDateAsync(reminder.HabitId, reminder.MemberId, localToday);

            if (alreadyLoggedToday)
                return;

            Notification notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = reminder.MemberId,
                UserType = UserType.Member,
                Content = $"Reminder: you have not logged \"{reminder.Habit.Name}\" today.",
                CreatedAt = nowUtc,
                Status = NotificationStatus.Unread,
                Type = NotificationType.Reminder
            };

            await notificationRepository.CreateNotificationAsync(notification);
            await reminderRepository.UpdateLastSentAtAsync(reminder.ReminderId, nowUtc);
        }

        private static bool WasAlreadySentToday(DateTime? lastSentAt, DateOnly localToday, TimeZoneInfo timezone)
        {
            if (lastSentAt == null)
                return false;

            DateTime lastSentUtc = DateTime.SpecifyKind(lastSentAt.Value, DateTimeKind.Utc);
            DateTime localLastSent = TimeZoneInfo.ConvertTimeFromUtc(lastSentUtc, timezone);

            return DateOnly.FromDateTime(localLastSent) == localToday;
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