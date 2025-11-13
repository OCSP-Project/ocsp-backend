using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class BudgetDetail : AuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public Guid? WorkItemId { get; set; }                            // Link to work item
        public WorkItem? WorkItem { get; set; }

        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
