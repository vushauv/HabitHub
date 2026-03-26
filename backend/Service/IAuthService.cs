using backend.Dtos.AuthDtos;

namespace backend.Service
{
    public interface IAuthService
    {
        public Task<AuthResponseDto> Login(LoginRequestDto request, string? ipAddress, string? deviceInfo);
        public Task<AuthResponseDto> Register(RegisterRequestDto request, string? ipAddress, string? deviceInfo);
    }
}
