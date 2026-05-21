using backend.Dtos.AuthDtos;
using backend.Enums;

namespace backend.Service.Interfaces
{
    public interface IAuthService
    {
        public Task<AuthResponseDto> Login(LoginRequestDto request, string? ipAddress, string? deviceInfo);
        public Task<AuthResponseDto> Register(RegisterRequestDto request, string? ipAddress, string? deviceInfo);
        public Task<List<SessionDto>> ViewActiveSessions(Guid userId, UserType userType, string currentSessionId);
        public Task InvalidateSpecificSession(Guid userId, UserType userType, string sessionId);
        public Task ChangePassword(Guid userId, UserType userType, string currentSessionId, ChangePasswordRequestDto request);
        public Task ChangeEmail(Guid userId, UserType userType, string currentSessionId, ChangeEmailRequestDto request);
        public Task<UserDto> GetMe(Guid userId, UserType userType);
    }
}
