using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public enum WorkItemDocumentType
    {
        Drawing = 1,            // Construction drawing
        Photo = 2,              // Site photo
        Video = 3,              // Video
        Report = 4,             // Progress report
        Certificate = 5,        // Acceptance certificate
        PaymentDocument = 6,    // Payment document
        Other = 99              // Other
    }

    public class WorkItemDocument : AuditableEntity
    {
        public Guid WorkItemId { get; set; }
        public WorkItem? WorkItem { get; set; }

        public WorkItemDocumentType DocumentType { get; set; }
        public string FileName { get; set; } = string.Empty;             // File name
        public string FilePath { get; set; } = string.Empty;             // URL/Path
        public string? Description { get; set; }                         // Description

        public string FileType { get; set; } = string.Empty;             // MIME type
        public long FileSize { get; set; }                               // Bytes

        public Guid UploadedById { get; set; }
        public User? UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int? Version { get; set; }                                // Version
        public bool IsLatest { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
