namespace backend.Auth;
using backend.Logging;
using backend.Repositories;
using backend.Enums;
using System.Linq;

public class SessionAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionAuthenticationMiddleware> _logger;

    public SessionAuthenticationMiddleware(RequestDelegate next, ILogger<SessionAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISessionRepository sessions)
    {
        string? sessionId = context.Request.Headers["X-Session-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            await _next(context);
            return;
        }
        var session = await sessions.GetByIdAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Auth rejected: session {SessionFingerprint} not found", LogRedaction.Fingerprint(sessionId));
            await _next(context);
            return;
        }
        if (session.SessionState != SessionState.Active)
        {
            _logger.LogWarning("Auth rejected: session {SessionFingerprint} for user {UserId} is {SessionState}",
                LogRedaction.Fingerprint(sessionId), session.UserId, session.SessionState);
            await _next(context);
            return;
        }
        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Auth rejected: session {SessionFingerprint} expired for user {UserId}",
                LogRedaction.Fingerprint(sessionId), session.UserId);
            await sessions.ExpireSpecificSessionAsync(session.SessionId);
            await _next(context);
            return;
        }
        await sessions.RefreshSpecificSession(session.SessionId);
        CurrentUserContext currentUser = new(session.UserId, session.UserType, session.SessionId);
        context.Items["CurrentUser"] = currentUser;
        _logger.LogDebug("Auth success: session {SessionFingerprint} for user {UserId} ({UserType})",
            LogRedaction.Fingerprint(sessionId), session.UserId, session.UserType);
        await _next(context);
    }
}