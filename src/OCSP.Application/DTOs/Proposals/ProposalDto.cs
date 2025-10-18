using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Proposals
{
    public class ProposalDto
    {
        public Guid Id { get; set; }
        public Guid QuoteRequestId { get; set; }
        public Guid ContractorUserId { get; set; }
        public string Status { get; set; } = ProposalStatus.Draft.ToString();
        public decimal PriceTotal { get; set; }
        public int DurationDays { get; set; }
        public string? TermsSummary { get; set; }
        public List<ProposalItemDto> Items { get; set; } = new();
        public ProposalContractorSummaryDto? Contractor { get; set; }
        
        // Excel-based proposal info
        public bool IsFromExcel { get; set; } = false;
        public string? ExcelFileName { get; set; }
        public string? ExcelFileUrl { get; set; }
        
        // Project Information from Excel
        public string? ProjectTitle { get; set; }
        public string? ConstructionArea { get; set; }
        public string? ConstructionTime { get; set; }
        public string? NumberOfWorkers { get; set; }
        public string? AverageSalary { get; set; }
        
        // Resubmission tracking
        public bool HasBeenSubmitted { get; set; } = false;
    }
}
