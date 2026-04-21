using backend.Models;
namespace backend.Repositories;

public interface IHabitTeamRepository
{
    Task<HabitTeam> CreateHabitTeamAsync(HabitTeam team);
    Task<HabitTeam?> GetHabitTeamByIdAsync(Guid teamId);
    Task<bool> CheckOwnershipOfTeamAsync(Guid teamId, Guid creatorId);
    Task<List<HabitTeam>> GetAllHabitTeamsByCreatorAsync(Guid creatorId);
    Task<bool> DeleteHabitTeamAsync(Guid teamId);
}