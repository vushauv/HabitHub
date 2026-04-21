namespace backend.Exceptions
{
    public class AuthRequiredException:AppException
    {
        public AuthRequiredException()
            : base(
                  StatusCodes.Status401Unauthorized,
                  "auth-required",
                  "Authentication is required."
                  )
        { }
    }
}
