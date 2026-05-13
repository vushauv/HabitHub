using backend.Auth;
using backend.Dtos.HabitDtos;
using backend.Dtos.HabitEntryDtos;
using backend.Dtos.TeamDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Models;
using backend.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;

namespace backend.Controllers
{
    [ApiController]
    public class HabitController(IHabitService habitService) :ControllerBase
    {
        [HttpPost("teams/{teamId}/habits")]
        public async Task<IActionResult> CreateHabit(Guid teamId, [FromBody] CreateHabitRequestDto request, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader) 
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
        public async Task<IActionResult> GetTeamHabits(Guid teamId, [FromQuery] HabitState state, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
        public async Task<IActionResult> EditHabit(Guid habitId, [FromBody] EditHabitRequestDto request, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
        public async Task<IActionResult> ArchiveHabit(Guid habitId, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
        public async Task<IActionResult> DeleteHabit(Guid habitId, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
        public async Task<IActionResult> GetHabit(Guid habitId, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
        public async Task<IActionResult> LogProgress(Guid habitId, [FromBody] LogProgressRequestDto request, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
        public async Task<IActionResult> UndoLog(Guid habitId, Guid entryId, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
        public async Task<IActionResult> ViewProgress(Guid habitId, [FromQuery] Guid? memberId, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
        public async Task<IActionResult> GetMyTodayEntryStatus(Guid habitId, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
        public async Task<IActionResult> ViewLeaderboard(Guid habitId, [FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

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
