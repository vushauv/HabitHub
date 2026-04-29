using backend.Models;

namespace backend.Repositories;

public class HabitEntryRepository : IHabitEntryRepository
{
    public Task<HabitEntry> CreateHabitEntryAsync(HabitEntry entry)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteHabitEntryAsync(Guid entryId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteHabitEntryByHabitMemberDateAsync(Guid habitId, Guid memberId, DateOnly logDate)
    {
        throw new NotImplementedException();
    }

    public Task<List<HabitEntry>> GetHabitEntriesByHabitAndDateRangeAsync(Guid habitId, DateOnly from, DateOnly to)
    {
        throw new NotImplementedException();
    }

    public Task<List<HabitEntry>> GetHabitEntriesByHabitAndMemberAsync(Guid habitId, Guid memberId)
    {
        throw new NotImplementedException();
    }

    public Task<List<HabitEntry>> GetHabitEntriesByHabitIdAsync(Guid habitId)
    {
        throw new NotImplementedException();
    }

    public Task<List<HabitEntry>> GetHabitEntriesByMemberIdAsync(Guid memberId)
    {
        throw new NotImplementedException();
    }

    public Task<List<HabitEntry>> GetHabitEntriesForLeaderboardAsync(Guid habitId)
    {
        throw new NotImplementedException();
    }

    public Task<HabitEntry?> GetHabitEntryByHabitMemberDateAsync(Guid habitId, Guid memberId, DateOnly logDate)
    {
        throw new NotImplementedException();
    }

    public Task<HabitEntry?> GetHabitEntryByIdAsync(Guid entryId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasHabitEntryForDateAsync(Guid habitId, Guid memberId, DateOnly logDate)
    {
        throw new NotImplementedException();
    }
}