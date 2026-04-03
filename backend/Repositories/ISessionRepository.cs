using backend.Models;
using backend.Enums;

namespace backend.Repositories;

public interface ISessionRepository
{
    Task<Session> CreateAsync(Session session);
    Task<List<Session>> GetActiveSessionsForUserAsync(Guid userId, UserType userType);
    Task<Session?> GetByIdAsync(string sessionId);
    Task InvalidateAsync(string sessionId);
    Task InvalidateAllExceptCurrentAsync(Guid userId, UserType userType, string currentSessionId);
    Task ExpirePastDueSessionsAsync();
}