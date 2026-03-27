using backend.Models;
using backend.Enums;

namespace backend.Repositories;

public interface ISessionRepository
{
    Task<Session> CreateAsync(Session session);
    Task<Session?> GetActiveSessionForUserAsync(Guid userId, UserType userType);
    Task<Session?> GetByIdAsync(Guid sessionId);
    Task UpdateAsync(Session session);
}