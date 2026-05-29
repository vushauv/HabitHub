using backend.Models;
namespace backend.Repositories.Interfaces;

public interface IHabitTeamRepository
{
    Task<HabitTeam> CreateHabitTeamAsync(HabitTeam team);
    Task<HabitTeam?> GetHabitTeamByIdAsync(Guid teamId);
    Task<HabitTeam?> GetHabitTeamByHabitIdAsync(Guid habitId);
    Task<bool> CheckOwnershipOfTeamAsync(Guid teamId, Guid creatorId);
    Task<List<HabitTeam>> GetAllHabitTeamsByCreatorAsync(Guid creatorId);
    Task<List<HabitTeam>> GetHabitTeamsByIdsAsync(List<Guid> teamIds);
    Task<bool> DeleteHabitTeamAsync(Guid teamId);
}