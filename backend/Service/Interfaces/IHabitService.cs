using backend.Dtos.HabitDtos;
using backend.Dtos.HabitEntryDtos;
using backend.Enums;
using backend.Models;

namespace backend.Service.Interfaces
{
    public interface IHabitService
    {
        public Task<CreateHabitResponseDto> CreateHabit(Guid userId, Guid teamId, CreateHabitRequestDto request);
        public Task<HabitSummaryDto> EditHabit(Guid userId, Guid habitId, EditHabitRequestDto request);
        public Task ArchiveHabit(Guid userId, Guid habitId);
        public Task DeleteHabit(Guid userId, Guid habitId);
        public Task<HabitSummaryDto> GetHabit(Guid userId, UserType userType, Guid habitId);
        public Task<HabitEntryResponseDto> LogProgress(Guid userId, UserType userType, Guid habitId, LogProgressRequestDto request);
        public Task UndoLog(Guid userId, UserType userType, Guid habitId, Guid entryId);
        public Task<TodayHabitEntryStatusDto> GetMyTodayEntryStatus(Guid userId, UserType userType, Guid habitId);
        public Task<List<HabitEntryResponseDto>> ViewProgress(Guid userId, UserType userType, Guid habitId, Guid? memberId);
        public Task<List<LeaderboardResponseDto>> ViewLeaderboard(Guid userId, UserType userType, Guid habitId);
        public Task<List<HabitSummaryDto>> GetTeamHabits(Guid userId, UserType userType, Guid teamId, HabitState state);
    }
}
