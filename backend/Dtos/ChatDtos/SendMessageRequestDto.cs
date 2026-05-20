using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.ChatDtos
{
    public record SendMessageRequestDto(
        [Required, MinLength(1), MaxLength(2000)]
        string Content
    );
}
