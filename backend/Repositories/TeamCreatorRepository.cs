using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class TeamCreatorRepository(AppDbContext db) : ITeamCreatorRepository
{
    public async Task<TeamCreator?> GetCreatorByEmailAsync(string email) => 
        await db.TeamCreators.SingleOrDefaultAsync(c => c.Email == email);
    public async Task<TeamCreator?> GetCreatorByIdAsync(Guid creatorId) =>
        await db.TeamCreators.FindAsync(creatorId);
    public async Task<TeamCreator> CreateCreatorAsync(TeamCreator creator)
    {
        db.TeamCreators.Add(creator);
        await db.SaveChangesAsync();
        return creator;
    }
    
    public async Task UpdatePasswordAsync(Guid creatorId, string newPasswordHash)
    {
        TeamCreator? creator = await db.TeamCreators.FindAsync(creatorId);
        if (creator == null) return;

        creator.PasswordHash = newPasswordHash;
        await db.SaveChangesAsync();
    }

    public async Task ChangeEmailAsync(Guid creatorId, string newEmail)
    {
        TeamCreator? creator = await db.TeamCreators.FindAsync(creatorId);
        if (creator == null) return;

        creator.Email = newEmail;
        await db.SaveChangesAsync();
    }

    public async Task<bool> EmailAlreadyExistsAsync(string email) =>
        await db.TeamCreators.AnyAsync(c => c.Email == email);
}