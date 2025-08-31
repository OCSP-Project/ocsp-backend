// OCSP.Domain/Entities/QuoteRequest.cs
using OCSP.Domain.Enums;
using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class QuoteRequest : AuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        public string Scope { get; set; } = string.Empty;   // mô tả phạm vi
        public DateTime? DueDate { get; set; }              // hạn contractor nộp báo giá
        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

        public ICollection<QuoteInvite> Invites { get; set; } = new List<QuoteInvite>();
    }
}

