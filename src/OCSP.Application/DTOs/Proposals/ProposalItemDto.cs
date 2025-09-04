namespace OCSP.Application.DTOs.Proposals
{
    public class ProposalItemDto
    {
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "gói";
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
