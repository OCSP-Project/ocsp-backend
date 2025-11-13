using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public enum InvitationStatus
    {
        Pending = 0,    // Đang chờ
        Accepted = 1,   // Đã chấp nhận
        Rejected = 2,   // Từ chối
        Expired = 3     // Hết hạn
    }

    public class ProjectInvitation : AuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        // Email người được mời (có thể chưa có account)
        public string InviteeEmail { get; set; } = string.Empty;

        // User ID nếu đã tồn tại trong hệ thống (nullable)
        public Guid? InviteeUserId { get; set; }
        public User? InviteeUser { get; set; }

        // Người gửi lời mời
        public Guid InvitedBy { get; set; }
        public User InvitedByUser { get; set; } = default!;

        // Vai trò được mời vào project
        public ProjectParticipantRole Role { get; set; }

        // Trạng thái lời mời
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        // Token để xác thực khi join
        public string InvitationToken { get; set; } = string.Empty;

        // Thời gian hết hạn (mặc định 7 ngày)
        public DateTime ExpiresAt { get; set; }

        // Thời gian chấp nhận/từ chối
        public DateTime? RespondedAt { get; set; }
    }
}
