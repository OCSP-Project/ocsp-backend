using OCSP.Domain.Common;
using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public enum ProjectRole { Supervisor = 1, Contractor = 2, Homeowner = 3 }
    public enum ParticipantStatus { Invited = 0, Active = 1, Left = 2 }

    public class ProjectParticipant : AuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

        public ProjectRole Role { get; set; }

        // NEW: Vai trò chi tiết (giám sát chính/phụ, nhà thầu chính/phụ)
        public ProjectParticipantRole DetailedRole { get; set; }

        public ParticipantStatus Status { get; set; } = ParticipantStatus.Active;

        public DateTime? JoinedAt { get; set; }
    }
}
