namespace backend.Models;

public class HabitTeam
{
    public Guid TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TeamCreator Creator { get; set; } = null!;
    public Guid CreatorId { get; set; }
    public List<Membership> Memberships { get; set; } = new List<Membership>();
    public List<InviteCode> InviteCodes { get;set; } = new List<InviteCode>();
    public List<Habit> Habits { get; set; } = new List<Habit>();
}