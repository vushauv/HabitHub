namespace backend.Dtos.AuthDtos
{
    public record ChangePasswordRequestDto(
        string CurrentPassword,
        string NewPassword
    );
}