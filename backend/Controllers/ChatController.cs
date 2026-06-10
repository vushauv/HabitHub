using backend.Auth;
using backend.Dtos.ChatDtos;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("teams/{teamId}/chat/messages")]
    [Authorize]
    public class ChatController(IChatService chatService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMessages(Guid teamId, [FromQuery] int offset = 0, [FromQuery] int count = 10)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            List<MessageDto> response = await chatService.GetMessages(currentUser.User, teamId, offset, count);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(Guid teamId, [FromBody] SendMessageRequestDto request)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            MessageDto response = await chatService.SendMessage(currentUser.User, teamId, request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid teamId, Guid messageId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await chatService.DeleteMessage(currentUser.User, teamId, messageId);
            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
