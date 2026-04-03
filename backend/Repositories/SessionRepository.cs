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
    public async Task<List<Session>> GetActiveSessionsForUserAsync(Guid userId, UserType userType) =>
        await db.Sessions.Where(
            s => s.UserType == userType &&
            s.UserId == userId &&
            s.SessionState == SessionState.Active &&
            s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.LastActiveAt)
            .ToListAsync();
    public async Task<Session?> GetByIdAsync(Guid sessionId) =>
        await db.Sessions.FindAsync(sessionId);
    public async Task UpdateAsync(Session session)
    {
        db.Sessions.Update(session);
        await db.SaveChangesAsync();
    }

    public async Task InvalidateAsync(Guid sessionId)
    {
        Session? session = await GetByIdAsync(sessionId);
        if (session == null) return;

        if(session.SessionState == SessionState.Active)
        {
            session.SessionState = SessionState.Invalidated;
            await db.SaveChangesAsync();
        }
    }

    public async Task InvalidateAllExceptCurrentAsync(Guid userId, UserType userType, Guid currentSessionId)
    {
         List<Session> sessions = await db.Sessions.Where(s => s.UserId == userId &&
                                                                s.UserType == userType &&
                                                                s.SessionState == SessionState.Active
                                                                && s.SessionId != currentSessionId)
                                                          .ToListAsync();

        foreach(Session session in sessions)
        {
            session.SessionState = SessionState.Invalidated;
        }

        await db.SaveChangesAsync();
    }

    public async Task ExpirePastDueSessionsAsync()
    {
        List<Session> sessions = await db.Sessions.Where(s => s.SessionState == SessionState.Active &&
                                                                s.ExpiresAt <= DateTime.UtcNow)
                                                         .ToListAsync();
        foreach(Session session in sessions)
        {
            session.SessionState = SessionState.Expired;
        }
        await db.SaveChangesAsync();
    }
}