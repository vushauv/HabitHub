namespace backend.Dtos.ReminderDtos
{
    public record HabitReminderResponseDto(
        Guid HabitId,
        TimeOnly? ReminderTime
    );
}