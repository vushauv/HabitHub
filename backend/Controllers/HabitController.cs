using backend.Auth;
using backend.Dtos.HabitDtos;
using backend.Dtos.HabitEntryDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Service;
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
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                CreateHabitResponseDto response = await habitService.CreateHabit(currentUser.UserId, teamId, request);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpGet("teams/{teamId}/habits")]
        public async Task<IActionResult> GetTeamHabits(Guid teamId, [FromQuery] HabitState state)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                List<HabitSummaryDto> response = await habitService.GetTeamHabits(currentUser.UserId, currentUser.UserType, teamId, state);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpPatch("habits/{habitId}")]
        public async Task<IActionResult> EditHabit(Guid habitId, [FromBody] EditHabitRequestDto request)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                HabitSummaryDto response = await habitService.EditHabit(currentUser.UserId, habitId, request);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpPost("habits/{habitId}/archive")]
        public async Task<IActionResult> ArchiveHabit(Guid habitId)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                await habitService.ArchiveHabit(currentUser.UserId, habitId);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch(AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpDelete("habits/{habitId}")]
        public async Task<IActionResult> DeleteHabit(Guid habitId)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                await habitService.DeleteHabit(currentUser.UserId, habitId);
                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpGet("habits/{habitId}")] 
        public async Task<IActionResult> GetHabit(Guid habitId)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                HabitSummaryDto response = await habitService.GetHabit(currentUser.UserId, currentUser.UserType, habitId);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpPost("habits/{habitId}/entries")]
        public async Task<IActionResult> LogProgress(Guid habitId, [FromBody] LogProgressRequestDto request)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                HabitEntryResponseDto response = await habitService.LogProgress(currentUser.UserId, currentUser.UserType, habitId, request);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpDelete("habits/{habitId}/entries/{entryId}")]
        public async Task<IActionResult> UndoLog(Guid habitId, Guid entryId)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                await habitService.UndoLog(currentUser.UserId, currentUser.UserType, habitId, entryId);
                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpGet("habits/{habitId}/entries")]
        public async Task<IActionResult> ViewProgress(Guid habitId, [FromQuery] Guid? memberId)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                List<HabitEntryResponseDto> response = await habitService.ViewProgress(currentUser.UserId, currentUser.UserType, habitId, memberId);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpGet("habits/{habitId}/entries/today")]
        public async Task<IActionResult> GetMyTodayEntryStatus(Guid habitId)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                TodayHabitEntryStatusDto response = await habitService.GetMyTodayEntryStatus(currentUser.UserId, currentUser.UserType, habitId);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpGet("habits/{habitId}/leaderboard")]
        public async Task<IActionResult> ViewLeaderboard(Guid habitId)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                List<LeaderboardResponseDto> response = await habitService.ViewLeaderboard(currentUser.UserId, currentUser.UserType, habitId);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }
    }
}
