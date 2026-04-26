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
            string name = NormalizeName(request.Name);
            string email = NormalizeEmail(request.Email);

            if (await creators.EmailAlreadyExistsAsync(email))
                throw new EmailAlreadyExistsException();

            TeamCreator creator = new TeamCreator
            {
                CreatorId = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = hasher.HashPassword(null!, request.Password),
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
            string name = NormalizeName(request.Name);
            string email = NormalizeEmail(request.Email);
            string timezone = NormalizeTimezone(request.Timezone);

            if (await members.EmailAlreadyExistsAsync(email))
                throw new EmailAlreadyExistsException();

            TeamMember member = new TeamMember
            {
                MemberId = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = hasher.HashPassword(null!, request.Password),
                Timezone = timezone
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
            string email = NormalizeEmail(request.Email);

            TeamCreator? creator = await creators.GetCreatorByEmailAsync(email);
            if (creator == null) throw new InvalidCredentialsException();
           
            PasswordVerificationResult passwordResult = hasher.VerifyHashedPassword(null!, creator.PasswordHash, request.Password);
            if(passwordResult == PasswordVerificationResult.Failed) throw new InvalidCredentialsException();

            Session createdSession = await CreateSessionAsync(creator.CreatorId, UserType.Creator, ipAddress, deviceInfo);
            return new AuthResponseDto(
                createdSession.SessionId,
                new UserDto(creator.CreatorId, creator.Name, creator.Email, UserType.Creator, null)
            );
        }

        private async Task<AuthResponseDto> LoginMemberAsync(LoginRequestDto request, string? ipAddress, string? deviceInfo)
        {
            string email = NormalizeEmail(request.Email);

            TeamMember? member = await members.GetMemberByEmailAsync(email);
            if (member == null) throw new InvalidCredentialsException();
            
            PasswordVerificationResult passwordResult = hasher.VerifyHashedPassword(null!, member.PasswordHash, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed) throw new InvalidCredentialsException();
       
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
                    s.SessionId == currentSessionId,
                    s.DeviceInfo,
                    s.IpAddress
                )).ToList();
        }

        public async Task InvalidateSpecificSession(Guid userId, UserType userType, string sessionId)
        {
            Session? session = await sessions.GetByIdAsync(sessionId);
            if(session == null 
                || session.UserType != userType 
                || session.UserId != userId 
                || session.SessionState != SessionState.Active)
            {
                throw new AppException( StatusCodes.Status404NotFound, "not-found", "Session not found");
            }
            await sessions.InvalidateAsync(session.SessionId);
        }
        public async Task ChangePassword(Guid userId, UserType userType, string currentSessionId, ChangePasswordRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new AppException(StatusCodes.Status400BadRequest, "validation-error", "Invalid request body.");
            }

            if(userType == UserType.Creator)
            {
                TeamCreator? creator = await creators.GetCreatorByIdAsync(userId);
                if(creator == null)
                    throw new AppException(StatusCodes.Status404NotFound, "not-found", "User not found.");
                
                var verifyResult = hasher.VerifyHashedPassword(null!, creator.PasswordHash, request.CurrentPassword);
                if(verifyResult == PasswordVerificationResult.Failed)
                    throw new AppException(StatusCodes.Status401Unauthorized, "invalid-credentials", "Invalid credentials.");

                string newHash = hasher.HashPassword(null!, request.NewPassword);
                
                await creators.UpdatePasswordAsync(userId, newHash);

            } else if(userType == UserType.Member)
            {
                TeamMember? member = await members.GetMemberByIdAsync(userId);
                if(member == null)
                    throw new AppException(StatusCodes.Status404NotFound, "not-found", "User not found.");
                
                var verifyResult = hasher.VerifyHashedPassword(null!, member.PasswordHash, request.CurrentPassword);
                if(verifyResult == PasswordVerificationResult.Failed)
                    throw new AppException(StatusCodes.Status401Unauthorized, "invalid-credentials", "Invalid credentials.");

                string newHash = hasher.HashPassword(null!, request.NewPassword);
                
                await members.UpdatePasswordAsync(userId, newHash);
            } else
            {
                throw new AppException(StatusCodes.Status400BadRequest, "validation-error", "Invalid user type.");
            }

            await sessions.InvalidateAllExceptCurrentAsync(userId, userType, currentSessionId);
        }
        public async Task ChangeEmail(Guid userId, UserType userType, string currentSessionId, ChangeEmailRequestDto request)
        {
            var email = NormalizeEmail(request.NewEmail);
            if(string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
                throw new AppException(StatusCodes.Status400BadRequest, "validation-error", "Invalid request body.");

            if(userType == UserType.Creator)
            {
                if(await creators.EmailAlreadyExistsAsync(email))
                    throw new AppException(StatusCodes.Status409Conflict, "email-already-exists", "Email already exists.");

                TeamCreator? creator = await creators.GetCreatorByIdAsync(userId);
                if(creator == null)
                    throw new AppException(StatusCodes.Status401Unauthorized, "invalid-credentials", "User not found."); //good error here?

                var verifyResult = hasher.VerifyHashedPassword(null!, creator.PasswordHash, request.Password);
                if(verifyResult == PasswordVerificationResult.Failed)
                    throw new AppException(StatusCodes.Status401Unauthorized, "invalid-credentials", "Invalid credentials.");

                await creators.ChangeEmailAsync(userId, email);

            } else if(userType == UserType.Member)
            {
                if(await members.EmailAlreadyExistsAsync(email))
                    throw new AppException(StatusCodes.Status409Conflict, "email-already-exists", "Email already exists.");

                TeamMember? member = await members.GetMemberByIdAsync(userId);
                if(member == null)
                    throw new AppException(StatusCodes.Status401Unauthorized, "invalid-credentials", "User not found."); //good error here?
                
                var verifyResult = hasher.VerifyHashedPassword(null!, member.PasswordHash, request.Password);
                if(verifyResult == PasswordVerificationResult.Failed)
                    throw new AppException(StatusCodes.Status401Unauthorized, "invalid-credentials", "Invalid credentials.");

                await members.ChangeEmailAsync(userId, email);
            }
            else
            {
                throw new AppException(StatusCodes.Status400BadRequest, "validation-error", "Invalid user type.");
            }

            await sessions.InvalidateAllExceptCurrentAsync(userId, userType, currentSessionId);
        }

        public async Task<UserDto> GetMe(Guid userId, UserType userType)
        {
            if (userType == UserType.Creator)
            {
                TeamCreator? creator = await creators.GetCreatorByIdAsync(userId);
                if (creator == null)
                    throw new NotFoundException();

                return new UserDto(creator.CreatorId, creator.Name, creator.Email, UserType.Creator, null);
            }
            else if (userType == UserType.Member)
            {
                TeamMember? member = await members.GetMemberByIdAsync(userId);
                if (member == null)
                    throw new NotFoundException();

                return new UserDto(member.MemberId, member.Name, member.Email, UserType.Member, member.Timezone);
            }
            else
            {
                throw new AuthRequiredException();
            }
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
        private static string NormalizeName(string name) => name.Trim();
        private static string NormalizeTimezone(string timezone) => timezone.Trim();
    }
}
          

