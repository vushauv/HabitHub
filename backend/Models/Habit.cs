using backend.Enums;
namespace backend.Models;

public class Habit
{
    public Guid HabitId { get; set; }
    public Guid TeamId { get; set; }
    public HabitTeam Team { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; } = string.Empty;
    public Guid CreatorId { get; set; }
    public TeamCreator Creator { get; set; } = null!;
    public HabitState HabitState { get; set; }
    public HabitType HabitType { get; set; }
    public Unit? Unit { get; set; }
    public DateTime? ExpiryDate { get; set; }
    //public DateTime ReminderTime { get; set; }
    public List<Reminder> Reminders { get; set; } = new List<Reminder>();
    public List<HabitEntry> Entries { get; set; } = new List<HabitEntry>();
}