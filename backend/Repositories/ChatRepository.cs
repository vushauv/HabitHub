using backend.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public class ChatRepository(AppDbContext db): IChatRepository
    {
        public async Task<TeamChat?> GetChatByTeamIdAsync(Guid teamId) =>
            await db.TeamChats.FirstOrDefaultAsync(c => c.TeamId == teamId);
        public async Task<TeamChat> CreateChatAsync(TeamChat chat)
        {
            db.TeamChats.Add(chat);
            await db.SaveChangesAsync();

            return chat;
        }
        public async Task<List<Message>> GetMessageByTeamIdAsync(Guid teamId) =>
            await db.Messages
                .Where(m => m.Chat.TeamId == teamId)
                .OrderBy(m => m.SendDate)
                .ToListAsync();

        public async Task<Message?> GetMessageByIdAsync(Guid messageId) =>
            await db.Messages
                .FirstOrDefaultAsync(m => m.MessageId == messageId);
        public async Task<Message?> GetMessageByIdAndTeamIdAsync(Guid messageId, Guid teamId) =>
            await db.Messages
                .FirstOrDefaultAsync(m => 
                m.MessageId == messageId &&
                m.Chat.TeamId == teamId);
        public async Task<Message> CreateMessageAsync(Message message)
        {
            db.Messages.Add(message);
            await db.SaveChangesAsync();

            return message;
        }
        public async Task<bool> DeleteMessageAsync(Guid messageId)
        {
            Message? message = await db.Messages.FirstOrDefaultAsync(m => m.MessageId == messageId);
            if (message == null)
                return false;

            db.Messages.Remove(message);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
