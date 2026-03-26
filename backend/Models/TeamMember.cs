namespace backend.Models;

public class TeamMember
{
    public Guid MemberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
}
