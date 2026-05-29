using backend.Models;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Repositories.Interfaces;

namespace backend.Repositories;

public class HabitTeamRepository(AppDbContext db, ILogger<HabitTeamRepository> logger) : IHabitTeamRepository
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
        logger.LogInformation("Created habit team {TeamId} for creator {CreatorId}", team.TeamId, team.CreatorId);
        return team;
    }

    public async Task<bool> DeleteHabitTeamAsync(Guid teamId)
    {
        HabitTeam? team = await GetHabitTeamByIdAsync(teamId);
        if(team == null)
        {
            logger.LogWarning("Delete habit team skipped, team {TeamId} not found", teamId);
            return false;
        }

        db.HabitTeams.Remove(team);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted habit team {TeamId}", teamId);
        return true;
    }

    public async Task<List<HabitTeam>> GetAllHabitTeamsByCreatorAsync(Guid creatorId) =>
        await db.HabitTeams.Where(t => t.CreatorId == creatorId).ToListAsync();
    public async Task<List<HabitTeam>> GetHabitTeamsByIdsAsync(List<Guid> teamIds) =>
        await db.HabitTeams.Where(t => teamIds.Contains(t.TeamId)).ToListAsync();

    public async Task<HabitTeam?> GetHabitTeamByIdAsync(Guid teamId) =>
        await db.HabitTeams.SingleOrDefaultAsync(t => t.TeamId == teamId);
    public async Task<HabitTeam?> GetHabitTeamByHabitIdAsync(Guid habitId) =>
        await db.HabitTeams.FirstOrDefaultAsync(team => team.Habits.Any(habit => habit.HabitId == habitId));

}