namespace OCSP.Application.DTOs.Admin
{
    public class RecentProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HomeownerName { get; set; } = string.Empty;
        public string? ContractorName { get; set; }
        public decimal Budget { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

