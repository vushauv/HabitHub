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
}