using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ContractorPost : AuditableEntity
    {
        public Guid ContractorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Contractor Contractor { get; set; } = null!;
        public ICollection<ContractorPostImage> Images { get; set; } = new List<ContractorPostImage>();
    }

    public class ContractorPostImage : BaseEntity
    {
        public Guid ContractorPostId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? Caption { get; set; }

        public ContractorPost ContractorPost { get; set; } = null!;
    }
}


