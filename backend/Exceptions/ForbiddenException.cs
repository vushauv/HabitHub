using Microsoft.AspNetCore.Connections.Features;

namespace backend.Exceptions
{
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string errorCode="forbidden", string message="This action is forbidden")
            : base(
                  StatusCodes.Status403Forbidden,
                  errorCode,
                  message
                  )
        { }
    }
}
