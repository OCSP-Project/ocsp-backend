using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public enum ImageCategory
    {
        Construction = 0,   // Thi công
        Incident = 1,       // Sự cố
        Material = 2        // Vật liệu
    }

    public class DiaryImage : AuditableEntity
    {
        // Foreign Key
        public Guid ConstructionDiaryId { get; set; }
        public ConstructionDiary? ConstructionDiary { get; set; }

        // Image Information
        public string Url { get; set; } = string.Empty;                  // S3 URL or base64 (temporary)
        public ImageCategory Category { get; set; }                      // Construction, Incident, Material
        public string? Description { get; set; }                         // Caption/description
        public DateTime UploadedAt { get; set; }                         // Upload timestamp
    }
}
