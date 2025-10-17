namespace OCSP.Application.DTOs.Proposals
{
    public class ProposalItemDto
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string? Notes { get; set; } // For percentage information
    }
}
