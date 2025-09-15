namespace OCSP.Application.DTOs.Project
{
    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;

        public decimal FloorArea { get; set; }
        public int NumberOfFloors { get; set; }

        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }

        // Supervisor chỉ định (optional)
        // public Guid? SupervisorId { get; set; }
        
        // Contractor chỉ định (optional)
        public Guid? ContractorId { get; set; }
    }
}
