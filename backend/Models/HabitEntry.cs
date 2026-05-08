using backend.Enums;
namespace backend.Models;

public class HabitEntry
{
    public Guid HabitId { get; set; }
    public Habit Habit { get; set; } = null!;
    public Guid MemberId { get; set; }
    public TeamMember Member { get; set; } = null!;
    public Guid EntryId { get; set; }
    //public DateTime Date { get; set; }
    public DateTime LoggedAt { get; set; }
    public DateOnly LogDate { get; set; }
    public EntryStatus Status { get; set; }
    public float? Value { get; set; }
    public string? Notes { get; set; } = string.Empty;
}