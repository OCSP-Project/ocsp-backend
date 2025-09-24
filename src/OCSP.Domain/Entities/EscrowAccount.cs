using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public class EscrowAccount : AuditableEntity
    {
        public Guid ContractId { get; set; }
        public Contract Contract { get; set; } = default!;

        public PaymentProvider Provider { get; set; } = PaymentProvider.Manual;
        public EscrowStatus Status { get; set; } = EscrowStatus.Pending;

        public decimal Balance { get; set; }
        public string? ExternalAccountId { get; set; }
    }
}
