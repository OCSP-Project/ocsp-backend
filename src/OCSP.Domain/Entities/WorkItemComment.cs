using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class WorkItemComment : AuditableEntity
    {
        public Guid WorkItemId { get; set; }
        public WorkItem? WorkItem { get; set; }

        public Guid CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        public string Content { get; set; } = string.Empty;              // Comment content
        public Guid? ParentCommentId { get; set; }                       // Reply to comment
        public WorkItemComment? ParentComment { get; set; }

        public ICollection<WorkItemComment> Replies { get; set; } = new List<WorkItemComment>();

        public string? Attachments { get; set; }                         // JSON array of file URLs
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
