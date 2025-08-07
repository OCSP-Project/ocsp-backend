using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class MaterialUsage : AuditableEntity
    {
        public string MaterialName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime UsageDate { get; set; }

        // Navigation properties
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}