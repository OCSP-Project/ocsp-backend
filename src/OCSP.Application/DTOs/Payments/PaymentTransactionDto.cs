// OCSP.Application/DTOs/Payments/PaymentTransactionDto.cs
using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Payments
{
    public class PaymentTransactionDto
    {
        public Guid Id { get; set; }
        public Guid ContractId { get; set; }
        public Guid? MilestoneId { get; set; }
        public PaymentType Type { get; set; }        // Fund / Commission / Release
        public PaymentStatus Status { get; set; }    // Pending / Succeeded / Failed
        public PaymentProvider Provider { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
