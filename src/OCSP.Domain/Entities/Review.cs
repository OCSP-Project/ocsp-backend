using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Review : AuditableEntity
    {
        public Guid ReviewerId { get; set; }
        public Guid ContractorId { get; set; }
        public Guid ProjectId { get; set; }
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
        
        // Navigation properties
        public User Reviewer { get; set; } = null!;
        public Contractor Contractor { get; set; } = null!;
        public Project Project { get; set; } = null!;
    }
}