// OCSP.Domain/Entities/PermitMetadata.cs
using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class PermitMetadata : AuditableEntity
    {

        // FK to ProjectDocument (1-1 relationship)
        public Guid ProjectDocumentId { get; set; }
        public virtual ProjectDocument ProjectDocument { get; set; } = null!;

        // Dữ liệu trích xuất từ OCR
        public string PermitNumber { get; set; } = string.Empty;
        public decimal? Area { get; set; }
        public string? Address { get; set; }
        public string? Owner { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // OCR confidence (0.0 - 1.0)
        public float? OcrConfidence { get; set; }

        // Verification status
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }

        public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
    }
}
