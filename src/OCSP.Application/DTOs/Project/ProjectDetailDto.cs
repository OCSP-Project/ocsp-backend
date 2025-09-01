namespace OCSP.Application.DTOs.Project
{
    public class ProjectDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal FloorArea { get; set; }
        public int NumberOfFloors { get; set; }
        public decimal Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public string Status { get; set; } = default!; // map từ enum

        public Guid HomeownerId { get; set; }
        public Guid? SupervisorId { get; set; }

        public List<ProjectParticipantDto> Participants { get; set; } = new();
    }

    public class ProjectParticipantDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string Status { get; set; } = default!;
    }
}
