using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class DiaryWorkItem : AuditableEntity
    {
        // Foreign Keys
        public Guid ConstructionDiaryId { get; set; }
        public ConstructionDiary? ConstructionDiary { get; set; }

        public Guid WorkItemId { get; set; }
        public WorkItem? WorkItem { get; set; }

        // Work Item Details (snapshot at time of diary entry)
        public string WorkItemName { get; set; } = string.Empty;
        public string? ConstructionArea { get; set; }                    // Khu vực thi công

        // Quantities
        public decimal PlannedQuantity { get; set; }                     // KL kế hoạch
        public decimal ConstructedQuantity { get; set; }                 // KL thi công
        public decimal RemainingQuantity { get; set; }                   // Còn lại
        public string Unit { get; set; } = string.Empty;                 // Đơn vị

        // Navigation Properties
        public ICollection<DiaryLabor> LaborEntries { get; set; } = new List<DiaryLabor>();
        public ICollection<DiaryEquipment> EquipmentEntries { get; set; } = new List<DiaryEquipment>();
    }
}
