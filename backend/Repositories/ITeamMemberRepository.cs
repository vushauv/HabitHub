using backend.Models;

namespace backend.Repositories;

public interface ITeamMemberRepository
{
    Task<TeamMember?> GetMemberByEmailAsync(string email);
    Task<TeamMember> CreateMemberAsync(TeamMember member);
    Task<TeamMember?> GetMemberByIdAsync(Guid memberId);
    Task<List<TeamMember>> GetMembersByIdsAsync(List<Guid> memberIds);

    Task UpdatePasswordAsync(Guid memberId, string newPasswordHash);
    Task ChangeEmailAsync(Guid memberId, string newEmail);
    Task<bool> EmailAlreadyExistsAsync(string email);
}