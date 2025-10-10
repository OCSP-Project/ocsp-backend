using OCSP.Domain.Common;
namespace OCSP.Domain.Entities
{
    public enum ProjectStatus { Draft = 0, Active = 1, Completed = 2, OnHold = 3 }

    public class Project : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public decimal FloorArea { get; set; }
        public int NumberOfFloors { get; set; }

        public decimal Budget { get; set; }                   // dự toán
        public decimal? ActualBudget { get; set; }            // thực chi

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

        // Chủ nhà (bắt buộc)
        public Guid HomeownerId { get; set; }
        public User? Homeowner { get; set; }

        // Supervisor chính (tùy chọn)
        public Guid? SupervisorId { get; set; }
        public Supervisor? Supervisor { get; set; }
        // NEW: Project Documents
        public virtual ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
        // Nav
        public ICollection<ProjectParticipant> Participants { get; set; } = new List<ProjectParticipant>();
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}
