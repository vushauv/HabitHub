using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.HabitEntryDtos
{
    public record HabitEntryResponseDto(
        Guid EntryId,
        Guid HabitId,
        Guid MemberId,
        DateTime LoggedAt,
        DateOnly LogDate,
        EntryStatus Status,
        float? Value,
        string? Notes
    );
}