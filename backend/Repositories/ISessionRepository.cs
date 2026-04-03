using backend.Models;
using backend.Enums;

namespace backend.Repositories;

public interface ISessionRepository
{
    Task<Session> CreateAsync(Session session);
    Task<List<Session>> GetActiveSessionsForUserAsync(Guid userId, UserType userType);
    Task<Session?> GetByIdAsync(Guid sessionId);
    Task InvalidateAsync(Guid sessionId);
    Task InvalidateAllExceptCurrentAsync(Guid userId, UserType userType, Guid currentSessionId);
    Task ExpirePastDueSessionsAsync();
}