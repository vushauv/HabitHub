using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.TeamDtos
{
    public record CreateTeamRequestDto(
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        string Name
    );
}
