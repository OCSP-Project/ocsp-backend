// OCSP.Application/DTOs/Project/ProjectDocumentDto.cs
namespace OCSP.Application.DTOs.Project
{
    public class ProjectDocumentDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public int DocumentType { get; set; } // 1=Drawing, 2=Permit
        public string DocumentTypeName { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileSizeFormatted { get; set; } = string.Empty; // "15.2 MB"

        public bool IsEncrypted { get; set; }
        public string FileHash { get; set; } = string.Empty;

        public Guid UploadedByUserId { get; set; }
        public string UploadedByUsername { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }

        public int Version { get; set; }
        public bool IsLatest { get; set; }

        // Nếu là Permit → có metadata
        public PermitMetadataDto? PermitMetadata { get; set; }
    }

    public class PermitMetadataDto
    {
        public Guid Id { get; set; }
        public string PermitNumber { get; set; } = string.Empty;
        public decimal? Area { get; set; }
        public string? Address { get; set; }
        public string? Owner { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public float? OcrConfidence { get; set; }
        public bool IsVerified { get; set; }
        public DateTime ExtractedAt { get; set; }

        // Warnings
        public List<string> Warnings { get; set; } = new();
    }

    public class UploadProjectDocumentDto
    {
        public int DocumentType { get; set; } // 1=Drawing, 2=Permit
        public string? Description { get; set; }
    }
}