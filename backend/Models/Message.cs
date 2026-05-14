using backend.Enums;

namespace backend.Models
{
    public class Message
    {
        public Guid MessageId { get; set; }
        public Guid ChatId { get; set; }
        public TeamChat Chat { get; set; } = null!;
        public Guid UserId { get; set; }
        public UserType UserType { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SendDate { get; set; } = DateTime.UtcNow;
    }
}
