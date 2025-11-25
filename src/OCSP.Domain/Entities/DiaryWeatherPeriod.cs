using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class DiaryWeatherPeriod : AuditableEntity
    {
        // Foreign Key
        public Guid ConstructionDiaryId { get; set; }
        public ConstructionDiary? ConstructionDiary { get; set; }

        // Weather Information
        public string Period { get; set; } = string.Empty;               // "morning", "afternoon", "evening", "night"
        public string Condition { get; set; } = string.Empty;            // "Nắng", "Mưa nhỏ", "Nhiều mây", etc.
        public string? Temperature { get; set; }                         // "25-30", "30"
    }
}
