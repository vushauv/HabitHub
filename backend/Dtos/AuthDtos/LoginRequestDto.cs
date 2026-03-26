using backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.AuthDtos
{
    public record LoginRequestDto(
        [Required] string Email,
        [Required] string Password,
        [Required] UserType UserType
    );
}
