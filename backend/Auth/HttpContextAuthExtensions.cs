namespace backend.Auth;

public static class HttpContextAuthExtensions
{
    public static CurrentUserContext? GetCurrentUser(this HttpContext httpContext)
    {
        return httpContext.Items["CurrentUser"] as CurrentUserContext;
    }
}