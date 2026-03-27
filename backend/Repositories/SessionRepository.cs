using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.Enums;

namespace backend.Repositories;

public class SessionRepository(AppDbContext db) : ISessionRepository
{
    public async Task<Session> CreateAsync(Session session)
    {
        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }
    public async Task<Session?> GetActiveSessionForUserAsync(Guid userId, UserType userType) => 
        await db.Sessions.FirstOrDefaultAsync(
            s => s.UserType == userType && 
            s.UserId == userId && 
            s.SessionState == SessionState.Active && 
            s.ExpiresAt > DateTime.UtcNow);
    public async Task<Session?> GetByIdAsync(Guid sessionId) =>
        await db.Sessions.FindAsync(sessionId);
    public async Task UpdateAsync(Session session)
    {
        db.Sessions.Update(session);
        await db.SaveChangesAsync();
    }
}