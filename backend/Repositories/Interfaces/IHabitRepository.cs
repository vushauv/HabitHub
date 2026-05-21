using backend.Enums;
using backend.Models;

namespace backend.Repositories.Interfaces;

public interface IHabitRepository
{
    public Task<Habit> CreateHabitAsync(Habit habit);
    public Task<Habit?> GetHabitByIdAsync(Guid habitId);
    public Task<Habit?> GetHabitWithEntriesByIdAsync(Guid habitId);
    public Task<List<Habit>> GetHabitsByTeamIdAsync(Guid teamId);
    public Task<List<Habit>> GetActiveHabitsByTeamIdAsync(Guid teamId);
    public Task<List<Guid>> GetActiveHabitIdsWithReminderTimeByTeamIdAsync(Guid teamId);
    public Task<List<Habit>> GetArchivedHabitsByTeamIdAsync(Guid teamId);
    public Task<List<Habit>> GetHabitsByCreatorIdAsync(Guid creatorId);
    public Task UpdateHabitAsync(Habit habit);
    public Task UpdateHabitStateAsync(Guid habitId, HabitState state);
    public Task<bool> ArchiveHabitAsync(Guid habitId);
    public Task<bool> DeleteHabitAsync(Guid habitId);
    public Task<bool> HabitExistsAsync(Guid habitId);
    public Task ArchiveExpiredActiveHabitsAsync();
}