namespace OCSP.Application.DTOs.Quotes
{
    public class QuoteRequestDetailDto
{
    public Guid Id { get; set; }
    public string Scope { get; set; } = "";
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // từ bản cũ
    public string ProjectName { get; set; } = string.Empty;
    public Guid HomeownerId { get; set; }

    // từ bản mới
    public ProjectSummaryDto Project { get; set; } = new();
    public HomeownerSummaryDto Homeowner { get; set; } = new();
    public List<InviteeSummaryDto> Invitees { get; set; } = new();
    public MyProposalSummaryDto MyProposal { get; set; } = new();
}

}
