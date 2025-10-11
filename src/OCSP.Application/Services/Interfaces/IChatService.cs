using OCSP.Application.DTOs.Chat;
using OCSP.Domain.Entities;

namespace OCSP.Application.Services.Interfaces
{
    public interface IChatService
    {
        Task<Conversation> StartConversationAsync(Guid? projectId, Guid[] participantIds); // Make projectId nullable
        Task<ChatMessageResult> SendMessageAsync(Guid conversationId, Guid senderId, string content);
        Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId);
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId);
    }
}