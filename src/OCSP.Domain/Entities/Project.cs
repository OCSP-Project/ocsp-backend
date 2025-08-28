using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Project : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal FloorArea { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal Budget { get; set; }
        public decimal? ActualBudget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public string Status { get; set; } = string.Empty;

        // Navigation properties
        public Guid? SupervisorId { get; set; }
        public Supervisor? Supervisor { get; set; }
        public Guid HomeownerId { get; set; }
        public User? Homeowner { get; set; }

        public ICollection<Contract>? Contracts { get; set; }
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}