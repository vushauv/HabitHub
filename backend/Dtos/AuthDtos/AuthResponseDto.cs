namespace backend.Dtos.AuthDtos
{
    public record AuthResponseDto(
        string SessionId,
        UserDto User
    );
}
