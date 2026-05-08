using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using backend.Enums;

namespace backend.Dtos.HabitDtos
{
    public record EditHabitRequestDto(
        [StringLength(256)]
        string? Name,
        [StringLength(512)]
        string? Goal,
        DateTime? ExpiryDate, 
        bool ClearGoal = false,
        bool ClearExpiryDate = false
    );
}