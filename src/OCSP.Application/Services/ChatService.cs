using Microsoft.EntityFrameworkCore;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Conversation> StartConversationAsync(Guid projectId, Guid[] participantIds)
        {
            // Check if conversation already exists for project + participants
            var existing = await _context.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c =>
                    c.ProjectId == projectId &&
                    c.Participants.Count == participantIds.Length &&
                    c.Participants.All(p => participantIds.Contains(p.UserId))
                );

            if (existing != null) return existing;

            var conversation = new Conversation
            {
                ProjectId = projectId,
                Participants = participantIds.Select(id => new ConversationParticipant
                {
                    UserId = id
                }).ToList()
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return conversation;
        }

        public async Task<ChatMessage> SendMessageAsync(Guid conversationId, Guid senderId, string content)
        {
            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId)
        {
            return await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId)
        {
            return await _context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .ToListAsync();
        }
    }
}
