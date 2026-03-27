using Microsoft.AspNetCore.Http;
namespace backend.Exceptions
{
    public class InvalidCredentialsException: AppException
    {
        public InvalidCredentialsException(string message)
            : base(
                  StatusCodes.Status401Unauthorized,
                  "invalid-credentials",
                  //"The provided credentials are invalid."
                  message
                  ) { }
    }
}
