// OCSP.Application/DTOs/Milestones/BulkCreateMilestonesDto.cs
namespace OCSP.Application.DTOs.Milestones
{
    // DTO dùng khi homeowner muốn tạo nhiều milestone 1 lần
    public class BulkCreateMilestonesDto
    {
        public Guid ContractId { get; set; }
        public List<CreateMilestoneDto> Milestones { get; set; } = new();
    }

    
}
