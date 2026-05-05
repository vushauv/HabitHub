using backend.Data;
using backend.Enums;
using backend.Models;
using Microsoft.EntityFrameworkCore;
namespace backend.Repositories;

public class HabitEntryRepository(AppDbContext db) : IHabitEntryRepository
{
    public async Task<HabitEntry> CreateHabitEntryAsync(HabitEntry entry)
    {
        db.HabitEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteHabitEntryAsync(Guid entryId)
    {
        HabitEntry? entry = await db.HabitEntries.FirstOrDefaultAsync(e => e.EntryId == entryId);
        if (entry == null)
            return false;
        db.HabitEntries.Remove(entry);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteHabitEntryByHabitMemberLogDateAsync(Guid habitId, Guid memberId, DateOnly logDate)
    {
        HabitEntry? entry = await db.HabitEntries.FirstOrDefaultAsync(e => e.HabitId == habitId && e.MemberId == memberId && e.LogDate == logDate);
        if(entry == null)
            return false;
        
        db.HabitEntries.Remove(entry);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<HabitEntry>> GetHabitEntriesByHabitAndLogDateRangeAsync(Guid habitId, DateOnly from, DateOnly to) =>
        await db.HabitEntries.Where(e => e.HabitId == habitId && e.LogDate >= from && e.LogDate <= to).ToListAsync();

    public async Task<List<HabitEntry>> GetHabitEntriesByHabitAndMemberAsync(Guid habitId, Guid memberId) =>
        await db.HabitEntries.Where(e => e.HabitId == habitId && e.MemberId == memberId).ToListAsync();

    public async Task<List<HabitEntry>> GetHabitEntriesByHabitIdAsync(Guid habitId) => 
        await db.HabitEntries.Where(e => e.HabitId == habitId).ToListAsync();

    public async Task<List<HabitEntry>> GetHabitEntriesByMemberIdAsync(Guid memberId) =>
        await db.HabitEntries.Where(e => e.MemberId == memberId).ToListAsync();

    public async Task<List<HabitEntry>> GetHabitEntriesForLeaderboardAsync(Guid habitId) =>
        await db.HabitEntries.Where(e => e.HabitId == habitId).ToListAsync();

    public async Task<HabitEntry?> GetHabitEntryByHabitMemberLogDateAsync(Guid habitId, Guid memberId, DateOnly logDate) =>
        await db.HabitEntries.FirstOrDefaultAsync(e => e.HabitId == habitId && e.MemberId == memberId && e.LogDate == logDate);

    public async Task<HabitEntry?> GetHabitEntryByIdAsync(Guid entryId) =>
        await db.HabitEntries.FirstOrDefaultAsync(e => e.EntryId == entryId);

    public async Task<bool> HasHabitEntryForLogDateAsync(Guid habitId, Guid memberId, DateOnly logDate) =>
        await db.HabitEntries.AnyAsync(e => e.HabitId == habitId && e.MemberId == memberId && e.LogDate == logDate);
}