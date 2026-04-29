using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.AuthDtos
{
    public record HabitEntryResponseDto(
        Guid EntryId,
        Guid HabitId,
        Guid MemberId,
        DateTime LoggedAt,
        EntryStatus Status,
        float? Value,
        string? Notes
    );
}