using backend.Models;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class HabitTeamRepository(AppDbContext db) : IHabitTeamRepository
{
    public async Task<bool> CheckOwnershipOfTeamAsync(Guid teamId, Guid creatorId)
    {
        HabitTeam? team = await GetHabitTeamByIdAsync(teamId);
        if(team == null)
            return false;

        if(team.CreatorId == creatorId)
            return true;
        return false;
    }

    public async Task<HabitTeam> CreateHabitTeamAsync(HabitTeam team)
    {
        db.HabitTeams.Add(team);
        await db.SaveChangesAsync();
        return team;
    }

    public async Task<bool> DeleteHabitTeamAsync(Guid teamId)
    {
        HabitTeam? team = await GetHabitTeamByIdAsync(teamId);
        if(team == null)
            return false;

        db.HabitTeams.Remove(team);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<HabitTeam>> GetAllHabitTeamsByCreatorAsync(Guid creatorId) =>
        await db.HabitTeams.Where(t => t.CreatorId == creatorId).ToListAsync();

    public async Task<HabitTeam?> GetHabitTeamByIdAsync(Guid teamId) =>
        await db.HabitTeams.SingleOrDefaultAsync(t => t.TeamId == teamId);
}