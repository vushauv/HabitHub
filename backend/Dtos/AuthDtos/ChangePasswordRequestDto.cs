using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.AuthDtos
{
    public record ChangePasswordRequestDto(
        [Required, StringLength(128, MinimumLength = 8)]
        string CurrentPassword,
        [Required, StringLength(128, MinimumLength = 8)]
        string NewPassword
    );
}