using backend.Enums;
using backend.Validation;
using System.ComponentModel.DataAnnotations;
namespace backend.Dtos.AuthDtos
{
    public record RegisterRequestDto(
        [Required, StringLength(256, MinimumLength = 2)] 
        string Name,
        [Required, ValidEmail, StringLength(256)]
        string Email,
        [Required, StringLength(128, MinimumLength = 8)] 
        string Password,
        [Required, ValidTimezone] 
        string Timezone,
        [Required] 
        UserType? UserType
    );
}
