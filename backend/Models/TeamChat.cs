namespace backend.Models
{
    public class TeamChat
    {
        public Guid ChatdId { get; set; }
        public Guid TeamId { get; set; }
        public HabitTeam Team { get; set; } = null!;
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}
