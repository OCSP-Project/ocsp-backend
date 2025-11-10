namespace OCSP.Application.DTOs.Admin
{
    public class AdminProjectListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public string HomeownerName { get; set; } = string.Empty;
        public string? ContractorName { get; set; }
        public string? SupervisorName { get; set; }
        public decimal Budget { get; set; }
        public decimal? ActualBudget { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

