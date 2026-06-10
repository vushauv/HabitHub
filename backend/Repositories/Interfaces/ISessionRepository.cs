using backend.Models;
using backend.Enums;

namespace backend.Repositories.Interfaces;

public interface ISessionRepository
{
    Task<Session> CreateAsync(Session session);
    Task<List<Session>> GetActiveSessionsForUserAsync(Guid userId);
    Task<Session?> GetByIdAsync(string sessionId);
    Task<Session?> GetByIdWithUserAsync(string sessionId);
    Task InvalidateAsync(string sessionId);
    Task InvalidateAllExceptCurrentAsync(Guid userId, string currentSessionId);
    Task ExpirePastDueSessionsAsync();
    Task RefreshSpecificSession(string sessionId);
    Task ExpireSpecificSessionAsync(string sessionId);
}