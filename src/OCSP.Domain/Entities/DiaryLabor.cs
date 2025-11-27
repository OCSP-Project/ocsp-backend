using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class DiaryLabor : AuditableEntity
    {
        // Foreign Key
        public Guid DiaryWorkItemId { get; set; }
        public DiaryWorkItem? DiaryWorkItem { get; set; }

        // Labor Information (snapshot)
        public string LaborName { get; set; } = string.Empty;            // Tên nhân công
        public string? Position { get; set; }                            // Chức vụ (from Labor master data)

        // Work Details
        public string WorkHours { get; set; } = string.Empty;            // "3.5/7" format
        public string Team { get; set; } = string.Empty;                 // "Nhóm 2"
        public string Shift { get; set; } = string.Empty;                // "7h00-17h00"
        public decimal Quantity { get; set; }                            // 2.9
        public string Unit { get; set; } = string.Empty;                 // "Công"

        // Optional: Reference to master data if exists
        public Guid? LaborId { get; set; }                               // Link to Labor master data (future)
    }
}
