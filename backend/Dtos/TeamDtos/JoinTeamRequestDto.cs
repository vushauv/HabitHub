using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.TeamDtos
{
    public record JoinTeamRequestDto(
        [Required, MinLength(8), MaxLength(8)]
        string Code
    );
}
