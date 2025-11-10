using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Admin
{
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<UserProjectInfoDto> Projects { get; set; } = new();
    }

    public class UserProjectInfoDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectStatus { get; set; } = string.Empty;
        public string ParticipationRole { get; set; } = string.Empty; // Homeowner, Supervisor, Contractor
        public DateTime? JoinedAt { get; set; }
    }
}
