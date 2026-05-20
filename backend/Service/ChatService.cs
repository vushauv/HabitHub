using backend.Dtos.ChatDtos;
using backend.Enums;
using backend.Exceptions;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Service.Interfaces;

namespace backend.Service
{
    public class ChatService(
        IHabitTeamRepository habitTeams,
        ITeamMemberRepository members,
        ITeamCreatorRepository creators,
        IMembershipRepository memberships,
        IChatRepository chats,
        ILogger<ChatService> logger
        ) : IChatService
    {
        private const int MaxMessagesPerPage = 100;

        public async Task<List<MessageDto>> GetMessages(Guid userId, UserType userType, Guid teamId, int offset, int count)
        {
            if (offset < 0)
                throw new RequestValidationException("Offset must be non-negative.");
            if (count <= 0 || count > MaxMessagesPerPage)
                throw new RequestValidationException($"Count must be between 1 and {MaxMessagesPerPage}.");

            (HabitTeam team, _) = await EnsureTeamAccessAsync(userId, userType, teamId, "Get messages");

            List<Message> messages = await chats.GetMessagesByTeamIdAsync(teamId, offset, count);

            Dictionary<Guid, string> memberNames = await ResolveMemberNamesAsync(messages);
            bool hasCreatorMsg = messages.Any(m => m.UserType == UserType.Creator);
            string? creatorName = hasCreatorMsg ? await ResolveCreatorNameAsync(team.CreatorId) : null;

            return messages.Select(m => new MessageDto(
                m.MessageId,
                m.UserId,
                m.UserType,
                m.UserType == UserType.Creator
                    ? (creatorName ?? "Unknown")
                    : memberNames.GetValueOrDefault(m.UserId, "Unknown"),
                m.Content,
                m.SendDate
            )).ToList();
        }

        public async Task<MessageDto> SendMessage(Guid userId, UserType userType, Guid teamId, SendMessageRequestDto request)
        {
            (HabitTeam team, TeamMember? member) = await EnsureTeamAccessAsync(userId, userType, teamId, "Send message");

            string content = NormalizeString(request.Content);
            if (content.Length == 0)
                throw new RequestValidationException("Message content is required.");

            TeamChat? chat = await chats.GetChatByTeamIdAsync(teamId);
            if (chat == null)
            {
                logger.LogWarning("Send message rejected: chat for team {TeamId} not found", teamId);
                throw new NotFoundException();
            }

            Message message = new Message
            {
                MessageId = Guid.NewGuid(),
                ChatId = chat.ChatId,
                UserId = userId,
                UserType = userType,
                Content = content,
                SendDate = DateTime.UtcNow
            };
            Message created = await chats.CreateMessageAsync(message);
            logger.LogInformation("User {UserId} sent message {MessageId} in team {TeamId}", userId, created.MessageId, teamId);

            string authorName = userType == UserType.Creator
                ? await ResolveCreatorNameAsync(team.CreatorId)
                : member!.Name;

            return new MessageDto(
                created.MessageId,
                created.UserId,
                created.UserType,
                authorName,
                created.Content,
                created.SendDate
            );
        }

        public async Task DeleteMessage(Guid userId, UserType userType, Guid teamId, Guid messageId)
        {
            (HabitTeam team, _) = await EnsureTeamAccessAsync(userId, userType, teamId, "Delete message");

            Message? message = await chats.GetMessageByIdAndTeamIdAsync(messageId, teamId);
            if (message == null)
            {
                logger.LogWarning("Delete message rejected: message {MessageId} not found in team {TeamId}", messageId, teamId);
                throw new MessageNotFoundException();
            }

            bool isAuthor = message.UserId == userId && message.UserType == userType;
            bool isTeamCreator = userType == UserType.Creator && team.CreatorId == userId;
            if (!isAuthor && !isTeamCreator)
            {
                logger.LogWarning("Delete message rejected: user {UserId} not authorized for message {MessageId}", userId, messageId);
                throw new MessageNotOwnException();
            }

            await chats.DeleteMessageAsync(messageId);
            logger.LogInformation("Deleted message {MessageId} from team {TeamId} by user {UserId}", messageId, teamId, userId);
        }

        private async Task<(HabitTeam Team, TeamMember? Member)> EnsureTeamAccessAsync(Guid userId, UserType userType, Guid teamId, string action)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
            {
                logger.LogWarning("{Action} rejected: team {TeamId} not found", action, teamId);
                throw new NotFoundException();
            }

            if (userType == UserType.Creator)
            {
                bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, userId);
                if (!isTeamCreator)
                {
                    logger.LogWarning("{Action} rejected: user {UserId} is not owner of team {TeamId}", action, userId, teamId);
                    throw new ForbiddenException();
                }
                return (team, null);
            }

            if (userType == UserType.Member)
            {
                TeamMember? member = await members.GetMemberByIdAsync(userId);
                if (member == null)
                    throw new ForbiddenException();

                bool isActiveMember = await memberships.IsActiveMembershipAsync(team.TeamId, member.MemberId);
                if (!isActiveMember)
                {
                    logger.LogWarning("{Action} rejected: user {UserId} not active in team {TeamId}", action, userId, teamId);
                    throw new ForbiddenException();
                }
                return (team, member);
            }

            throw new AuthRequiredException();
        }

        private async Task<Dictionary<Guid, string>> ResolveMemberNamesAsync(List<Message> messages)
        {
            List<Guid> memberIds = messages
                .Where(m => m.UserType == UserType.Member)
                .Select(m => m.UserId)
                .Distinct()
                .ToList();

            if (memberIds.Count == 0)
                return new Dictionary<Guid, string>();

            List<TeamMember> memberList = await members.GetMembersByIdsAsync(memberIds);
            return memberList.ToDictionary(m => m.MemberId, m => m.Name);
        }

        private async Task<string> ResolveCreatorNameAsync(Guid creatorId)
        {
            TeamCreator? creator = await creators.GetCreatorByIdAsync(creatorId);
            return creator?.Name ?? "Unknown";
        }

        private static string NormalizeString(string content) => content.Trim();
    }
}
