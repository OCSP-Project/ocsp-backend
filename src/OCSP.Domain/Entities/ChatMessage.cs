using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ChatMessage : AuditableEntity
    {
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; } = false;

        // Navigation properties
        public Guid SenderId { get; set; }
        public User? Sender { get; set; }

        public Guid ReceiverId { get; set; }
        public User? Receiver { get; set; }

        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}