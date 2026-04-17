namespace backend.Models;

public class HabitTeam
{
    public Guid TeamId {get; set;}
    public string Name {get; set;} = string.Empty;
    public TeamCreator Creator {get; set;} = null!;
    public Guid CreatorId {get; set;}
}