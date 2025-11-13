using Microsoft.AspNetCore.Http;

namespace OCSP.Application.DTOs.Budget
{
    public class PaymentRequestDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;

        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? PaidDate { get; set; }

        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;

        public Guid RequestedById { get; set; }
        public string RequestedByName { get; set; } = string.Empty;

        public Guid? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }

        public string? RejectionReason { get; set; }

        public List<string> RelatedWorkItemIds { get; set; } = new();
        public List<string> SupportingDocuments { get; set; } = new();
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreatePaymentRequestDto
    {
        public Guid ProjectId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<Guid>? RelatedWorkItemIds { get; set; }
        public List<IFormFile>? SupportingDocuments { get; set; }
        public string? Notes { get; set; }
    }

    public class ApprovePaymentRequestDto
    {
        public string? Notes { get; set; }
    }

    public class RejectPaymentRequestDto
    {
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class PaymentStatisticsDto
    {
        public int TotalRequests { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int PaidCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
    }
}
