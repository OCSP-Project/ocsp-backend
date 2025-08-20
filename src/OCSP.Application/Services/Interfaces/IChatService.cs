using OCSP.Domain.Entities;

namespace OCSP.Application.Services.Interfaces
{
    public interface IChatService
    {
        Task<Conversation> StartConversationAsync(Guid projectId, Guid[] participantIds);
        Task<ChatMessage> SendMessageAsync(Guid conversationId, Guid senderId, string content);
        Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId);
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId);
    }
}
