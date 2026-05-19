using backend.Enums;
using backend.Models;

namespace backend.Repositories.Interfaces;

public interface IInviteCodeRepository
{
    public Task<InviteCode> CreateInviteCodeAsync(InviteCode inviteCode);
    public Task<InviteCode?> GetInviteCodeByCodeAsync(string code);
    public Task<InviteCode?> GetInviteCodeByIdAsync(Guid codeId);
    public Task<List<InviteCode>> GetInviteCodesByTeamIdAsync(Guid teamId);
    public Task<List<InviteCode>> GetActiveInviteCodesByTeamIdAsync(Guid teamId);
    public Task UpdateInviteCodeStatusAsync(Guid codeId, CodeStatus status);
    public Task InvalidateActiveInviteCodesByTeamIdAsync(Guid teamId);
    public Task<bool> IsInviteCodeActiveAsync(string code);
    public Task ExpirePastDueInviteCodesAsync();
}