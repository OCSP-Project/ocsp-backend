using OCSP.Domain.Enums;
using OCSP.Domain.Common;
namespace OCSP.Domain.Entities
{
    public class QuoteInvite : AuditableEntity
    {
        public Guid QuoteRequestId { get; set; }
        public QuoteRequest QuoteRequest { get; set; } = default!;

        // Mời theo UserId của Contractor
        public Guid ContractorUserId { get; set; }

        public bool Accepted { get; set; } = false; // contractor accept lời mời (tuỳ UI)
    }
}
