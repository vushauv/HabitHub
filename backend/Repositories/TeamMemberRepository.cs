using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class TeamMemberRepository(AppDbContext db, ILogger<TeamMemberRepository> logger) : ITeamMemberRepository
{
    public async Task<TeamMember?> GetMemberByEmailAsync(string email) =>
        await db.TeamMembers.SingleOrDefaultAsync(m => m.Email == email);

    public async Task<TeamMember?> GetMemberByIdAsync(Guid memberId) =>
        await db.TeamMembers.FindAsync(memberId);
    public async Task<List<TeamMember>> GetMembersByIdsAsync(List<Guid> memberIds) =>
        await db.TeamMembers.Where(m => memberIds.Contains(m.MemberId)).ToListAsync();

    public async Task<TeamMember> CreateMemberAsync(TeamMember member)
    {
        db.TeamMembers.Add(member);
        await db.SaveChangesAsync();
        logger.LogInformation("Created team member {MemberId}", member.MemberId);
        return member;
    }
    public async Task UpdatePasswordAsync(Guid memberId, string newPasswordHash)
    {
        TeamMember? member = await db.TeamMembers.FindAsync(memberId);
        if (member == null)
        {
            logger.LogWarning("Update password skipped, member {MemberId} not found", memberId);
            return;
        }

        member.PasswordHash = newPasswordHash;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated password for member {MemberId}", memberId);
    }

    public async Task ChangeEmailAsync(Guid memberId, string newEmail)
    {
        TeamMember? member = await db.TeamMembers.FindAsync(memberId);
        if (member == null)
        {
            logger.LogWarning("Change email skipped, member {MemberId} not found", memberId);
            return;
        }

        member.Email = newEmail;
        await db.SaveChangesAsync();
        logger.LogInformation("Changed email for member {MemberId}", memberId);
    }

    public async Task<bool> EmailAlreadyExistsAsync(string email) =>
        await db.TeamMembers.AnyAsync(c => c.Email == email);

    public async Task<List<TeamMember>> GetMembersByHabitEntriesAsync(Guid habitId)
    {
        List<Guid> memberIds = await db.HabitEntries
            .Where(e => e.HabitId == habitId)
            .Select(e => e.MemberId)
            .Distinct()
            .ToListAsync();

        if (memberIds.Count == 0)
            return new List<TeamMember>();

        return await GetMembersByIdsAsync(memberIds);
    }
}