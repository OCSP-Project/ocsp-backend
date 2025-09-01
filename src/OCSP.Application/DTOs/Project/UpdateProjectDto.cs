namespace OCSP.Application.DTOs.Project
{ 
    public class UpdateProjectDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public decimal? FloorArea { get; set; }
        public int? NumberOfFloors { get; set; }
        public decimal? Budget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public string? Status { get; set; }
    }
}