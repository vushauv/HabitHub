using backend.Enums;

namespace backend.Models;

public class User
{
    public Guid UserId { get; set; }
    
    public List<Message> Messages { get; set; } = new List<Message>();
    public List<Notification> Notifications { get; set; } = new List<Notification>();
    public List<Session> Sessions { get; set; } = new List<Session>();

    public virtual UserType GetUserType()
    {
        throw new InvalidOperationException();
    }
}
