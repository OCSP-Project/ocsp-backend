using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class DiaryEquipment : AuditableEntity
    {
        // Foreign Key
        public Guid DiaryWorkItemId { get; set; }
        public DiaryWorkItem? DiaryWorkItem { get; set; }

        // Equipment Information (snapshot)
        public string EquipmentName { get; set; } = string.Empty;        // Tên máy móc
        public string Specifications { get; set; } = string.Empty;       // Thông số kỹ thuật

        // Usage Details
        public decimal HoursUsed { get; set; }                           // Giờ làm việc (0.019951967485682617)
        public decimal Quantity { get; set; }                            // Số lượng
        public string Unit { get; set; } = string.Empty;                 // "ca"

        // Optional: Reference to master data if exists
        public Guid? EquipmentId { get; set; }                           // Link to Equipment master data (future)
    }
}
