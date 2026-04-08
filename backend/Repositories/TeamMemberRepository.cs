using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class TeamMemberRepository(AppDbContext db) : ITeamMemberRepository
{
    public async Task<TeamMember?> GetMemberByEmailAsync(string email) => 
        await db.TeamMembers.SingleOrDefaultAsync(m => m.Email == email);
    public async Task<TeamMember> CreateMemberAsync(TeamMember member)
    {
        db.TeamMembers.Add(member);
        await db.SaveChangesAsync();
        return member;
    }
}