namespace OCSP.Application.DTOs.Project
{
    public class ProjectResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal FloorArea { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal Budget { get; set; }
        public decimal? ActualBudget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
        public Guid HomeownerId { get; set; }
        public string? HomeownerName { get; set; }
    }
}