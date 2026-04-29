using backend.Enums;
using backend.Models;

namespace backend.Repositories;

public class HabitRepository : IHabitRepository
{
    public Task ArchiveExpiredActiveHabitsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> ArchiveHabitAsync(Guid habitId)
    {
        throw new NotImplementedException();
    }

    public Task<Habit> CreateHabitAsync(Habit habit)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteHabitAsync(Guid habitId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Habit>> GetActiveHabitsByTeamIdAsync(Guid teamId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Habit>> GetArchivedHabitsByTeamIdAsync(Guid teamId)
    {
        throw new NotImplementedException();
    }

    public Task<Habit?> GetHabitByIdAsync(Guid habitId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Habit>> GetHabitsByCreatorIdAsync(Guid creatorId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Habit>> GetHabitsByTeamIdAsync(Guid teamId)
    {
        throw new NotImplementedException();
    }

    public Task<Habit?> GetHabitWithEntriesByIdAsync(Guid habitId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HabitExistsAsync(Guid habitId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateHabitAsync(Habit habit)
    {
        throw new NotImplementedException();
    }

    public Task UpdateHabitStateAsync(Guid habitId, HabitState state)
    {
        throw new NotImplementedException();
    }
}