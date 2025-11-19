namespace OCSP.Application.DTOs.Material
{
    public class MaterialApprovalHistoryDto
    {
        public Guid Id { get; set; }
        public Guid MaterialRequestId { get; set; }

        public Guid ApprovedById { get; set; }
        public string ApprovedByName { get; set; } = string.Empty;

        public string ApproverRole { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;

        public DateTime ActionDate { get; set; }
        public string? Comments { get; set; }
    }
}
