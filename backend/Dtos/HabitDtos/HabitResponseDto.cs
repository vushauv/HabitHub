using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.HabitDtos
{
    public record HabitResponseDto(
        Guid HabitId,
        Guid TeamId,
        string Name,
        string? Goal,
        Guid CreatorId,
        HabitState HabitState,
        HabitType HabitType,
        Unit? Unit,
        DateTime? ExpiryDate
    );
}