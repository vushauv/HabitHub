using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.HabitEntryDtos
{
    public record LogProgressRequestDto(
        float? Value,
        string? Notes,
        [Required]
        EntryStatus Status
    );
}