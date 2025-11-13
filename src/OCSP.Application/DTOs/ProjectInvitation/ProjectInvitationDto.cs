using OCSP.Domain.Enums;
using OCSP.Domain.Entities;

namespace OCSP.Application.DTOs.ProjectInvitation
{
    /// <summary>
    /// DTO cho lời mời vào project
    /// </summary>
    public class ProjectInvitationDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string InviteeEmail { get; set; } = string.Empty;
        public string InvitedByName { get; set; } = string.Empty;
        public string InvitedByEmail { get; set; } = string.Empty;
        public ProjectParticipantRole Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public InvitationStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt && Status == InvitationStatus.Pending;
    }

    /// <summary>
    /// Response sau khi gửi invitation
    /// </summary>
    public class InvitationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> SuccessfulInvites { get; set; } = new();
        public List<string> FailedInvites { get; set; } = new();
        public List<ProjectInvitationDto> Invitations { get; set; } = new();
    }

    /// <summary>
    /// DTO để accept/reject invitation
    /// </summary>
    public class RespondToInvitationDto
    {
        public bool Accept { get; set; }
    }
}
