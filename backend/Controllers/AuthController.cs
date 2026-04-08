using backend.Auth;
using backend.Dtos.AuthDtos;
using backend.Exceptions;
using backend.Service;
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
            try
            {
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                string? deviceInfo = Request.Headers.UserAgent.ToString();
                AuthResponseDto response = await authService.Register(request, ipAddress, deviceInfo);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch(AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message= ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                string? deviceInfo = Request.Headers.UserAgent.ToString();
                AuthResponseDto response =  await authService.Login(request, ipAddress, deviceInfo);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch(AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message= ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }
        [HttpGet("sessions")]
        public async Task<IActionResult> ViewActiveSessions([FromHeader(Name = "X-Session-Id")] string? sessionIdHeader)
        {
            try
            {
                CurrentUserContext? currentUser = HttpContext.GetCurrentUser();
                if(currentUser == null) 
                    return StatusCode(StatusCodes.Status401Unauthorized, new {error = "auth-required", message = "Authorization required."});
                
                List<SessionDto> activeSessions = await authService.ViewActiveSessions(currentUser.UserId, currentUser.UserType, currentUser.SessionId);
                return StatusCode(StatusCodes.Status200OK, activeSessions);
            }
            catch(AppException ex)
            {
                return StatusCode(ex.StatusCode, new { error = ex.ErrorCode, message= ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "internal-server-error", message = "Internal Server Error occured." });
            }
        }
    }
}
