using System.Security.Claims;
using System.Text.Encodings.Web;
using backend.Enums;
using backend.Logging;
using backend.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace backend.Auth;

public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ISessionRepository _sessions;
    
    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISessionRepository sessions
        ) : base(options, logger, encoder)
    {
        _sessions = sessions;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.LogInformation("Reached this part!");
        
        string? sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return AuthenticateResult.Fail("No session ID");
        }
        var session = await _sessions.GetByIdAsync(sessionId);
        if (session == null)
        {
            Logger.LogWarning("Auth rejected: session {SessionFingerprint} not found", LogRedaction.Fingerprint(sessionId));
            return AuthenticateResult.Fail("Session not found");
        }
        if (session.SessionState != SessionState.Active)
        {
            Logger.LogWarning("Auth rejected: session {SessionFingerprint} for user {UserId} is {SessionState}",
                LogRedaction.Fingerprint(sessionId), session.UserId, session.SessionState);
            return AuthenticateResult.Fail("Session not active");
        }
        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            Logger.LogWarning("Auth rejected: session {SessionFingerprint} expired for user {UserId}",
                LogRedaction.Fingerprint(sessionId), session.UserId);
            await _sessions.ExpireSpecificSessionAsync(session.SessionId);
            return AuthenticateResult.Fail("Expired Session");
        }
        await _sessions.RefreshSpecificSession(session.SessionId);
        CurrentUserContext currentUser = new(session.UserId, session.UserType, session.SessionId);
        Context.Items["CurrentUser"] = currentUser;
        Logger.LogDebug("Auth success: session {SessionFingerprint} for user {UserId} ({UserType})",
            LogRedaction.Fingerprint(sessionId), session.UserId, session.UserType);

        var claimsIdentity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new Claim(ClaimTypes.Role, ((int)session.UserType).ToString())
        ], nameof(SessionAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }
    
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.WriteAsJsonAsync(new { error = "auth-required", message = "Authentication is required." });
        return Task.CompletedTask;
    }
}