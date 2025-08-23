using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ProfileDocument : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Foreign key to Profile (not User directly)
        public Guid ProfileId { get; set; }
        public virtual Profile Profile { get; set; } = null!;
        
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public string DocumentType { get; set; } = string.Empty; // Business License, Certificate, Service Area, etc.
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
