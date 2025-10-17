using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ProposalItem : AuditableEntity
    {
        public Guid ProposalId { get; set; }
        public Proposal Proposal { get; set; } = default!;

        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Notes { get; set; } // For percentage information
    }
}
