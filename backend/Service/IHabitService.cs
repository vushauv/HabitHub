using backend.Dtos.HabitDtos;
using backend.Models;
using backend.Enums;

namespace backend.Service
{
    public interface IHabitService
    {
        public Task<CreateHabitResponseDto> CreateHabit(Guid userId, Guid teamId, CreateHabitRequestDto request);
        public Task<HabitSummaryDto> EditHabit(Guid userId, Guid habitId, EditHabitRequestDto request);
        public Task ArchiveHabit(Guid userId, Guid habitId);
        public Task DeleteHabit(Guid userId, Guid habitId);
        public Task<HabitSummaryDto> GetHabit(Guid userId, UserType userType, Guid habitId);

    }
}
