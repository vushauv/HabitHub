using backend.Data;
using backend.Enums;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class InviteCodeRepository(AppDbContext db) : IInviteCodeRepository
{
    public async Task<InviteCode> CreateInviteCodeAsync(InviteCode inviteCode)
    {
        db.Add(inviteCode);
        await db.SaveChangesAsync();
        return inviteCode;
    }

    public async Task<List<InviteCode>> GetActiveInviteCodesByTeamIdAsync(Guid teamId) =>
        await db.InviteCodes.Where(i => i.TeamId == teamId && i.Status == CodeStatus.Active && i.ExpiryDate > DateTime.UtcNow).ToListAsync();

    public async Task<InviteCode?> GetInviteCodeByCodeAsync(string code) =>
        await db.InviteCodes.SingleOrDefaultAsync(i => i.Code == code);

    public async Task<InviteCode?> GetInviteCodeByIdAsync(Guid codeId) =>
        await db.InviteCodes.SingleOrDefaultAsync(i => i.CodeId == codeId);

    public async Task<List<InviteCode>> GetInviteCodesByTeamIdAsync(Guid teamId) =>
        await db.InviteCodes.Where(i => i.TeamId == teamId).ToListAsync();

    public async Task<bool> IsInviteCodeActiveAsync(string code)
    {
        InviteCode? inviteCode = await GetInviteCodeByCodeAsync(code);
        if(inviteCode == null)
            return false;
        if(inviteCode.Status == CodeStatus.Active && inviteCode.ExpiryDate <= DateTime.UtcNow)
        {
            inviteCode.Status = CodeStatus.Expired;
            await db.SaveChangesAsync();
        }
        return inviteCode.Status == CodeStatus.Active;
    }

    public async Task UpdateInviteCodeStatusAsync(Guid codeId, CodeStatus status)
    {
        InviteCode? code = await GetInviteCodeByIdAsync(codeId);
        if(code == null)
            return;
        
        code.Status = status;
        await db.SaveChangesAsync();
    }

    public async Task ExpirePastDueInviteCodesAsync()
    {
        List<InviteCode> inviteCodes = await db.InviteCodes.Where(i => i.Status == CodeStatus.Active && i.ExpiryDate <= DateTime.UtcNow).ToListAsync();
        foreach(var code in inviteCodes)
            code.Status = CodeStatus.Expired;

        await db.SaveChangesAsync();
    }
}