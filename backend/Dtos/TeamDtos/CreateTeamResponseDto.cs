using System.Security.Cryptography.X509Certificates;

namespace backend.Dtos.TeamDtos
{
    public record CreateTeamResponseDto(
        Guid TeamId,
        string Name
    );
}
