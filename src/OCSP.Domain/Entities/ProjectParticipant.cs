using OCSP.Domain.Common;
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
        public ParticipantStatus Status { get; set; } = ParticipantStatus.Active;

        public DateTime? JoinedAt { get; set; }
    }
}
