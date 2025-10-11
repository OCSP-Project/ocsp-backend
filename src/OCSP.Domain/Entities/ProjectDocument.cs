// OCSP.Domain/Entities/ProjectDocument.cs
using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public class ProjectDocument : AuditableEntity
    {

        // FK to Project

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        // Document type
        public ProjectDocumentType DocumentType { get; set; } // Drawing, Permit

        // File info
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty; // Đường dẫn file đã MÃ HÓA (nếu là Permit)
        public string FileType { get; set; } = string.Empty; // .pdf, .jpg, .png
        public long FileSize { get; set; }

        // Encryption info (CHỈ cho Permit)
        public bool IsEncrypted { get; set; } = false;
        public string? EncryptionKeyId { get; set; } // Reference to encryption key (nếu dùng KMS)
        public string FileHash { get; set; } = string.Empty; // SHA256 để verify integrity

        // Metadata trích xuất (CHỈ cho Permit)
        public string? ExtractedDataJson { get; set; } // JSON chứa data từ OCR

        // Audit
        public Guid UploadedByUserId { get; set; }
        public virtual User UploadedBy { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Version control (optional, cho phép upload lại)
        public int Version { get; set; } = 1;
        public bool IsLatest { get; set; } = true;
    }
}
