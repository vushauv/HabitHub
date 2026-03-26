using backend.Dtos.AuthDtos;
using backend.Repositories;
using Microsoft.AspNetCore.Identity;
using backend.Models;
using backend.Exceptions;

namespace backend.Service
{
    public class AuthService(ITeamCreatorRepository creators, ITeamMemberRepository members, ISessionRepository sessions) : IAuthService
    {
        private PasswordHasher<object> hasher = new PasswordHasher<object>();
        public async Task<AuthResponseDto> Register(RegisterRequestDto request, string? ipAddress, string? deviceInfo)
        {
            return request.UserType switch
            {
                UserType.Creator => await RegisterCreatorAsync(request, ipAddress, deviceInfo),
                UserType.Member => await RegisterMemberAsync(request, ipAddress, deviceInfo),
                _ => throw new RequestValidationException("Invalid user type.")
            };

        }

        public async Task<AuthResponseDto> Login(LoginRequestDto request, string? ipAddress, string? deviceInfo)
        {
            return request.UserType switch
            {
                UserType.Creator => await LoginCreatorAsync(request, ipAddress, deviceInfo),
                UserType.Member => await LoginMemberAsync(request, ipAddress, deviceInfo),
                _ => throw new RequestValidationException("Invalid user type.")
            };
        }

        private async Task<AuthResponseDto> RegisterCreatorAsync(RegisterRequestDto request, string? ipAddress, string? deviceInfo)
        {
            TeamCreator? existing = await creators.GetCreatorByEmailAsync(request.Email);
            if (existing != null) throw new EmailAlreadyExistsException();

            TeamCreator creator = new TeamCreator
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Password = hasher.HashPassword(null, request.Password),
            };

            TeamCreator createdCreator = await creators.CreateCreatorAsync(creator);
            Session createdSession = await CreateSessionAsync(createdCreator.Id, ipAddress, deviceInfo);

            return (new RegisterResponseDto(
                createdSession.Id,
                new UserDto(createdCreator.Id, createdCreator.Name, createdCreator.Email, UserType.Creator, null))
            );
        }

        private async Task<AuthResponseDto> RegisterMemberAsync(RegisterRequestDto request, string? ipAddress, string? deviceInfo)
        {
            TeamMember? existing = await members.GetMemberByEmailAsync(request.Email);
            if (existing != null) throw new EmailAlreadyExistsException();

            TeamMember member = new TeamMember
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Password = hasher.HashPassword(null, request.Password),
                Timezone = request.Timezone
            };

            TeamMember createdMember = await members.CreateMemberAsync(member);
            Session createdSession = await CreateSessionAsync(createdMember.Id,ipAddress,deviceInfo);

            return (new RegisterResponseDto(
                createdSession.Id,
                new UserDto(createdMember.Id, createdMember.Name, createdMember.Email, UserType.Member, createdMember.Timezone))
            );
        }

        private async Task<AuthResponseDto> LoginCreatorAsync(LoginRequestDto request, string? ipAddress, string? deviceInfo)
        {
            TeamCreator? creator = await creators.GetCreatorByEmailAsync(request.Email);
            if (creator == null)
            {
                throw new InvalidCredentialsException("Invalid email or password.");
            }

            PasswordVerificationResult passwordResult = hasher.VerifyHashedPassword(null!, creator.Password, request.Password);
            if(passwordResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException("Invalid email or password.");
            }

            Session createdSession = await CreateSessionAsync(creator.Id, ipAddress, deviceInfo);

            return new AuthResponseDto(
                createdSession.Id,
                new UserDto(creator.Id, creator.Name, creator.Email, UserType.Creator, null)
            );
        }

        private async Task<AuthResponseDto> LoginMemberAsync(LoginRequestDto request, string? ipAddress, string? deviceInfo)
        {
            TeamMember? member = await members.GetMemberByEmailAsync(request.Email);
            if (member == null)
            {
                throw new InvalidCredentialsException("Invalid email or password.");
            }
            PasswordVerificationResult passwordResult = hasher.VerifyHashedPassword(null!, member.Password, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException("Invalid email or password.");
            }

            Session createdSession = await CreateSessionAsync(member.Id, ipAddress, deviceInfo);

            return new AuthResponseDto(
                createdSession.Id,
                new UserDto(member.Id, member.Name, member.Email, UserType.Member, member.Timezone)
            );
        }

        private async Task<Session> CreateSessionAsync(Guid userId, string? ipAddress, string? deviceInfo)
        {
            Session session = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                SessionState = SessionState.Active,
                IpAddress = ipAddress ?? string.Empty,
                DeviceInfo = deviceInfo ?? string.Empty
            };

            return await sessions.CreateAsync(session);
        }
    }
}
          

