using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public enum ConstructionRating
    {
        Good = 0,           // Tốt
        Average = 1,        // Trung bình
        Poor = 2            // Kém
    }

    public class ConstructionDiary : AuditableEntity
    {
        // Basic Information
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public DateTime DiaryDate { get; set; }                          // Diary date
        public string? ConstructionTeam { get; set; }                    // Tổ đội thi công

        // Construction Assessment
        public ConstructionRating SafetyRating { get; set; }             // Công tác an toàn
        public ConstructionRating QualityRating { get; set; }            // Chất lượng thi công
        public ConstructionRating ProgressRating { get; set; }           // Tiến độ thi công
        public ConstructionRating CleanlinessRating { get; set; }        // Công tác vệ sinh

        // Reports
        public string? IncidentReport { get; set; }                      // Báo cáo sự cố
        public string? Recommendations { get; set; }                     // Đề xuất - kiến nghị
        public string? Notes { get; set; }                               // Ghi chú

        // Supervisor Information
        public string? SupervisorName { get; set; }                      // Tên người giám sát
        public string? SupervisorPosition { get; set; }                  // Chức vụ người giám sát

        // Additional Information
        public string? ContractorName { get; set; }                      // Tên nhà thầu
        public string? SupervisorUnitName { get; set; }                  // Tên đơn vị giám sát

        // Navigation Properties
        public ICollection<DiaryWorkItem> WorkItems { get; set; } = new List<DiaryWorkItem>();
        public ICollection<DiaryMaterialEntry> MaterialEntries { get; set; } = new List<DiaryMaterialEntry>();
        public ICollection<DiaryWeatherPeriod> WeatherPeriods { get; set; } = new List<DiaryWeatherPeriod>();
        public ICollection<DiaryImage> Images { get; set; } = new List<DiaryImage>();
    }
}
