namespace OCSP.Application.DTOs.Supervisor
{
    public class SupervisorListDto
    {
        public Guid Id { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
