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

        public async Task<List<MessageDto>> GetMessages(User user, Guid teamId, int offset, int count)
        {
            if (offset < 0)
                throw new RequestValidationException("Offset must be non-negative.");
            if (count <= 0 || count > MaxMessagesPerPage)
                throw new RequestValidationException($"Count must be between 1 and {MaxMessagesPerPage}.");

            (HabitTeam team, _) = await EnsureTeamAccessAsync(user, teamId, "Get messages");

            List<Message> messages = await chats.GetMessagesByTeamIdAsync(teamId, offset, count);

            Dictionary<Guid, string> memberNames = await ResolveMemberNamesAsync(messages);
            bool hasCreatorMsg = messages.Any(m => m.User is TeamCreator);
            string? creatorName = hasCreatorMsg ? await ResolveCreatorNameAsync(team.CreatorId) : null;

            return messages.Select(m => new MessageDto(
                m.MessageId,
                m.UserId,
                m.User!.GetUserType(),
                m.User is TeamCreator
                    ? (creatorName ?? "Unknown")
                    : memberNames.GetValueOrDefault(m.UserId, "Unknown"),
                m.Content,
                m.SendDate
            )).ToList();
        }

        public async Task<MessageDto> SendMessage(User user, Guid teamId, SendMessageRequestDto request)
        {
            (HabitTeam team, TeamMember? member) = await EnsureTeamAccessAsync(user, teamId, "Send message");

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
                UserId = user.UserId,
                Content = content,
                SendDate = DateTime.UtcNow
            };
            Message created = await chats.CreateMessageAsync(message);
            logger.LogInformation("User {UserId} sent message {MessageId} in team {TeamId}", user.UserId, created.MessageId, teamId);

            string authorName = user is TeamCreator
                ? await ResolveCreatorNameAsync(team.CreatorId)
                : member!.Name;

            return new MessageDto(
                created.MessageId,
                created.UserId,
                user.GetUserType(),
                authorName,
                created.Content,
                created.SendDate
            );
        }

        public async Task DeleteMessage(User user, Guid teamId, Guid messageId)
        {
            (HabitTeam team, _) = await EnsureTeamAccessAsync(user, teamId, "Delete message");

            Message? message = await chats.GetMessageByIdAndTeamIdAsync(messageId, teamId);
            if (message == null)
            {
                logger.LogWarning("Delete message rejected: message {MessageId} not found in team {TeamId}", messageId, teamId);
                throw new MessageNotFoundException();
            }

            bool isAuthor = message.UserId == user.UserId;
            bool isTeamCreator = message.User is TeamCreator && team.CreatorId == user.UserId;
            if (!isAuthor && !isTeamCreator)
            {
                logger.LogWarning("Delete message rejected: user {UserId} not authorized for message {MessageId}", user.UserId, messageId);
                throw new MessageNotOwnException();
            }

            await chats.DeleteMessageAsync(messageId);
            logger.LogInformation("Deleted message {MessageId} from team {TeamId} by user {UserId}", messageId, teamId, user.UserId);
        }

        private async Task<(HabitTeam Team, TeamMember? Member)> EnsureTeamAccessAsync(User user, Guid teamId, string action)
        {
            HabitTeam? team = await habitTeams.GetHabitTeamByIdAsync(teamId);
            if (team == null)
            {
                logger.LogWarning("{Action} rejected: team {TeamId} not found", action, teamId);
                throw new NotFoundException();
            }

            if (user is TeamCreator)
            {
                bool isTeamCreator = await habitTeams.CheckOwnershipOfTeamAsync(team.TeamId, user.UserId);
                if (!isTeamCreator)
                {
                    logger.LogWarning("{Action} rejected: user {UserId} is not owner of team {TeamId}", action, user.UserId, teamId);
                    throw new ForbiddenException();
                }
                return (team, null);
            }

            if (user is TeamMember)
            {
                TeamMember? member = await members.GetMemberByIdAsync(user.UserId);
                if (member == null)
                    throw new ForbiddenException();

                bool isActiveMember = await memberships.IsActiveMembershipAsync(team.TeamId, member.UserId);
                if (!isActiveMember)
                {
                    logger.LogWarning("{Action} rejected: user {UserId} not active in team {TeamId}", action, user.UserId, teamId);
                    throw new ForbiddenException();
                }
                return (team, member);
            }

            throw new AuthRequiredException();
        }

        private async Task<Dictionary<Guid, string>> ResolveMemberNamesAsync(List<Message> messages)
        {
            List<Guid> memberIds = messages
                .Where(m => m.User is TeamMember)
                .Select(m => m.UserId)
                .Distinct()
                .ToList();

            if (memberIds.Count == 0)
                return new Dictionary<Guid, string>();

            List<TeamMember> memberList = await members.GetMembersByIdsAsync(memberIds);
            return memberList.ToDictionary(m => m.UserId, m => m.Name);
        }

        private async Task<string> ResolveCreatorNameAsync(Guid creatorId)
        {
            TeamCreator? creator = await creators.GetCreatorByIdAsync(creatorId);
            return creator?.Name ?? "Unknown";
        }

        private static string NormalizeString(string content) => content.Trim();
    }
}
