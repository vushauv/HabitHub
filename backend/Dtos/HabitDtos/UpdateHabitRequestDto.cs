using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.HabitDtos
{
    public record UpdateHabitRequestDto(
        [StringLength(256)]
        string? Name,
        [StringLength(512)]
        string? Goal,
        HabitType? HabitType,
        Unit? Unit,
        DateTime? ExpiryDate
    );
}