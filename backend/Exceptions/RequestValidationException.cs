using Microsoft.AspNetCore.Http;

namespace backend.Exceptions
{
    public class RequestValidationException: AppException
    {
        public RequestValidationException(string message)
            : base(
                  StatusCodes.Status400BadRequest,
                  "validation-error",
                  message
                  ) { }
    }
}
