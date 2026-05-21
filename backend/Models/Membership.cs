using backend.Enums;
namespace backend.Models;

public class Membership
{
    public Guid MembershipId {get; set;}
    public Guid MemberId {get; set;}
    public TeamMember Member { get; set; } = null!;
    public Guid TeamId {get;set;}
    public HabitTeam Team { get; set; } = null!;
    public MembershipStatus Status {get; set;}
}