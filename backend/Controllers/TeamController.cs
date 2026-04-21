using Microsoft.AspNetCore.Mvc;
using backend.Auth;
using backend.Dtos.TeamDtos;
using backend.Exceptions;
using backend.Service;

namespace backend.Controllers
{
    [ApiController]
    [Route("teams")]
    public class TeamController(ITeamService teamService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequestDto request)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if(currentUser == null)
                    throw new AuthRequiredException();
                
                CreateTeamResponseDto response = await teamService.CreateTeam(currentUser.UserId, request);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch(AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpPost("{teamId}/invite-codes")]
        public async Task<IActionResult> GenerateInviteCode(Guid teamId)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

                GenerateInviteCodeResponseDto response = await teamService.GenerateInviteCode(currentUser.UserId, teamId);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpDelete("{teamId}/invite-codes/{codeId}")]
        public async Task<IActionResult> InvalidateInviteCode(Guid teamId, Guid codeId)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

                await teamService.InvalidateInviteCode(currentUser.UserId, teamId, codeId);
                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinTeam([FromBody] JoinTeamRequestDto request)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

                JoinTeamResponseDto response = await teamService.JoinTeam(currentUser.UserId,request);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpPost("{teamId}/members/{memberId}/kick")]
        public async Task<IActionResult> KickUser(Guid teamId, Guid memberId)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

                await teamService.KickUser(currentUser.UserId, teamId, memberId);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpPost("{teamId}/leave")]
        public async Task<IActionResult> LeaveTeam(Guid teamId)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

                await teamService.LeaveTeam(currentUser.UserId, teamId);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(Guid teamId)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if (currentUser == null)
                    throw new AuthRequiredException();

                await teamService.DeleteTeam(currentUser.UserId, teamId);
                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }
    }
}

