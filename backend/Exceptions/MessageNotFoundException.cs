using Microsoft.AspNetCore.Http;

namespace backend.Exceptions
{
    public class MessageNotFoundException: AppException
    {
        public MessageNotFoundException()
            : base(
                  StatusCodes.Status404NotFound,
                  "message-not-found",
                  "Message not found."
                  ) { }
    }
}
