using backend.Data;
using backend.Enums;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class MembershipRepository(AppDbContext db, ILogger<MembershipRepository> logger) : IMembershipRepository
{
    public async Task<Membership> CreateMembershipAsync(Membership membership)
    {
        db.Memberships.Add(membership);
        await db.SaveChangesAsync();
        logger.LogInformation("Created membership {MembershipId} for member {MemberId} in team {TeamId}", membership.MembershipId, membership.MemberId, membership.TeamId);
        return membership;
    }

    public async Task<List<Membership>> GetActiveMembershipsByTeamIdAsync(Guid teamId) =>
        await db.Memberships.Where(m => m.TeamId == teamId && m.Status == MembershipStatus.Active).ToListAsync();

    public async Task<List<Membership>> GetActiveMembershipsByMemberIdAsync(Guid memberId) =>
        await db.Memberships.Where(m => m.MemberId == memberId && m.Status == MembershipStatus.Active).ToListAsync();

    public async Task<Membership?> GetMembershipByIdAsync(Guid membershipId) =>
        await db.Memberships.SingleOrDefaultAsync(m => m.MembershipId == membershipId);

    public async Task<Membership?> GetMembershipByTeamIdAndMemberIdAsync(Guid teamId, Guid memberId) =>
        await db.Memberships.SingleOrDefaultAsync(m => m.TeamId == teamId && m.MemberId == memberId);

    public async Task<List<Membership>> GetMembershipsByMemberIdAsync(Guid memberId) =>
        await db.Memberships.Where(m => m.MemberId == memberId).ToListAsync();

    public async Task<List<Membership>> GetMembershipsByTeamIdAsync(Guid teamId) =>
        await db.Memberships.Where(m => m.TeamId == teamId).ToListAsync();

    public async Task<bool> IsActiveMembershipAsync(Guid teamId, Guid memberId)
    {
        Membership? membership = await GetMembershipByTeamIdAndMemberIdAsync(teamId, memberId);
        if(membership == null)
            return false;
        
        return membership.Status == MembershipStatus.Active;
    }

    public async Task UpdateMembershipStatusAsync(Guid teamId, Guid memberId, MembershipStatus status)
    {
        Membership? membership = await GetMembershipByTeamIdAndMemberIdAsync(teamId, memberId);
        if(membership == null)
        {
            logger.LogWarning("Update membership status skipped, membership for member {MemberId} in team {TeamId} not found", memberId, teamId);
            return;
        }

        membership.Status = status;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated membership {MembershipId} status to {Status}", membership.MembershipId, status);
    }
}