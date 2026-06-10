using backend.Enums;

namespace backend.Models;

public class TeamCreator : User
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<HabitTeam> Teams {get;set;} = new List<HabitTeam>();

    public override UserType GetUserType()
    {
        return UserType.Creator;
    }
}
