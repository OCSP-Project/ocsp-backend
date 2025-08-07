using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Payment : AuditableEntity
    {
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }

        // Navigation properties
        public Guid ContractId { get; set; }
        public Contract? Contract { get; set; }
    }
}
