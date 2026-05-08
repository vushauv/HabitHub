using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class TeamCreatorRepository(AppDbContext db, ILogger<TeamCreatorRepository> logger) : ITeamCreatorRepository
{
    public async Task<TeamCreator?> GetCreatorByEmailAsync(string email) =>
        await db.TeamCreators.SingleOrDefaultAsync(c => c.Email == email);
    public async Task<TeamCreator?> GetCreatorByIdAsync(Guid creatorId) =>
        await db.TeamCreators.FindAsync(creatorId);
    public async Task<TeamCreator> CreateCreatorAsync(TeamCreator creator)
    {
        db.TeamCreators.Add(creator);
        await db.SaveChangesAsync();
        logger.LogInformation("Created team creator {CreatorId}", creator.CreatorId);
        return creator;
    }

    public async Task UpdatePasswordAsync(Guid creatorId, string newPasswordHash)
    {
        TeamCreator? creator = await db.TeamCreators.FindAsync(creatorId);
        if (creator == null)
        {
            logger.LogWarning("Update password skipped, creator {CreatorId} not found", creatorId);
            return;
        }

        creator.PasswordHash = newPasswordHash;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated password for creator {CreatorId}", creatorId);
    }

    public async Task ChangeEmailAsync(Guid creatorId, string newEmail)
    {
        TeamCreator? creator = await db.TeamCreators.FindAsync(creatorId);
        if (creator == null)
        {
            logger.LogWarning("Change email skipped, creator {CreatorId} not found", creatorId);
            return;
        }

        creator.Email = newEmail;
        await db.SaveChangesAsync();
        logger.LogInformation("Changed email for creator {CreatorId}", creatorId);
    }

    public async Task<bool> EmailAlreadyExistsAsync(string email) =>
        await db.TeamCreators.AnyAsync(c => c.Email == email);
}