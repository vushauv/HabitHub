using backend.Exceptions;

namespace backend.Auth;

public static class HttpContextAuthExtensions
{
    public static CurrentUserContext? GetCurrentUser(this HttpContext httpContext)
    {
        return httpContext.Items["CurrentUser"] as CurrentUserContext;
    }
    public static CurrentUserContext RequireCurrentUser(this HttpContext httpContext)
    {
        var currentUser = httpContext.GetCurrentUser();
        if (currentUser == null)
            throw new AuthRequiredException();
        return currentUser;
    }
}