using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public enum NotificationType
    {
        // Material Request Notifications
        MaterialRequestUploaded = 1,
        MaterialRequestApproved = 2,
        MaterialRequestRejected = 3,
        MaterialRequestPartiallyApproved = 4,

        // Other notification types can be added here
        ProjectInvitation = 100,
        PaymentRequest = 200,
        ProgressUpdate = 300,
        WorkItemCommentMention = 400,

        // Quote & Proposal Notifications
        QuoteRequestSent = 500,
        ProposalSubmitted = 501,
        ProposalAccepted = 502,
        ProposalRejected = 503,
        ProposalRevisionRequested = 504,
    }

    public class Notification : AuditableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime NotificationDate { get; set; }

        // Notification type and reference
        public NotificationType Type { get; set; }
        public Guid? ReferenceId { get; set; } // ID of MaterialRequest, PaymentRequest, etc.
        public string? ActionUrl { get; set; } // URL to navigate when clicked

        // Link to Project (optional, for project-specific notifications)
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }

        // Navigation properties
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}