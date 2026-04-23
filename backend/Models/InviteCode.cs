using backend.Enums;
namespace backend.Models;

public class InviteCode
{
    public Guid CodeId {get;set;}
    public string Code {get; set;} = string.Empty;
    public Guid TeamId {get; set;}
    public HabitTeam Team {get; set;} = null!;
    public DateTime ExpiryDate {get;set;}
    public CodeStatus Status {get; set;}
}