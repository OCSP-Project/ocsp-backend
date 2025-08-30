using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Contract : AuditableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;

        // Project
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

      
        public Guid ContractorId { get; set; }
        public Contractor Contractor { get; set; } = null!;

       
        public Guid? HomeownerId { get; set; }
        public User? Homeowner { get; set; }
    }
}
