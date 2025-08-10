namespace OCSP.Application.DTOs.Supervisor
{
    public class ProjectItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class SupervisorDetailDto : SupervisorListDto
    {
        public List<ProjectItemDto> Projects { get; set; } = new();
    }
}