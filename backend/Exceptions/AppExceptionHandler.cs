using Microsoft.AspNetCore.Diagnostics;

namespace backend.Exceptions;

public class AppExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is AppException ex)
        {
            httpContext.Response.StatusCode = ex.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(new { error = ex.ErrorCode, message = ex.Message }, cancellationToken);
            return true;
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new { error = "internal-server-error", message = "Internal Server Error occured." }, cancellationToken);
        return true;
    }
}