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
        public ContractorSummaryDto? Contractor { get; set; }
    }
}
