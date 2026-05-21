namespace backend.Exceptions
{
    public class NotFoundException: AppException
    {
        public NotFoundException(string errorCode = "not-found", string message = "Resource not found.")
            : base(
                  StatusCodes.Status404NotFound,
                  errorCode,
                  message
                  )
        { }
    }
}
