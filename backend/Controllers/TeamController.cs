using backend.Auth;
using backend.Dtos.TeamDtos;
using backend.Exceptions;
using backend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("teams")]
    [Authorize]
    public class TeamController(ITeamService teamService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequestDto request)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();
                
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
                var currentUser = HttpContext.RequireCurrentUser();

                InviteCodeDto response = await teamService.GenerateInviteCode(currentUser.UserId, teamId);
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
                var currentUser = HttpContext.RequireCurrentUser();

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
                var currentUser = HttpContext.RequireCurrentUser();

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
                var currentUser = HttpContext.RequireCurrentUser();

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
                var currentUser = HttpContext.RequireCurrentUser();

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
                var currentUser = HttpContext.RequireCurrentUser();

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

        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                List<TeamSummaryDto> response = await teamService.GetTeams(currentUser.UserId, currentUser.UserType);
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

        [HttpGet("{teamId}")]
        public async Task<IActionResult> GetTeam(Guid teamId)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                TeamSummaryDto response = await teamService.GetTeam(currentUser.UserId, currentUser.UserType, teamId);
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

        [HttpGet("{teamId}/members")]
        public async Task<IActionResult> GetTeamMembers(Guid teamId)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                List<TeamMemberDto> response = await teamService.GetTeamMembers(currentUser.UserId, currentUser.UserType, teamId);
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

        [HttpGet("{teamId}/invite-codes")]
        public async Task<IActionResult> GetInviteCodes(Guid teamId)
        {
            try
            {
                var currentUser = HttpContext.RequireCurrentUser();

                List<InviteCodeDto> response = await teamService.GetActiveInviteCodes(currentUser.UserId, teamId);
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

