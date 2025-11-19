namespace OCSP.Application.DTOs.Material
{
    public class MaterialRequestDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        public Guid ContractorId { get; set; }
        public string ContractorName { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;

        // Approval status
        public bool ApprovedByHomeowner { get; set; }
        public string? HomeownerName { get; set; }
        public DateTime? ApprovedByHomeownerAt { get; set; }

        public bool ApprovedBySupervisor { get; set; }
        public string? SupervisorName { get; set; }
        public DateTime? ApprovedBySupervisorAt { get; set; }

        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }

        // File info
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }

        public int MaterialCount { get; set; }
        public decimal TotalEstimatedAmount { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class MaterialRequestDetailDto : MaterialRequestDto
    {
        public List<MaterialDto> Materials { get; set; } = new();
        public List<MaterialApprovalHistoryDto> ApprovalHistories { get; set; } = new();
    }

    public class CreateMaterialRequestDto
    {
        public Guid ProjectId { get; set; }
        public string? Notes { get; set; }
    }

    public class ApproveMaterialRequestDto
    {
        public string? Comments { get; set; }
    }

    public class RejectMaterialRequestDto
    {
        public string Reason { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }
}
