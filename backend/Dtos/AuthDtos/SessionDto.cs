using backend.Enums;
namespace backend.Dtos.AuthDtos
{
    public record SessionDto(
        string SessionId,
        DateTime CreatedAt,
        DateTime LastActiveAt,
        DateTime ExpiresAt, 
        SessionState SessionState,
        bool IsCurrent,
        string? DeviceInfo,
        string? IpAddress
    );
}