using backend.Dtos.AuthDtos;
using backend.Repositories;
using Microsoft.AspNetCore.Identity;
using backend.Models;
using backend.Exceptions;
using backend.Enums;
using backend.Utils;

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
                CreatorId = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = hasher.HashPassword(null, request.Password),
            };

            TeamCreator createdCreator = await creators.CreateCreatorAsync(creator);
            Session createdSession = await CreateSessionAsync(createdCreator.CreatorId, UserType.Creator, ipAddress, deviceInfo);

            return new AuthResponseDto(
                createdSession.SessionId,
                new UserDto(createdCreator.CreatorId, createdCreator.Name, createdCreator.Email, UserType.Creator, null))
            ;
        }

        private async Task<AuthResponseDto> RegisterMemberAsync(RegisterRequestDto request, string? ipAddress, string? deviceInfo)
        {
            TeamMember? existing = await members.GetMemberByEmailAsync(request.Email);
            if (existing != null) throw new EmailAlreadyExistsException();

            TeamMember member = new TeamMember
            {
                MemberId = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = hasher.HashPassword(null, request.Password),
                Timezone = request.Timezone
            };

            TeamMember createdMember = await members.CreateMemberAsync(member);
            Session createdSession = await CreateSessionAsync(createdMember.MemberId, UserType.Member, ipAddress,deviceInfo);

            return new AuthResponseDto(
                createdSession.SessionId,
                new UserDto(createdMember.MemberId, createdMember.Name, createdMember.Email, UserType.Member, createdMember.Timezone))
            ;
        }

        private async Task<AuthResponseDto> LoginCreatorAsync(LoginRequestDto request, string? ipAddress, string? deviceInfo)
        {
            TeamCreator? creator = await creators.GetCreatorByEmailAsync(request.Email);
            if (creator == null)
            {
                throw new InvalidCredentialsException();
            }

            PasswordVerificationResult passwordResult = hasher.VerifyHashedPassword(null!, creator.PasswordHash, request.Password);
            if(passwordResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException();
            }

            Session createdSession = await CreateSessionAsync(creator.CreatorId, UserType.Creator, ipAddress, deviceInfo);

            return new AuthResponseDto(
                createdSession.SessionId,
                new UserDto(creator.CreatorId, creator.Name, creator.Email, UserType.Creator, null)
            );
        }

        private async Task<AuthResponseDto> LoginMemberAsync(LoginRequestDto request, string? ipAddress, string? deviceInfo)
        {
            TeamMember? member = await members.GetMemberByEmailAsync(request.Email);
            if (member == null)
            {
                throw new InvalidCredentialsException();
            }
            PasswordVerificationResult passwordResult = hasher.VerifyHashedPassword(null!, member.PasswordHash, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException();
            }

            Session createdSession = await CreateSessionAsync(member.MemberId, UserType.Member, ipAddress, deviceInfo);

            return new AuthResponseDto(
                createdSession.SessionId,
                new UserDto(member.MemberId, member.Name, member.Email, UserType.Member, member.Timezone)
            );
        }

        private async Task<Session> CreateSessionAsync(Guid userId, UserType userType, string? ipAddress, string? deviceInfo)
        {
            Session session = new()
            {
                SessionId = SessionIdGenerator.GenerateSessionId(),
                UserId = userId,
                UserType = userType,
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                SessionState = SessionState.Active,
                IpAddress = ipAddress ?? string.Empty,
                DeviceInfo = deviceInfo ?? string.Empty
            };

            return await sessions.CreateAsync(session);
        }

        public async Task<List<SessionDto>> ViewActiveSessions(Guid userId, UserType userType, string currentSessionId)
        {
            var activeSessions = await sessions.GetActiveSessionsForUserAsync(userId, userType);
            return activeSessions
                .Select(s => new SessionDto(s.SessionId, 
                    s.UserType, 
                    s.CreatedAt, 
                    s.LastActiveAt, 
                    s.ExpiresAt, 
                    s.SessionState,
                    s.SessionId == currentSessionId
                )).ToList();
        }
    }
}
          

