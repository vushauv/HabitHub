using Microsoft.AspNetCore.Http;

namespace backend.Exceptions
{
    public class MessageNotOwnException: AppException
    {
        public MessageNotOwnException()
            : base(
                  StatusCodes.Status403Forbidden,
                  "message-not-own",
                  "You can only delete your own messages."
                  ) { }
    }
}
