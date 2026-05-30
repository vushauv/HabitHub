using backend.Auth;
using backend.Dtos.ReminderDtos;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    public class ReminderController(IReminderService reminderService) :ControllerBase
    {
        [HttpPatch("habits/{habitId}/reminder")]
        public async Task<IActionResult> SetHabitReminder(Guid habitId, [FromBody] SetReminderRequestDto request)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            HabitReminderResponseDto response = await reminderService.SetHabitReminder(currentUser.UserId, currentUser.UserType, habitId, request);

            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpDelete("habits/{habitId}/reminder")]
        public async Task<IActionResult> ClearHabitReminder(Guid habitId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await reminderService.ClearHabitReminder(currentUser.UserId, currentUser.UserType, habitId);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPatch("habits/{habitId}/my-reminder")]
        public async Task<IActionResult> ChangeMyReminder(Guid habitId, [FromBody] ChangeMyReminderRequestDto request)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            MyReminderResponseDto response = await reminderService.ChangeMyReminder(currentUser.UserId, currentUser.UserType, habitId, request);

            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpGet("habits/{habitId}/my-reminder")]
        public async Task<IActionResult> GetMyReminder(Guid habitId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            MyReminderResponseDto response = await reminderService.GetMyReminder(currentUser.UserId, currentUser.UserType, habitId);

            return StatusCode(StatusCodes.Status200OK, response);
        }
    }
}
