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
    }
}
