namespace backend.Auth;
using backend.Repositories;
using backend.Enums;
using System.Linq;
public class SessionAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    public SessionAuthenticationMiddleware(RequestDelegate next) => _next = next;
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
            await _next(context);
            return;
        }
        if (session.SessionState != SessionState.Active)
        {
            await _next(context);
            return;
        }
        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            await _next(context);
            return;
        }
        CurrentUserContext currentUser = new(session.UserId, session.UserType, session.SessionId);
        context.Items["CurrentUser"] = currentUser;
        await _next(context);
    }
}