using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ProposalItem : AuditableEntity
    {
        public Guid ProposalId { get; set; }
        public Proposal Proposal { get; set; } = default!;

        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = "gói";
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Qty * UnitPrice; // không map cột DB
    }
}
