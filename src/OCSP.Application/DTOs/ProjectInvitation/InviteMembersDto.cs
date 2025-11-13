using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.ProjectInvitation
{
    /// <summary>
    /// DTO để mời nhiều thành viên vào project
    /// </summary>
    public class InviteMembersDto
    {
        /// <summary>
        /// Danh sách email người được mời
        /// </summary>
        public List<string> Emails { get; set; } = new();

        /// <summary>
        /// Vai trò được mời vào project
        /// </summary>
        public ProjectParticipantRole Role { get; set; }

        /// <summary>
        /// Thông điệp tùy chỉnh (optional)
        /// </summary>
        public string? CustomMessage { get; set; }
    }
}
