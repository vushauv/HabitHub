using backend.Enums;
using System.Collections.Specialized;

namespace backend.Models;

public class Session 
{
    public string SessionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public UserType UserType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public SessionState SessionState { get; set; }
    public string? IpAddress { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; } = string.Empty;
}