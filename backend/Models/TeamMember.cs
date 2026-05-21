namespace backend.Models;

public class TeamMember
{
    public Guid MemberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public List<Membership> Memberships {get;set;} = new List<Membership>();
    public List<HabitEntry> HabitEntries { get; set; } = new List<HabitEntry>();
    public List<Reminder> Reminders { get; set; } = new List<Reminder>();
}
