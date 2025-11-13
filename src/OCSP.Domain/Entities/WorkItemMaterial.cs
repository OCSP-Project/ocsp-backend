using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class WorkItemMaterial : AuditableEntity
    {
        public Guid WorkItemId { get; set; }
        public WorkItem? WorkItem { get; set; }

        public string Name { get; set; } = string.Empty;                 // Material name
        public string? Code { get; set; }                                // Material code (if any)
        public string Unit { get; set; } = string.Empty;                 // Unit (kg, m³, pcs...)

        public decimal PlannedQuantity { get; set; }                     // Planned quantity
        public decimal? ActualQuantity { get; set; }                     // Actual quantity used

        public decimal UnitPrice { get; set; }                           // Unit price
        public decimal TotalAmount { get; set; }                         // Total amount

        public string? Supplier { get; set; }                            // Supplier
        public DateTime? PlannedDeliveryDate { get; set; }               // Planned delivery date
        public DateTime? ActualDeliveryDate { get; set; }                // Actual delivery date

        public string? Notes { get; set; }
    }
}
