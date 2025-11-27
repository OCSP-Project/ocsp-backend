using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Material : AuditableEntity
    {
        public Guid MaterialRequestId { get; set; }
        public MaterialRequest? MaterialRequest { get; set; }

        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public Guid? WorkItemId { get; set; }               // Liên kết với công việc (optional)
        public WorkItem? WorkItem { get; set; }

        // Basic info
        public string Code { get; set; } = string.Empty;    // Mã số
        public string Name { get; set; } = string.Empty;    // Hạng mục
        public string Unit { get; set; } = string.Empty;    // Đơn vị (m², m³, kg...)
        public decimal UnitPrice { get; set; }              // Đơn giá

        // Quantities
        public decimal? ContractQuantity { get; set; }      // Khối lượng theo hợp đồng
        public decimal EstimatedQuantity { get; set; }      // Khối lượng theo NKTC (Nhà thầu)
        public decimal? ActualQuantity { get; set; }        // Khối lượng nghiệm thu (Giám sát)

        // Amounts (calculated)
        public decimal? ContractAmount { get; set; }        // Thành tiền theo hợp đồng
        public decimal EstimatedAmount { get; set; }        // Thành tiền theo NKTC
        public decimal? ActualAmount { get; set; }          // Thành tiền nghiệm thu

        // Additional info
        public string? Description { get; set; }
        public string? Supplier { get; set; }               // Nhà cung cấp
        public string? Notes { get; set; }

        public int SortOrder { get; set; }

        // Navigation
        public ICollection<MaterialPayment> Payments { get; set; } = new List<MaterialPayment>();
    }
}
