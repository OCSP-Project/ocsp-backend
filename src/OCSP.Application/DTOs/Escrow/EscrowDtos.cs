// OCSP.Application/DTOs/Escrow/EscrowDtos.cs
using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Escrow
{
    public class SetupEscrowDto
    {
        public Guid ContractId { get; set; }
        public PaymentProvider Provider { get; set; } = PaymentProvider.VnPay; // enum int
        public decimal Amount { get; set; } = 0m;         // nạp lần đầu (optional)
        public string? ExternalAccountId { get; set; }    // nếu có
    }

    public class EscrowAccountDto
    {
        public Guid Id { get; set; }
        public Guid ContractId { get; set; }
        public PaymentProvider Provider { get; set; }
        public string Status { get; set; } = string.Empty; // EscrowStatus
        public decimal Balance { get; set; }
        public string? ExternalAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FundMilestoneDto
    {
        public Guid MilestoneId { get; set; }
        public decimal Amount { get; set; } // <= Milestone.Amount
    }

    public class ReleaseMilestoneDto
    {
        public Guid MilestoneId { get; set; }
        public decimal? CommissionRate { get; set; } // 0.10m mặc định
    }

    public class MilestonePayoutResultDto
    {
        public Guid MilestoneId { get; set; }
        public decimal Gross { get; set; }       // tổng giải ngân = amount mốc
        public decimal Commission { get; set; }  // hoa hồng nền tảng
        public decimal Net { get; set; }         // tiền về contractor
        public decimal EscrowBalanceAfter { get; set; }
        public string Status { get; set; } = string.Empty; // MilestoneStatus sau khi xử lý
    }
}
