namespace backend.Models;

public class User
{
    public Guid UserId { get; set; }
    
    public List<Session> Sessions { get; set; } = new List<Session>();
}
