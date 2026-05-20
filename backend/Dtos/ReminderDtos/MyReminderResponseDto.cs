namespace backend.Dtos.ReminderDtos
{
    public record MyReminderResponseDto(
        Guid HabitId,
        Guid MemberId,
        bool Enabled,
        TimeOnly? ReminderTime
    );
}