namespace backend.Dtos.AuthDtos
{
    public record ChangeEmailRequestDto(
        string NewEmail,
        string Password
    );
}