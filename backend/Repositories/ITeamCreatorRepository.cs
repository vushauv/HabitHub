using backend.Models;

namespace backend.Repositories;

public interface ITeamCreatorRepository
{
    Task<TeamCreator?> GetCreatorByEmailAsync(string email);
    Task<TeamCreator> CreateCreatorAsync(TeamCreator creator);
    Task<TeamCreator?> GetCreatorByIdAsync(Guid creatorId);
    Task UpdatePasswordAsync(Guid creatorId, string newPasswordHash);
    Task ChangeEmailAsync(Guid creatorId, string newEmail);
    Task<bool> EmailAlreadyExistsAsync(string email);
}