using backend.Auth;
using backend.Dtos.AuthDtos;
using backend.Exceptions;
using backend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            string? deviceInfo = Request.Headers.UserAgent.ToString();
            AuthResponseDto response = await authService.Register(request, ipAddress, deviceInfo);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            string? deviceInfo = Request.Headers.UserAgent.ToString();
            AuthResponseDto response =  await authService.Login(request, ipAddress, deviceInfo);
            return StatusCode(StatusCodes.Status200OK, response);
        }
        [HttpGet("sessions")]
        [Authorize]
        public async Task<IActionResult> ViewActiveSessions()
        {
            var currentUser = HttpContext.RequireCurrentUser();

            List<SessionDto> activeSessions = await authService.ViewActiveSessions(currentUser.UserId, currentUser.UserType, currentUser.SessionId);
            return StatusCode(StatusCodes.Status200OK, activeSessions);
        }
        
        [HttpDelete("sessions/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> InvalidateSpecificSession(string sessionId)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await authService.InvalidateSpecificSession(currentUser.UserId, currentUser.UserType, sessionId);
            return StatusCode(StatusCodes.Status204NoContent);
        }
        
        [HttpDelete("logout")]
        [Authorize]
        public async Task<IActionResult> LogoutFromCurrentSession()
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await authService.InvalidateSpecificSession(currentUser.UserId, currentUser.UserType, currentUser.SessionId);
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var currentUser = HttpContext.RequireCurrentUser();

            await authService.ChangePassword(currentUser.UserId, currentUser.UserType, currentUser.SessionId, request);
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpPost("change-email")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequestDto request)
        {
            var currentUser = HttpContext.RequireCurrentUser(); 

            await authService.ChangeEmail(currentUser.UserId, currentUser.UserType, currentUser.SessionId, request);
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var currentUser = HttpContext.RequireCurrentUser();

            UserDto userInfo = await authService.GetMe(currentUser.UserId, currentUser.UserType);
            return StatusCode(StatusCodes.Status200OK, userInfo);
        }
    }
}
