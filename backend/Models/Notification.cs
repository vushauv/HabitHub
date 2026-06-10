using backend.Enums;
using System.Globalization;

namespace backend.Models
{
    public class Notification
    {
        public Guid NotificationId { get; set; } 
        public Guid UserId { get; set; }
        public User? User { get; set; } = null;
        public string Content { get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread; 
        public NotificationType Type { get; set; } 

        public Guid? ReminderId { get; set; }
        public Reminder? Reminder { get; set; }
    }
}
