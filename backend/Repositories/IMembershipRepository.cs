using backend.Enums;
using backend.Models;

namespace backend.Repositories;

public interface IMembershipRepository
{
    public Task<Membership> CreateMembershipAsync(Membership membership);
    public Task<Membership?> GetMembershipByTeamIdAndMemberIdAsync(Guid teamId, Guid memberId);
    public Task<Membership?> GetMembershipByIdAsync(Guid membershipId);
    public Task<List<Membership>> GetMembershipsByTeamIdAsync(Guid teamId);
    public Task<List<Membership>> GetActiveMembershipsByTeamIdAsync(Guid teamId);
    public Task<List<Membership>> GetMembershipsByMemberIdAsync(Guid memberId);
    public Task UpdateMembershipStatusAsync(Guid teamId, Guid memberId, MembershipStatus status);
    public Task<bool> IsActiveMembershipAsync(Guid teamId, Guid memberId);
}