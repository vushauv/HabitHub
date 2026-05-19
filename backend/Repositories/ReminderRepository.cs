using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using backend.Enums;

namespace backend.Repositories
{
    public class ReminderRepository(AppDbContext db): IReminderRepository
    {
        public async Task<Reminder?> GetReminderByHabitAndMemberAsync(Guid habitId, Guid memberId) =>
            await db.Reminders.FirstOrDefaultAsync(r => r.HabitId == habitId && r.MemberId == memberId);
        public async Task<List<Reminder>> GetEnabledRemindersWithHabitAndMemberAsync() =>
             await db.Reminders.Include(r => r.Habit).Include(r => r.Member)
                .Where(r =>
                    r.Enabled &&
                    r.Habit.HabitState == HabitState.Active &&
                    r.Habit.ReminderTime != null)
                .ToListAsync();
        public async Task<Reminder> CreateReminderAsync(Reminder reminder)
        {
            db.Reminders.Add(reminder);
            await db.SaveChangesAsync();

            return reminder;
        }
        public async Task CreateMissingRemindersForHabitAsync(Guid habitId, List<Guid> memberIds)
        {
            if (memberIds.Count == 0)
                return;

            List<Guid> existingMemberIds = await db.Reminders
                .Where(r => r.HabitId == habitId)
                .Select(r => r.MemberId)
                .ToListAsync();

            List<Reminder> newReminders = memberIds
                .Where(memberId => !existingMemberIds.Contains(memberId))
                .Select(memberId => new Reminder
                {
                    ReminderId = Guid.NewGuid(),
                    HabitId = habitId,
                    MemberId = memberId,
                    Enabled = true,
                    LastSentAt = null
                })
                .ToList();

            if (newReminders.Count == 0)
                return;

            db.Reminders.AddRange(newReminders);
            await db.SaveChangesAsync();
        }
        public async Task CreateMissingRemindersForMemberAsync(Guid memberId, List<Guid> habitIds)
        {
            if (habitIds.Count == 0)
                return;

            List<Guid> existingHabitIds = await db.Reminders
                .Where(r => r.MemberId == memberId && habitIds.Contains(r.HabitId))
                .Select(r => r.HabitId)
                .ToListAsync();

            List<Reminder> newReminders = habitIds
                .Where(habitId => !existingHabitIds.Contains(habitId))
                .Select(habitId => new Reminder
                {
                    ReminderId = Guid.NewGuid(),
                    HabitId = habitId,
                    MemberId = memberId,
                    Enabled = true,
                    LastSentAt = null
                })
                .ToList();

            if (newReminders.Count == 0)
                return;

            db.Reminders.AddRange(newReminders);
            await db.SaveChangesAsync();
        }
        public async Task<bool> SetHabitReminderTimeAsync(Guid habitId, DateTime reminderTime)
        {
            Habit? habit = await db.Habits.FirstOrDefaultAsync(h => h.HabitId == habitId);
            if (habit == null)
                return false;

            habit.ReminderTime = reminderTime;
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ClearHabitReminderTimeAsync(Guid habitId)
        {
            Habit? habit = await db.Habits.FirstOrDefaultAsync(h => h.HabitId == habitId);
            if (habit == null)
                return false;

            habit.ReminderTime = null;
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> SetReminderEnabledAsync(Guid habitId, Guid memberId, bool enabled)
        {
            Reminder? reminder = await db.Reminders.FirstOrDefaultAsync(r => r.HabitId == habitId && r.MemberId == memberId);
            if (reminder == null)
                return false;

            reminder.Enabled = enabled;
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateLastSentAtAsync(Guid reminderId, DateTime lastSentAt)
        {
            Reminder? reminder = await db.Reminders.FirstOrDefaultAsync(r => r.ReminderId == reminderId);
            if (reminder == null)
                return false;

            reminder.LastSentAt = lastSentAt;
            await db.SaveChangesAsync();
            return true;
        }
        public async Task DisableAllRemindersForHabitAsync(Guid habitId)
        {
            List<Reminder> reminders = await db.Reminders
                .Where(r => r.HabitId == habitId && r.Enabled)
                .ToListAsync();

            if (reminders.Count == 0)
                return;

            foreach (Reminder reminder in reminders)
                reminder.Enabled = false;

            await db.SaveChangesAsync();
        }
    }
}
