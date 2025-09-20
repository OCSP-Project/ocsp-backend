using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public class PaymentTransaction : AuditableEntity
    {
        public Guid ContractId { get; set; }
        public Contract Contract { get; set; } = default!;

        public Guid? MilestoneId { get; set; }
        public ContractMilestone? Milestone { get; set; }

        public PaymentProvider Provider { get; set; }
        public PaymentType Type { get; set; }
        public PaymentStatus Status { get; set; }

        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? ProviderTxnId { get; set; }
    }
}
