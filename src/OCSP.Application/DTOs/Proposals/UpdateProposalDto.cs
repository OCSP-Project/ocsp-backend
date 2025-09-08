namespace OCSP.Application.DTOs.Proposals
{
    public class UpdateProposalDto
    {
        public int DurationDays { get; set; }
        public string? TermsSummary { get; set; }
        public List<ProposalItemDto> Items { get; set; } = new();
    }
}


