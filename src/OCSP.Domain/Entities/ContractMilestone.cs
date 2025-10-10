using OCSP.Domain.Common;
using OCSP.Domain.Enums;
// Alias to disambiguate from OCSP.Domain.Entities.MilestoneStatus defined in ProjectTimeline.cs
using MilestoneStatusEnum = OCSP.Domain.Enums.MilestoneStatus;

namespace OCSP.Domain.Entities
{
    public class ContractMilestone : AuditableEntity
    {
        public Guid ContractId { get; set; }
        public Contract Contract { get; set; } = default!;

        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Note { get; set; }
        public MilestoneStatusEnum Status { get; set; } = MilestoneStatusEnum.Planned;
    }
}
