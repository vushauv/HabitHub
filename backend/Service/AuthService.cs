using backend.Dtos.AuthDtos;
using backend.Logging;
using Microsoft.AspNetCore.Identity;
using backend.Models;
using backend.Exceptions;
using backend.Enums;
using backend.Utils;
using backend.Service.Interfaces;
using backend.Repositories.Interfaces;
using backend.Data.UnitOfWork;

namespace backend.Service
{
    public class AuthService(
        ITeamCreatorRepository creators, 
        ITeamMemberRepository members, 
        ISessionRepository sessions, 
        INotificationRepository notifications, 
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger) : IAuthService
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
            {
                logger.LogWarning("Registration rejected: email already exists for {UserType}", UserType.Creator);
                throw new EmailAlreadyExistsException();
            }

            TeamCreator creator = new TeamCreator
            {
                CreatorId = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = hasher.HashPassword(null!, request.Password),
            };

            AuthResponseDto response = await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                TeamCreator createdCreator = await creators.CreateCreatorAsync(creator);
                Session createdSession = await CreateSessionAsync(createdCreator.CreatorId, UserType.Creator, ipAddress, deviceInfo);

                return new AuthResponseDto(
                    createdSession.SessionId,
                    new UserDto(createdCreator.CreatorId, createdCreator.Name, createdCreator.Email, UserType.Creator, null))
                ;
            });

            logger.LogInformation("Registered creator {UserId}", response.User.Id);
            return response;
        }

        private async Task<AuthResponseDto> RegisterMemberAsync(RegisterRequestDto request, string? ipAddress, string? deviceInfo)
        {
            string name = NormalizeName(request.Name);
            string email = NormalizeEmail(request.Email);
            string timezone = NormalizeTimezone(request.Timezone);

            if (await members.EmailAlreadyExistsAsync(email))
            {
                logger.LogWarning("Registration rejected: email already exists for {UserType}", UserType.Member);
                throw new EmailAlreadyExistsException();
            }

            TeamMember member = new TeamMember
            {
                MemberId = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = hasher.HashPassword(null!, request.Password),
                Timezone = timezone
            };

            AuthResponseDto response = await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                TeamMember createdMember = await members.CreateMemberAsync(member);
                Session createdSession = await CreateSessionAsync(createdMember.MemberId, UserType.Member, ipAddress, deviceInfo);

                return new AuthResponseDto(
                    createdSession.SessionId,
                    new UserDto(createdMember.MemberId, createdMember.Name, createdMember.Email, UserType.Member, createdMember.Timezone))
                ;
            });
            logger.LogInformation("Registered member {MemberId}", response.User.Id);
            return response;
        }

        private async Task<AuthResponseDto> LoginCreatorAsync(LoginRequestDto request, string? ipAddress, string? deviceInfo)
        {
            string email = NormalizeEmail(request.Email);

            TeamCreator? creator = await creators.GetCreatorByEmailAsync(email);
            if (creator == null)
            {
                logger.LogWarning("Login failed: creator not found");
                throw new InvalidCredentialsException();
            }

            PasswordVerificationResult passwordResult = hasher.VerifyHashedPassword(null!, creator.PasswordHash, request.Password);
            if(passwordResult == PasswordVerificationResult.Failed)
            {
                logger.LogWarning("Login failed: invalid password for creator {CreatorId}", creator.CreatorId);
                throw new InvalidCredentialsException();
            }

            Session createdSession = await CreateSessionAsync(creator.CreatorId, UserType.Creator, ipAddress, deviceInfo);
            logger.LogInformation("Creator {CreatorId} logged in", creator.CreatorId);
            return new AuthResponseDto(
                createdSession.SessionId,
                new UserDto(creator.CreatorId, creator.Name, creator.Email, UserType.Creator, null)
            );
        }

        private async Task<AuthResponseDto> LoginMemberAsync(LoginRequestDto request, string? ipAddress, string? deviceInfo)
        {
            string email = NormalizeEmail(request.Email);

            TeamMember? member = await members.GetMemberByEmailAsync(email);
            if (member == null)
            {
                logger.LogWarning("Login failed: member not found");
                throw new InvalidCredentialsException();
            }

            PasswordVerificationResult passwordResult = hasher.VerifyHashedPassword(null!, member.PasswordHash, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                logger.LogWarning("Login failed: invalid password for member {MemberId}", member.MemberId);
                throw new InvalidCredentialsException();
            }

            Session createdSession = await CreateSessionAsync(member.MemberId, UserType.Member, ipAddress, deviceInfo);
            logger.LogInformation("Member {MemberId} logged in", member.MemberId);
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
                logger.LogWarning("Invalidate session rejected: session {SessionFingerprint} not found or unauthorized for user {UserId}",
                    LogRedaction.Fingerprint(sessionId), userId);
                throw new NotFoundException();
            }
            await sessions.InvalidateAsync(session.SessionId);
            logger.LogInformation("Invalidated session {SessionFingerprint} for user {UserId} ({UserType})",
                LogRedaction.Fingerprint(sessionId), userId, userType);
        }
        public async Task ChangePassword(Guid userId, UserType userType, string currentSessionId, ChangePasswordRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new RequestValidationException("Invalid request body.");
            }

            if(userType == UserType.Creator)
            {
                TeamCreator? creator = await creators.GetCreatorByIdAsync(userId);
                if(creator == null)
                    throw new NotFoundException("user-not-found", "User not found.");
                
                var verifyResult = hasher.VerifyHashedPassword(null!, creator.PasswordHash, request.CurrentPassword);
                if(verifyResult == PasswordVerificationResult.Failed)
                {
                    logger.LogWarning("Change password rejected: invalid current password for creator {CreatorId}", userId);
                    throw new InvalidCredentialsException();
                }
            } 
            else if(userType == UserType.Member)
            {
                TeamMember? member = await members.GetMemberByIdAsync(userId);
                if(member == null)
                    throw new NotFoundException("user-not-found", "User not found.");

                var verifyResult = hasher.VerifyHashedPassword(null!, member.PasswordHash, request.CurrentPassword);
                if(verifyResult == PasswordVerificationResult.Failed)
                {
                    logger.LogWarning("Change password rejected: invalid current password for member {MemberId}", userId);
                    throw new InvalidCredentialsException();
                }
            } 
            else
            {
                throw new AuthRequiredException(); 
            }

            string newHash = hasher.HashPassword(null!, request.NewPassword);

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (userType == UserType.Creator)
                {
                    await creators.UpdatePasswordAsync(userId, newHash);
                }
                else if (userType == UserType.Member)
                {
                    await members.UpdatePasswordAsync(userId, newHash);
                }
                else
                {
                    throw new AuthRequiredException();
                }
                await CreateSystemNotification(userId, userType, "Your password was changed.");
                await sessions.InvalidateAllExceptCurrentAsync(userId, userType, currentSessionId);
            });
           
            logger.LogInformation("Changed password for {UserType} {UserId}", userType, userId);
        }
        public async Task ChangeEmail(Guid userId, UserType userType, string currentSessionId, ChangeEmailRequestDto request)
        {
            var email = NormalizeEmail(request.NewEmail);
            if(string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
                throw new RequestValidationException("Invalid request body.");

            if(userType == UserType.Creator)
            {
                if(await creators.EmailAlreadyExistsAsync(email))
                {
                    logger.LogWarning("Change email rejected: email already exists for creator {CreatorId}", userId);
                    throw new EmailAlreadyExistsException();
                }

                TeamCreator? creator = await creators.GetCreatorByIdAsync(userId);
                if(creator == null)
                    throw new NotFoundException("user-not-found", "User not found.");

                var verifyResult = hasher.VerifyHashedPassword(null!, creator.PasswordHash, request.Password);
                if(verifyResult == PasswordVerificationResult.Failed)
                {
                    logger.LogWarning("Change email rejected: invalid password for creator {CreatorId}", userId);
                    throw new InvalidCredentialsException();
                }
            } 
            else if(userType == UserType.Member)
            {
                if(await members.EmailAlreadyExistsAsync(email))
                {
                    logger.LogWarning("Change email rejected: email already exists for member {MemberId}", userId);
                    throw new EmailAlreadyExistsException();
                }

                TeamMember? member = await members.GetMemberByIdAsync(userId);
                if(member == null)
                    throw new NotFoundException("user-not-found", "User not found.");

                var verifyResult = hasher.VerifyHashedPassword(null!, member.PasswordHash, request.Password);
                if(verifyResult == PasswordVerificationResult.Failed)
                {
                    logger.LogWarning("Change email rejected: invalid password for member {MemberId}", userId);
                    throw new InvalidCredentialsException();
                }
            }
            else
            {
                throw new AuthRequiredException();
            }

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (userType == UserType.Creator)
                {
                    await creators.ChangeEmailAsync(userId, email);
                }
                else if (userType == UserType.Member)
                {
                    await members.ChangeEmailAsync(userId, email);
                }
                else
                {
                    throw new AuthRequiredException();
                }
                await CreateSystemNotification(userId, userType, "Your email address was changed.");
                await sessions.InvalidateAllExceptCurrentAsync(userId, userType, currentSessionId);
            });
            logger.LogInformation("Changed email for {UserType} {UserId}", userType, userId);
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
        private async Task CreateSystemNotification(Guid userId, UserType userType, string content)
        {
            Notification notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                UserType = userType,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                Status = NotificationStatus.Unread,
                Type = NotificationType.System
            };

            await notifications.CreateNotificationAsync(notification);
        }
    }
}
          

