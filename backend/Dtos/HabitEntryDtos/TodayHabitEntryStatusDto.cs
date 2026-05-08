using backend.Enums;

namespace backend.Dtos.HabitEntryDtos
{
    public record TodayHabitEntryStatusDto(
        EntryStatus Status,
        HabitEntryResponseDto? Entry
    );
}
