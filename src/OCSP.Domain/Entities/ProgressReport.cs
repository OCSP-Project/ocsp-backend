using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ProgressReport : AuditableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PercentageComplete { get; set; }
        public DateTime ReportDate { get; set; }

        // Navigation properties
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public Guid ContractorId { get; set; }
        public User? Contractor { get; set; }
    }
}