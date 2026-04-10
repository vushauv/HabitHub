using backend.Enums;
using backend.Validation;
using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.AuthDtos
{
    public record LoginRequestDto(
        [Required, ValidEmail, StringLength(256)]
        string Email,
        [Required, StringLength(128, MinimumLength = 8)]
        string Password,
        [Required] 
        UserType UserType
    );
}
