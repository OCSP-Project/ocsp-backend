using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ConversationParticipant : AuditableEntity
    {


        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        // Vai trò của user trong conversation (Admin=0, Supervisor=1, Contractor=2, Homeowner=3)
        public int Role { get; set; }
    }
}
