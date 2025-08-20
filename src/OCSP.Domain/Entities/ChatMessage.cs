using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ChatMessage : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;

        // Người gửi
        public Guid SenderId { get; set; }
        public User? Sender { get; set; }

        // Thuộc Conversation nào
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }
    }
}
