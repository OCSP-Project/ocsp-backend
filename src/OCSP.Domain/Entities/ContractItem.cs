// OCSP.Domain/Entities/Contract.cs
using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{


    public class ContractItem : AuditableEntity
    {
        public Guid ContractId { get; set; }
        public Contract Contract { get; set; } = default!;

        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
