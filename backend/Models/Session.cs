using backend.Enums;

namespace backend.Models;

public class Session 
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public UserType UserType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public SessionState SessionState { get; set; }
    public string? IpAddress { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; } = string.Empty;
}