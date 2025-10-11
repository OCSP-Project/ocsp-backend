using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Milestones
{
    // DTO dùng khi homeowner tạo 1 milestone đơn lẻ
    public class CreateMilestoneDto
    {
        public string Name { get; set; } = string.Empty;   // Tên mốc (bắt buộc)
        public decimal Amount { get; set; }                // Số tiền của mốc (bắt buộc > 0)
        public DateTime? DueDate { get; set; }             // Hạn hoàn thành mong muốn
        public string? Note { get; set; }                  // Ghi chú chi tiết (<=500 ký tự)
    }

    // DTO trả về khi đọc milestone
    public class MilestoneDto
    {
        public Guid Id { get; set; }
        public Guid ContractId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = string.Empty; // MilestoneStatus enum → string
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    
    // DTO contractor submit milestone đã hoàn thành (có thể kèm ghi chú/ảnh biên bản)
    public class SubmitMilestoneDto
    {
        public Guid MilestoneId { get; set; }
        public string? Note { get; set; }
    }

    // DTO dùng để cập nhật milestone (homeowner)
    public class UpdateMilestoneDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Note { get; set; }
    }
}
