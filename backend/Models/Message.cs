namespace backend.Models
{
    public class Message
    {
        public Guid MessageId { get; set; }
        public Guid ChatId { get; set; }
        public TeamChat Chat { get; set; } = null!;
        public Guid UserId { get; set; }
        public User? User { get; set; } = null;
        public string Content { get; set; } = string.Empty;
        public DateTime SendDate { get; set; } = DateTime.UtcNow;
    }
}
