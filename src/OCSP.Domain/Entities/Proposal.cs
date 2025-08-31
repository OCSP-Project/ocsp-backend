using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public class Proposal : AuditableEntity
    {
        public Guid QuoteRequestId { get; set; }
        public QuoteRequest QuoteRequest { get; set; } = default!;

        public Guid ContractorUserId { get; set; }   // User.Id của nhà thầu

        public ProposalStatus Status { get; set; } = ProposalStatus.Draft;

        public decimal PriceTotal { get; set; }      // tổng giá
        public int DurationDays { get; set; }        // thời gian thi công (ngày)
        public string? TermsSummary { get; set; }    // điều khoản tóm tắt

        public ICollection<ProposalItem> Items { get; set; } = new List<ProposalItem>();
    }
}
