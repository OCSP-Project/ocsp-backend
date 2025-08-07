using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Project : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Budget { get; set; }
        public string Status { get; set; } = string.Empty;

        // Navigation properties
        public Guid SupervisorId { get; set; }
        public User? Supervisor { get; set; }

        public ICollection<Contract>? Contracts { get; set; }
    }
}