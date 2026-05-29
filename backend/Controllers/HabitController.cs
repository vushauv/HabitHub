using backend.Auth;
using backend.Dtos.HabitDtos;
using backend.Dtos.HabitEntryDtos;
using backend.Dtos.ReminderDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Authorize]
    public class HabitController(IHabitService habitService) :ControllerBase
    {
        [HttpPost("teams/{teamId}/habits")]
        public async Task<IActionResult> CreateHabit(Guid teamId, [FromBody] CreateHabitRequestDto request) 
        {
            var currentUser = HttpContext.RequireCurrentUser();

            CreateHabitResponseDto response = await habitService.CreateHabit(currentUser.UserId, teamId, request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpGet("teams/{teamId}/habits")]
        public async Task<IActionResult> GetTeamHabits(Guid teamId, [FromQuery] HabitState state)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            List<HabitSummaryDto> response = await habitService.GetTeamHabits(currentUser.UserId, currentUser.UserType, teamId, state);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpPatch("habits/{habitId}")]
        public async Task<IActionResult> EditHabit(Guid habitId, [FromBody] EditHabitRequestDto request)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            HabitSummaryDto response = await habitService.EditHabit(currentUser.UserId, habitId, request);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpPost("habits/{habitId}/archive")]
        public async Task<IActionResult> ArchiveHabit(Guid habitId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await habitService.ArchiveHabit(currentUser.UserId, habitId);
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpDelete("habits/{habitId}")]
        public async Task<IActionResult> DeleteHabit(Guid habitId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await habitService.DeleteHabit(currentUser.UserId, habitId);
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpGet("habits/{habitId}")] 
        public async Task<IActionResult> GetHabit(Guid habitId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            HabitSummaryDto response = await habitService.GetHabit(currentUser.UserId, currentUser.UserType, habitId);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpPost("habits/{habitId}/entries")]
        public async Task<IActionResult> LogProgress(Guid habitId, [FromBody] LogProgressRequestDto request)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            HabitEntryResponseDto response = await habitService.LogProgress(currentUser.UserId, currentUser.UserType, habitId, request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpDelete("habits/{habitId}/entries/{entryId}")]
        public async Task<IActionResult> UndoLog(Guid habitId, Guid entryId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await habitService.UndoLog(currentUser.UserId, currentUser.UserType, habitId, entryId);
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpGet("habits/{habitId}/entries")]
        public async Task<IActionResult> ViewProgress(Guid habitId, [FromQuery] Guid? memberId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            List<HabitEntryResponseDto> response = await habitService.ViewProgress(currentUser.UserId, currentUser.UserType, habitId, memberId);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpGet("habits/{habitId}/entries/today")]
        public async Task<IActionResult> GetMyTodayEntryStatus(Guid habitId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            TodayHabitEntryStatusDto response = await habitService.GetMyTodayEntryStatus(currentUser.UserId, currentUser.UserType, habitId);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpGet("habits/{habitId}/leaderboard")]
        public async Task<IActionResult> ViewLeaderboard(Guid habitId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            List<LeaderboardResponseDto> response = await habitService.ViewLeaderboard(currentUser.UserId, currentUser.UserType, habitId);
            return StatusCode(StatusCodes.Status200OK, response);
        }
    }
}
