namespace backend.Models;

public class User
{
    public Guid UserId { get; set; }
    
    public List<Notification> Notifications { get; set; } = new List<Notification>();
    public List<Session> Sessions { get; set; } = new List<Session>();
}
