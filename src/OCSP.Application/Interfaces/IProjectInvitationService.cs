using OCSP.Application.DTOs.ProjectInvitation;
using OCSP.Domain.Enums;

namespace OCSP.Application.Interfaces
{
    public interface IProjectInvitationService
    {
        /// <summary>
        /// Mời nhiều thành viên vào project
        /// </summary>
        Task<InvitationResponseDto> InviteMembersAsync(
            Guid projectId,
            Guid invitedBy,
            InviteMembersDto dto,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy danh sách lời mời của một project
        /// </summary>
        Task<List<ProjectInvitationDto>> GetProjectInvitationsAsync(
            Guid projectId,
            CancellationToken ct = default);

        /// <summary>
        /// Lấy thông tin invitation bằng token
        /// </summary>
        Task<ProjectInvitationDto?> GetInvitationByTokenAsync(
            string token,
            CancellationToken ct = default);

        /// <summary>
        /// Chấp nhận hoặc từ chối lời mời
        /// </summary>
        Task<bool> RespondToInvitationAsync(
            string token,
            Guid userId,
            bool accept,
            CancellationToken ct = default);

        /// <summary>
        /// Kiểm tra xem user có quyền mời role này không
        /// </summary>
        bool CanInviteRole(ProjectParticipantRole inviterRole, ProjectParticipantRole targetRole);

        /// <summary>
        /// Hủy lời mời
        /// </summary>
        Task<bool> CancelInvitationAsync(
            Guid invitationId,
            Guid userId,
            CancellationToken ct = default);
    }
}
