using backend.Enums;
using System.Globalization;

namespace backend.Models
{
    public class Notification
    {
        public Guid NotificationId { get; set; } 
        public Guid UserId { get; set; }
        public UserType UserType { get; set; }
        public string Content { get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread; 
        public NotificationType Type { get; set; } 
    }
}
