using backend.Data;
using backend.Enums;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class HabitRepository(AppDbContext db) : IHabitRepository
{
    public async Task ArchiveExpiredActiveHabitsAsync()
    {
        DateTime now = DateTime.UtcNow;
        List<Habit> toArchive = await db.Habits.Where(h => h.HabitState == HabitState.Active && h.ExpiryDate != null && h.ExpiryDate <= now).ToListAsync();
        foreach( var habit in toArchive)
        {
            habit.HabitState = HabitState.Archived;
        }
        await db.SaveChangesAsync();
    }

    public async Task<bool> ArchiveHabitAsync(Guid habitId)
    {
        Habit? habit = await db.Habits.FirstOrDefaultAsync(h => h.HabitId == habitId);
        if(habit == null)
            return false;
        habit.HabitState = HabitState.Archived;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Habit> CreateHabitAsync(Habit habit)
    {
        db.Habits.Add(habit);
        await db.SaveChangesAsync();
        return habit;
    }

    public async Task<bool> DeleteHabitAsync(Guid habitId)
    {
        Habit? habit = await db.Habits.FirstOrDefaultAsync(h => h.HabitId == habitId);
        if(habit == null)
            return false;

        habit.HabitState = HabitState.Closed;

        List<HabitEntry> entries = await db.HabitEntries.Where(e => e.HabitId == habitId).ToListAsync();

        db.HabitEntries.RemoveRange(entries);
        db.Habits.Remove(habit);

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Habit>> GetActiveHabitsByTeamIdAsync(Guid teamId) => 
        await db.Habits.Where(h => h.TeamId == teamId && h.HabitState == HabitState.Active).ToListAsync();

    public async Task<List<Habit>> GetArchivedHabitsByTeamIdAsync(Guid teamId) =>
        await db.Habits.Where(h => h.TeamId == teamId && h.HabitState == HabitState.Archived).ToListAsync();

    public async Task<Habit?> GetHabitByIdAsync(Guid habitId) =>
        await db.Habits.FirstOrDefaultAsync(h => h.HabitId == habitId);

    public async Task<List<Habit>> GetHabitsByCreatorIdAsync(Guid creatorId) => 
        await db.Habits.Where(h => h.CreatorId == creatorId).ToListAsync();

    public async Task<List<Habit>> GetHabitsByTeamIdAsync(Guid teamId) =>
        await db.Habits.Where(h => h.TeamId == teamId).ToListAsync();

    public async Task<Habit?> GetHabitWithEntriesByIdAsync(Guid habitId) =>
        await db.Habits.Include(h => h.Entries).FirstOrDefaultAsync(h => h.HabitId == habitId);

    public async Task<bool> HabitExistsAsync(Guid habitId) =>
        await db.Habits.AnyAsync(h => h.HabitId == habitId);

    public async Task UpdateHabitAsync(Habit habit)
    {
        Habit? existing = await db.Habits.FirstOrDefaultAsync(h => h.HabitId == habit.HabitId);
        if (existing == null)
            throw new KeyNotFoundException("Habit not found.");

        existing.Name = habit.Name;
        existing.Goal = habit.Goal;
        existing.ExpiryDate = habit.ExpiryDate;

        await db.SaveChangesAsync();
    }

    public async Task UpdateHabitStateAsync(Guid habitId, HabitState state)
    {
        Habit? habit = await db.Habits.FirstOrDefaultAsync(h => h.HabitId == habitId);
        if(habit == null)
            throw new KeyNotFoundException("Habit not found.");
        habit.HabitState = state;
        await db.SaveChangesAsync();
    }
}