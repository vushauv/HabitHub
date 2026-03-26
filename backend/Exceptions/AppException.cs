namespace backend.Exceptions
{
    public class AppException: Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public AppException(int statusCode, string errorCode, string? message = null) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
