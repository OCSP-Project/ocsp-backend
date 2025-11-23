using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public enum ApprovalAction
    {
        Approved = 1,
        Rejected = 2
    }

    public enum ApproverRole
    {
        Homeowner = 1,
        Supervisor = 2
    }

    public class MaterialApprovalHistory : AuditableEntity
    {
        public Guid MaterialRequestId { get; set; }
        public MaterialRequest? MaterialRequest { get; set; }

        public Guid ApprovedById { get; set; }              // User ID người phê duyệt
        public User? ApprovedBy { get; set; }

        public ApproverRole ApproverRole { get; set; }      // Vai trò người duyệt
        public ApprovalAction Action { get; set; }          // Approved/Rejected

        public DateTime ActionDate { get; set; }
        public string? Comments { get; set; }               // Nhận xét khi duyệt/từ chối
    }
}
