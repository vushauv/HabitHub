using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.HabitDtos
{
    public record HabitSummaryDto(
        Guid HabitId,
        string Name,
        string? Goal,
        HabitState HabitState,
        HabitType HabitType,
        Unit? Unit,
        DateTime? ExpiryDate
    );
}