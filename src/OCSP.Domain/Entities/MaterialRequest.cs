using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public enum MaterialRequestStatus
    {
        Pending = 0,            // Chờ phê duyệt
        PartiallyApproved = 1,  // Một trong hai đã duyệt
        Approved = 2,           // Cả hai đã duyệt
        Rejected = 3            // Bị từ chối
    }

    public class MaterialRequest : AuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public Guid ContractorId { get; set; }          // Nhà thầu tạo request
        public User? Contractor { get; set; }

        public DateTime RequestDate { get; set; }
        public MaterialRequestStatus Status { get; set; } = MaterialRequestStatus.Pending;

        // Homeowner approval
        public bool ApprovedByHomeowner { get; set; } = false;
        public Guid? ApprovedByHomeownerId { get; set; }
        public DateTime? ApprovedByHomeownerAt { get; set; }

        // Supervisor approval
        public bool ApprovedBySupervisor { get; set; } = false;
        public Guid? ApprovedBySupervisorId { get; set; }
        public DateTime? ApprovedBySupervisorAt { get; set; }

        // Rejection tracking
        public Guid? RejectedById { get; set; }
        public DateTime? RejectedAt { get; set; }

        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }

        // File info
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }

        // Navigation
        public ICollection<Material> Materials { get; set; } = new List<Material>();
        public ICollection<MaterialApprovalHistory> ApprovalHistories { get; set; } = new List<MaterialApprovalHistory>();
    }
}
