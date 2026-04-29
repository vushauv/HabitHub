using backend.Data;
using backend.Enums;
using backend.Models;
using Microsoft.EntityFrameworkCore;
namespace backend.Repositories;

public class HabitEntryRepository(AppDbContext db) : IHabitEntryRepository
{
    public Task<HabitEntry> CreateHabitEntryAsync(HabitEntry entry)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteHabitEntryAsync(Guid entryId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteHabitEntryByHabitMemberLogDateAsync(Guid habitId, Guid memberId, DateOnly logDate)
    {
        throw new NotImplementedException();
    }

    public Task<List<HabitEntry>> GetHabitEntriesByHabitAndLogDateRangeAsync(Guid habitId, DateOnly from, DateOnly to)
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

    public Task<HabitEntry?> GetHabitEntryByHabitMemberLogDateAsync(Guid habitId, Guid memberId, DateOnly logDate)
    {
        throw new NotImplementedException();
    }

    public Task<HabitEntry?> GetHabitEntryByIdAsync(Guid entryId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasHabitEntryForLogDateAsync(Guid habitId, Guid memberId, DateOnly logDate)
    {
        throw new NotImplementedException();
    }
}