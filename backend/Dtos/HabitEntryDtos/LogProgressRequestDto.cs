using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.Dtos.AuthDtos
{
    public record LogProgressRequestDto(
        float? Value,
        string? Notes,
        [Required]
        EntryStatus Status,
        [Required]
        DateTime LoggedAt
    );
}