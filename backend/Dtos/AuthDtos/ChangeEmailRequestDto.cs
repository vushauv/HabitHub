namespace backend.Dto.AuthDtos
{
    public record ChangeEmailRequestDto(
        string NewEmail,
        string Password
    );
}