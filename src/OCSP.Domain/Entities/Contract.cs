// OCSP.Domain/Entities/Contract.cs
using OCSP.Domain.Common;
using OCSP.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCSP.Domain.Entities
{
    public class Contract : AuditableEntity
    {
        [ForeignKey("ProjectId")]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        public Guid QuoteRequestId { get; set; }
        public Guid ProposalId { get; set; }

        public Guid HomeownerUserId { get; set; }
        public Guid ContractorUserId { get; set; }

        public decimal TotalPrice { get; set; }
        public int DurationDays { get; set; }

        public string Terms { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        public DateTime? SignedByHomeownerAt { get; set; }
        public DateTime? SignedByContractorAt { get; set; }

        public ICollection<ContractItem> Items { get; set; } = new List<ContractItem>();
    }


}
