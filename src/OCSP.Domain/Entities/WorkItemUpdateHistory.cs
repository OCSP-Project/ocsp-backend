using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class WorkItemUpdateHistory : AuditableEntity
    {
        public Guid WorkItemId { get; set; }
        public WorkItem? WorkItem { get; set; }

        public string FieldName { get; set; } = string.Empty;            // Field name
        public string? OldValue { get; set; }                            // Old value (JSON)
        public string? NewValue { get; set; }                            // New value (JSON)

        public Guid UpdatedById { get; set; }
        public User? UpdatedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }                              // Reason for change
    }
}
