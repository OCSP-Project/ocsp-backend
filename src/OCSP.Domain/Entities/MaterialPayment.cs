using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class MaterialPayment : AuditableEntity
    {
        public Guid MaterialId { get; set; }
        public Material? Material { get; set; }

        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public DateTime PaymentDate { get; set; }

        // Payment details
        public decimal PaidQuantity { get; set; }           // Khối lượng thanh toán lần này
        public decimal PaidAmount { get; set; }             // Thành tiền thanh toán lần này

        public decimal RemainingQuantity { get; set; }      // Khối lượng còn lại sau thanh toán
        public decimal RemainingAmount { get; set; }        // Thành tiền còn lại sau thanh toán

        // Payment info
        public string? PaymentMethod { get; set; }          // Phương thức thanh toán
        public string? InvoiceNumber { get; set; }          // Số hóa đơn
        public string? TransactionReference { get; set; }   // Mã giao dịch

        public string? Notes { get; set; }
        public string? ApprovedBy { get; set; }             // Người phê duyệt thanh toán
        public DateTime? ApprovedAt { get; set; }
    }
}
