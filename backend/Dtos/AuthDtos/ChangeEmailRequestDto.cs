using backend.Validation;
using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.AuthDtos
{
    public record ChangeEmailRequestDto(
        [Required, ValidEmail, StringLength(256)]
        string NewEmail,
        [Required, StringLength(128, MinimumLength = 8)]
        string Password
    );
}