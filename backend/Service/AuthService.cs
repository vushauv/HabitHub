using backend.Dtos.AuthDtos;
using backend.Logging;
using backend.Models;
using backend.Exceptions;
using backend.Enums;
using backend.Utils;
using backend.Service.Interfaces;
using backend.Repositories.Interfaces;
using backend.Data.UnitOfWork;
using backend.Configuration;
using Microsoft.Extensions.Options;

namespace backend.Service
{
    public class AuthService(
        ITeamCreatorRepository creators,
        ITeamMemberRepository members,
        ISessionRepository sessions,
        INotificationRepository notifications,
        IUnitOfWork unitOfWork,
        IOptions<AppSettings> appSettings,
        ILogger<AuthService> logger) : IAuthService
    {
        private readonly string _pepper = appSettings.Value.Pepper;
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
                UserId = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = PasswordUtils.HashPassword(request.Password, _pepper),
            };

            AuthResponseDto response = await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                TeamCreator createdCreator = await creators.CreateCreatorAsync(creator);
                var (_, rawSessionId) = await CreateSessionAsync(createdCreator.UserId, ipAddress, deviceInfo);

                return new AuthResponseDto(
                    rawSessionId,
                    new UserDto(createdCreator.UserId, createdCreator.Name, createdCreator.Email, UserType.Creator, null))
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
                UserId = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = PasswordUtils.HashPassword(request.Password, _pepper),
                Timezone = timezone
            };

            AuthResponseDto response = await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                TeamMember createdMember = await members.CreateMemberAsync(member);
                var (_, rawSessionId) = await CreateSessionAsync(createdMember.UserId, ipAddress, deviceInfo);

                return new AuthResponseDto(
                    rawSessionId,
                    new UserDto(createdMember.UserId, createdMember.Name, createdMember.Email, UserType.Member, createdMember.Timezone))
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

            if (!PasswordUtils.VerifyPassword(request.Password, creator.PasswordHash, _pepper))
            {
                logger.LogWarning("Login failed: invalid password for creator {CreatorId}", creator.UserId);
                throw new InvalidCredentialsException();
            }

            var (_, rawSessionId) = await CreateSessionAsync(creator.UserId, ipAddress, deviceInfo);
            logger.LogInformation("Creator {CreatorId} logged in", creator.UserId);
            return new AuthResponseDto(
                rawSessionId,
                new UserDto(creator.UserId, creator.Name, creator.Email, UserType.Creator, null)
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

            if (!PasswordUtils.VerifyPassword(request.Password, member.PasswordHash, _pepper))
            {
                logger.LogWarning("Login failed: invalid password for member {MemberId}", member.UserId);
                throw new InvalidCredentialsException();
            }

            var (_, rawSessionId) = await CreateSessionAsync(member.UserId, ipAddress, deviceInfo);
            logger.LogInformation("Member {MemberId} logged in", member.UserId);
            return new AuthResponseDto(
                rawSessionId,
                new UserDto(member.UserId, member.Name, member.Email, UserType.Member, member.Timezone)
            );
        }

        private async Task<(Session session, string rawId)> CreateSessionAsync(Guid userId, string? ipAddress, string? deviceInfo)
        {
            string rawId = SessionIdGenerator.GenerateSessionId();
            Session session = new()
            {
                SessionId = SessionIdHasher.Hash(rawId),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                SessionState = SessionState.Active,
                IpAddress = ipAddress ?? string.Empty,
                DeviceInfo = deviceInfo ?? string.Empty
            };

            return (await sessions.CreateAsync(session), rawId);
        }

        public async Task<List<SessionDto>> ViewActiveSessions(Guid userId, string currentSessionId)
        {
            var activeSessions = await sessions.GetActiveSessionsForUserAsync(userId);
            return activeSessions
                .Select(s => new SessionDto(s.SessionId, 
                    s.CreatedAt, 
                    s.LastActiveAt, 
                    s.ExpiresAt, 
                    s.SessionState,
                    s.SessionId == currentSessionId,
                    s.DeviceInfo,
                    s.IpAddress
                )).ToList();
        }

        public async Task InvalidateSpecificSession(Guid userId, string sessionId)
        {
            Session? session = await sessions.GetByIdAsync(sessionId);
            if(session == null
                || session.UserId != userId
                || session.SessionState != SessionState.Active)
            {
                logger.LogWarning("Invalidate session rejected: session {SessionFingerprint} not found or unauthorized for user {UserId}",
                    LogRedaction.Fingerprint(sessionId), userId);
                throw new NotFoundException();
            }
            await sessions.InvalidateAsync(session.SessionId);
            logger.LogInformation("Invalidated session {SessionFingerprint} for user {UserId}",
                LogRedaction.Fingerprint(sessionId), userId);
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

                if (!PasswordUtils.VerifyPassword(request.CurrentPassword, creator.PasswordHash, _pepper))
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

                if (!PasswordUtils.VerifyPassword(request.CurrentPassword, member.PasswordHash, _pepper))
                {
                    logger.LogWarning("Change password rejected: invalid current password for member {MemberId}", userId);
                    throw new InvalidCredentialsException();
                }
            }
            else
            {
                throw new AuthRequiredException();
            }

            string newHash = PasswordUtils.HashPassword(request.NewPassword, _pepper);

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
                await CreateSystemNotification(userId, "Your password was changed.");
                await sessions.InvalidateAllExceptCurrentAsync(userId, currentSessionId);
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

                if (!PasswordUtils.VerifyPassword(request.Password, creator.PasswordHash, _pepper))
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

                if (!PasswordUtils.VerifyPassword(request.Password, member.PasswordHash, _pepper))
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
                await CreateSystemNotification(userId, "Your email address was changed.");
                await sessions.InvalidateAllExceptCurrentAsync(userId, currentSessionId);
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

                return new UserDto(creator.UserId, creator.Name, creator.Email, UserType.Creator, null);
            }
            else if (userType == UserType.Member)
            {
                TeamMember? member = await members.GetMemberByIdAsync(userId);
                if (member == null)
                    throw new NotFoundException();

                return new UserDto(member.UserId, member.Name, member.Email, UserType.Member, member.Timezone);
            }
            else
            {
                throw new AuthRequiredException();
            }
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
        private static string NormalizeName(string name) => name.Trim();
        private static string NormalizeTimezone(string timezone) => timezone.Trim();
        private async Task CreateSystemNotification(Guid userId, string content)
        {
            Notification notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                Status = NotificationStatus.Unread,
                Type = NotificationType.System
            };

            await notifications.CreateNotificationAsync(notification);
        }
    }
}
          

