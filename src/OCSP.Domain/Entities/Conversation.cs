using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Conversation : AuditableEntity
    {
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }

        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
