using backend.Enums;

namespace backend.Dtos.AuthDtos
{
    public record UserDto(
        Guid Id,
        string Name,
        string Email,
        UserType UserType,
        string? Timezone
    );
}
