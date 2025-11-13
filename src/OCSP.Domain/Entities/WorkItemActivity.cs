using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public enum ActivityType
    {
        Created = 1,
        ProgressUpdated = 2,
        StatusChanged = 3,
        AssigneeAdded = 4,
        AssigneeRemoved = 5,
        DocumentAdded = 6,
        CommentAdded = 7,
        InfoChanged = 8,
        Started = 9,
        Completed = 10
    }

    public class WorkItemActivity : AuditableEntity
    {
        public Guid WorkItemId { get; set; }
        public WorkItem? WorkItem { get; set; }

        public ActivityType Type { get; set; }
        public string Description { get; set; } = string.Empty;          // Activity description
        public string? OldValue { get; set; }                            // JSON - old value
        public string? NewValue { get; set; }                            // JSON - new value

        public Guid PerformedById { get; set; }
        public User? PerformedBy { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }
}
