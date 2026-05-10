using Microsoft.AspNetCore.Diagnostics;

namespace backend.Exceptions;

public class AppExceptionHandler(ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is AppException ex)
        {
            httpContext.Response.StatusCode = ex.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(new { error = ex.ErrorCode, message = ex.Message }, cancellationToken);
            await httpContext.Response.CompleteAsync();
            return true;
        }
        logger.LogError(exception, "Unhandled exception: {}", exception.Message);
        
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new { error = "internal-server-error", message = "Internal Server Error occured." }, cancellationToken);
        await httpContext.Response.CompleteAsync();
        return true;
    }
}