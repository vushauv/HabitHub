using backend.Dtos.ReminderDtos;
using backend.Enums;

namespace backend.Service.Interfaces
{
    public interface IReminderService
    {
        Task<HabitReminderResponseDto> SetHabitReminder(Guid userId, UserType userType, Guid habitId, SetReminderRequestDto request);
        Task ClearHabitReminder(Guid userId, UserType userType, Guid habitId);
        Task<MyReminderResponseDto> ChangeMyReminder(Guid userId, UserType userType, Guid habitId, ChangeMyReminderRequestDto request);
        Task<MyReminderResponseDto> GetMyReminder(Guid userId, UserType userType, Guid habitId);
    }
}