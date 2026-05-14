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
            var currentUser = HttpContext.RequireCurrentUser();

            CreateTeamResponseDto response = await teamService.CreateTeam(currentUser.UserId, request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPost("{teamId}/invite-codes")]
        public async Task<IActionResult> GenerateInviteCode(Guid teamId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            InviteCodeDto response = await teamService.GenerateInviteCode(currentUser.UserId, teamId);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpDelete("{teamId}/invite-codes/{codeId}")]
        public async Task<IActionResult> InvalidateInviteCode(Guid teamId, Guid codeId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await teamService.InvalidateInviteCode(currentUser.UserId, teamId, codeId);
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinTeam([FromBody] JoinTeamRequestDto request)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            JoinTeamResponseDto response = await teamService.JoinTeam(currentUser.UserId,request);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpPost("{teamId}/members/{memberId}/kick")]
        public async Task<IActionResult> KickUser(Guid teamId, Guid memberId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await teamService.KickUser(currentUser.UserId, teamId, memberId);
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpPost("{teamId}/leave")]
        public async Task<IActionResult> LeaveTeam(Guid teamId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await teamService.LeaveTeam(currentUser.UserId, teamId);
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(Guid teamId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await teamService.DeleteTeam(currentUser.UserId, teamId);
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            var currentUser = HttpContext.RequireCurrentUser();

            List<TeamSummaryDto> response = await teamService.GetTeams(currentUser.UserId, currentUser.UserType);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpGet("{teamId}")]
        public async Task<IActionResult> GetTeam(Guid teamId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            TeamSummaryDto response = await teamService.GetTeam(currentUser.UserId, currentUser.UserType, teamId);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpGet("{teamId}/members")]
        public async Task<IActionResult> GetTeamMembers(Guid teamId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            List<TeamMemberDto> response = await teamService.GetTeamMembers(currentUser.UserId, currentUser.UserType, teamId);
            return StatusCode(StatusCodes.Status200OK, response);
        }

        [HttpGet("{teamId}/invite-codes")]
        public async Task<IActionResult> GetInviteCodes(Guid teamId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            List<InviteCodeDto> response = await teamService.GetActiveInviteCodes(currentUser.UserId, teamId);
            return StatusCode(StatusCodes.Status200OK, response);
        }
    }
}

