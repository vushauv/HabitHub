using backend.Enums;
namespace backend.Dtos.AuthDtos
{
    public record SessionDto(
        string SessionId,
        UserType UserType,
        DateTime CreatedAt,
        DateTime LastActiveAt,
        DateTime ExpiresAt, 
        SessionState SessionState,
        bool IsCurrent
    );
}