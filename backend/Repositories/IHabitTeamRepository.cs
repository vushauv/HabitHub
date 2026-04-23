using backend.Models;
namespace backend.Repositories;

public interface IHabitTeamRepository
{
    Task<HabitTeam> CreateHabitTeamAsync(HabitTeam team);
    Task<HabitTeam?> GetHabitTeamByIdAsync(Guid teamId);
    Task<bool> CheckOwnershipOfTeamAsync(Guid teamId, Guid creatorId);
    Task<List<HabitTeam>> GetAllHabitTeamsByCreatorAsync(Guid creatorId);
    Task<List<HabitTeam>> GetHabitTeamsByIdsAsync(List<Guid> teamIds);
    Task<bool> DeleteHabitTeamAsync(Guid teamId);
}