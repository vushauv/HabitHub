using Microsoft.AspNetCore.Http;
namespace backend.Exceptions
{
    public class InvalidCredentialsException: AppException
    {
        public InvalidCredentialsException()
            : base(
                  StatusCodes.Status401Unauthorized,
                  "invalid-credentials",
                  "The provided credentials are invalid."
                  ) { }
    }
}
