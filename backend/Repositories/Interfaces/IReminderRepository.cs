using backend.Models;

namespace backend.Repositories.Interfaces
{
    public interface IReminderRepository
    {
        Task<Reminder?> GetReminderByHabitAndMemberAsync(Guid habitId, Guid memberId);
        Task<List<Reminder>> GetEnabledRemindersWithHabitAndMemberAsync();
        Task<Reminder> CreateReminderAsync(Reminder reminder);
        Task CreateMissingRemindersForHabitAsync(Guid habitId, List<Guid> memberIds);
        Task CreateMissingRemindersForMemberAsync(Guid memberId, List<Guid> habitIds);
        Task<bool> SetHabitReminderTimeAsync(Guid habitId, DateTime reminderTime);
        Task<bool> ClearHabitReminderTimeAsync(Guid habitId);
        Task<bool> SetReminderEnabledAsync(Guid habitId, Guid memberId, bool enabled);
        Task<bool> UpdateLastSentAtAsync(Guid reminderId, DateTime lastSentAt);
        Task DisableAllRemindersForHabitAsync(Guid habitId);
    }
}
