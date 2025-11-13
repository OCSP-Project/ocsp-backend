using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public class PaymentRequest : AuditableEntity
    {
        public string Code { get; set; } = string.Empty;                 // Unique code

        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? PaidDate { get; set; }

        public decimal Amount { get; set; }                              // Amount
        public string Description { get; set; } = string.Empty;          // Payment description

        public PaymentRequestStatus Status { get; set; } = PaymentRequestStatus.Pending;

        public Guid RequestedById { get; set; }                          // Requester
        public User? RequestedBy { get; set; }

        public Guid? ApprovedById { get; set; }                          // Approver
        public User? ApprovedBy { get; set; }

        public string? RejectionReason { get; set; }                     // Rejection reason

        // Related work items
        public string? RelatedWorkItemIds { get; set; }                  // JSON array of work item IDs

        // Supporting documents
        public string? SupportingDocuments { get; set; }                 // JSON array of document URLs
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
