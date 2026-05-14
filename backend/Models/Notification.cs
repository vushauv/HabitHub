using backend.Enums;
using System.Globalization;

namespace backend.Models
{
    public class Notification
    {
        public Guid NotificationId { get; set; } 
        public Guid UserId { get; set; }
        public UserType UserType { get; set; }
        public string Content { get; set; } = string.Empty; //TODO
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // TODO
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread; //TODO
    }
}
