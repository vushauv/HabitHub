using backend.Data;
using backend.Enums;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class InviteCodeRepository(AppDbContext db, ILogger<InviteCodeRepository> logger) : IInviteCodeRepository
{
    public async Task<InviteCode> CreateInviteCodeAsync(InviteCode inviteCode)
    {
        db.Add(inviteCode);
        await db.SaveChangesAsync();
        logger.LogInformation("Created invite code {CodeId} for team {TeamId}", inviteCode.CodeId, inviteCode.TeamId);
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
            logger.LogInformation("Marked invite code {CodeId} as expired on access", inviteCode.CodeId);
        }
        return inviteCode.Status == CodeStatus.Active;
    }

    public async Task UpdateInviteCodeStatusAsync(Guid codeId, CodeStatus status)
    {
        InviteCode? code = await GetInviteCodeByIdAsync(codeId);
        if(code == null)
        {
            logger.LogWarning("Update invite code status skipped, code {CodeId} not found", codeId);
            return;
        }

        code.Status = status;
        await db.SaveChangesAsync();
        logger.LogInformation("Updated invite code {CodeId} status to {Status}", codeId, status);
    }

    public async Task InvalidateActiveInviteCodesByTeamIdAsync(Guid teamId)
    {
        List<InviteCode> activeCodes = await GetActiveInviteCodesByTeamIdAsync(teamId);
        foreach (InviteCode code in activeCodes)
            code.Status = CodeStatus.Invalid;

        await db.SaveChangesAsync();
        logger.LogInformation("Invalidated {Count} active invite codes for team {TeamId}", activeCodes.Count, teamId);
    }

    public async Task ExpirePastDueInviteCodesAsync()
    {
        List<InviteCode> inviteCodes = await db.InviteCodes.Where(i => i.Status == CodeStatus.Active && i.ExpiryDate <= DateTime.UtcNow).ToListAsync();
        foreach(var code in inviteCodes)
            code.Status = CodeStatus.Expired;

        await db.SaveChangesAsync();
        logger.LogInformation("Expired {Count} past-due invite codes", inviteCodes.Count);
    }
}