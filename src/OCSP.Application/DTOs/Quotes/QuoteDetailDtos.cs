namespace OCSP.Application.DTOs.Quotes
{
    public class ProjectSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Address { get; set; } = "";
        public decimal Budget { get; set; }
        public decimal? ActualBudget { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal FloorArea { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
    }

    public class HomeownerSummaryDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class InviteeSummaryDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = "";
        public string? CompanyName { get; set; }  // lấy từ Contractors nếu có
    }

    public class MyProposalSummaryDto
    {
        public Guid? Id { get; set; }
        public string? Status { get; set; }
        public decimal? PriceTotal { get; set; }
        public int? DurationDays { get; set; }
    }

    public class QuoteRequestDetailDto
    {
        public Guid Id { get; set; }
        public string Scope { get; set; } = "";
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ProjectSummaryDto Project { get; set; } = new();
        public HomeownerSummaryDto Homeowner { get; set; } = new();
        public List<InviteeSummaryDto> Invitees { get; set; } = new();
        public MyProposalSummaryDto MyProposal { get; set; } = new();
    }
}
