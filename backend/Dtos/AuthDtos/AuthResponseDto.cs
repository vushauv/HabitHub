namespace backend.Dtos.AuthDtos
{
    public record AuthResponseDto(
        Guid SessionId,
        UserDto User
    );
}
