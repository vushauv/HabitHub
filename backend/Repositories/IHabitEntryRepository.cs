using backend.Models;

namespace backend.Repositories;

public interface IHabitEntryRepository
{
    public Task<HabitEntry> CreateHabitEntryAsync(HabitEntry entry);
    public Task<HabitEntry?> GetHabitEntryByIdAsync(Guid entryId);
    public Task<HabitEntry?> GetHabitEntryByHabitMemberDateAsync(Guid habitId, Guid memberId, DateOnly logDate);
    public Task<bool> HasHabitEntryForDateAsync(Guid habitId, Guid memberId, DateOnly logDate);
    public Task<List<HabitEntry>> GetHabitEntriesByHabitIdAsync(Guid habitId);
    public Task<List<HabitEntry>> GetHabitEntriesByHabitAndMemberAsync(Guid habitId, Guid memberId);
    public Task<List<HabitEntry>> GetHabitEntriesByMemberIdAsync(Guid memberId);
    public Task<List<HabitEntry>> GetHabitEntriesByHabitAndDateRangeAsync(Guid habitId, DateOnly from, DateOnly to);
    public Task<List<HabitEntry>> GetHabitEntriesForLeaderboardAsync(Guid habitId);
    public Task<bool> DeleteHabitEntryAsync(Guid entryId);
    public Task<bool> DeleteHabitEntryByHabitMemberDateAsync(Guid habitId, Guid memberId, DateOnly logDate);
}