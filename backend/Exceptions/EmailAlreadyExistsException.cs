using Microsoft.AspNetCore.Http;

namespace backend.Exceptions
{
    public class EmailAlreadyExistsException: AppException
    {
        public EmailAlreadyExistsException() 
            : base(
                  StatusCodes.Status409Conflict,
                  "email-already-exists",
                  "An account with this email already exists."
                  ) { }
    }
}
