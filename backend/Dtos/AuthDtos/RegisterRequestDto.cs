using backend.Enums;
using System.ComponentModel.DataAnnotations;
namespace backend.Dtos.AuthDtos
{
    public record RegisterRequestDto(
        [Required] string Name,
        [Required] string Email,
        [Required] string Password,
        [Required] string Timezone,
        [Required] UserType UserType
    );
}
