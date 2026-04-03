using backend.Models;

namespace backend.Repositories;

public interface ITeamMemberRepository
{
    Task<TeamMember?> GetMemberByEmailAsync(string email);
    Task<TeamMember> CreateMemberAsync(TeamMember member);
    Task<TeamMember?> GetMemberByIdAsync(Guid memberId);
    Task UpdateAsync(TeamMember member);
}