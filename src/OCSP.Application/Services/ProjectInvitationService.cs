using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OCSP.Application.DTOs.ProjectInvitation;
using OCSP.Application.Interfaces;
using OCSP.Infrastructure.ExternalServices.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;
using System.Security.Cryptography;

namespace OCSP.Application.Services
{
    public class ProjectInvitationService : IProjectInvitationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ProjectInvitationService(
            ApplicationDbContext context,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<InvitationResponseDto> InviteMembersAsync(
            Guid projectId,
            Guid invitedBy,
            InviteMembersDto dto,
            CancellationToken ct = default)
        {
            var response = new InvitationResponseDto();

            // Kiểm tra project tồn tại
            var project = await _context.Projects
                .Include(p => p.Homeowner)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);

            if (project == null)
            {
                response.Success = false;
                response.Message = "Không tìm thấy dự án";
                return response;
            }

            // Lấy thông tin người mời
            var inviter = await _context.Users.FindAsync(new object[] { invitedBy }, ct);
            if (inviter == null)
            {
                response.Success = false;
                response.Message = "Không tìm thấy người mời";
                return response;
            }

            // Xác định role của người mời trong project
            var inviterRole = await GetUserRoleInProject(projectId, invitedBy, ct);

            // Kiểm tra quyền mời
            if (!CanInviteRole(inviterRole, dto.Role))
            {
                response.Success = false;
                response.Message = "Bạn không có quyền mời vai trò này";
                return response;
            }

            // Xử lý từng email
            foreach (var email in dto.Emails.Distinct())
            {
                try
                {
                    // Kiểm tra email đã tồn tại trong project chưa
                    var existingParticipant = await _context.ProjectParticipants
                        .Include(pp => pp.User)
                        .Where(pp => pp.ProjectId == projectId && pp.User.Email == email)
                        .FirstOrDefaultAsync(ct);

                    if (existingParticipant != null)
                    {
                        response.FailedInvites.Add($"{email} - Đã là thành viên của dự án");
                        continue;
                    }

                    // Kiểm tra đã có lời mời pending chưa
                    var existingInvitation = await _context.ProjectInvitations
                        .Where(i => i.ProjectId == projectId &&
                                   i.InviteeEmail == email &&
                                   i.Status == InvitationStatus.Pending &&
                                   i.ExpiresAt > DateTime.UtcNow)
                        .FirstOrDefaultAsync(ct);

                    if (existingInvitation != null)
                    {
                        response.FailedInvites.Add($"{email} - Đã có lời mời đang chờ");
                        continue;
                    }

                    // Tìm user nếu email đã tồn tại trong hệ thống
                    var inviteeUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == email, ct);

                    // Tạo invitation token
                    var token = GenerateInvitationToken();

                    // Tạo invitation
                    var invitation = new ProjectInvitation
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projectId,
                        InviteeEmail = email,
                        InviteeUserId = inviteeUser?.Id,
                        InvitedBy = invitedBy,
                        Role = dto.Role,
                        Status = InvitationStatus.Pending,
                        InvitationToken = token,
                        ExpiresAt = DateTime.UtcNow.AddDays(7), // Hết hạn sau 7 ngày
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.ProjectInvitations.Add(invitation);
                    await _context.SaveChangesAsync(ct);

                    // Tạo invitation link
                    var frontendUrl = _configuration["Frontend:Url"] ?? "https://ocsp-tech-fe.vercel.app";
                    var invitationLink = $"{frontendUrl}/invitations/{token}";

                    // Gửi email
                    var roleName = GetRoleName(dto.Role);
                    await _emailService.SendInvitationEmailAsync(
                        email,
                        inviter.Username,
                        project.Name,
                        invitationLink,
                        roleName,
                        dto.CustomMessage,
                        ct);

                    response.SuccessfulInvites.Add(email);

                    // Map to DTO
                    var invitationDto = MapToDto(invitation, project.Name, inviter.Username, inviter.Email);
                    response.Invitations.Add(invitationDto);
                }
                catch (Exception ex)
                {
                    response.FailedInvites.Add($"{email} - Lỗi: {ex.Message}");
                }
            }

            response.Success = response.SuccessfulInvites.Count > 0;
            response.Message = response.Success
                ? $"Đã gửi {response.SuccessfulInvites.Count} lời mời thành công"
                : "Không thể gửi lời mời";

            return response;
        }

        public async Task<List<ProjectInvitationDto>> GetProjectInvitationsAsync(
            Guid projectId,
            CancellationToken ct = default)
        {
            var invitations = await _context.ProjectInvitations
                .Include(i => i.Project)
                .Include(i => i.InvitedByUser)
                .Where(i => i.ProjectId == projectId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(ct);

            return invitations.Select(i => MapToDto(
                i,
                i.Project.Name,
                i.InvitedByUser.Username,
                i.InvitedByUser.Email)).ToList();
        }

        public async Task<ProjectInvitationDto?> GetInvitationByTokenAsync(
            string token,
            CancellationToken ct = default)
        {
            var invitation = await _context.ProjectInvitations
                .Include(i => i.Project)
                .Include(i => i.InvitedByUser)
                .FirstOrDefaultAsync(i => i.InvitationToken == token, ct);

            if (invitation == null)
                return null;

            return MapToDto(
                invitation,
                invitation.Project.Name,
                invitation.InvitedByUser.Username,
                invitation.InvitedByUser.Email);
        }

        public async Task<bool> RespondToInvitationAsync(
            string token,
            Guid userId,
            bool accept,
            CancellationToken ct = default)
        {
            var invitation = await _context.ProjectInvitations
                .Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.InvitationToken == token, ct);

            if (invitation == null)
                return false;

            // Kiểm tra hết hạn
            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                await _context.SaveChangesAsync(ct);
                return false;
            }

            // Kiểm tra status
            if (invitation.Status != InvitationStatus.Pending)
                return false;

            // Lấy user
            var user = await _context.Users.FindAsync(new object[] { userId }, ct);
            if (user == null)
                return false;

            // Kiểm tra email khớp
            if (user.Email != invitation.InviteeEmail)
                return false;

            invitation.RespondedAt = DateTime.UtcNow;
            invitation.InviteeUserId = userId;

            if (accept)
            {
                invitation.Status = InvitationStatus.Accepted;

                // Tạo ProjectParticipant
                var participant = new ProjectParticipant
                {
                    Id = Guid.NewGuid(),
                    ProjectId = invitation.ProjectId,
                    UserId = userId,
                    Role = GetProjectRole(invitation.Role),
                    DetailedRole = invitation.Role,
                    Status = ParticipantStatus.Active,
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ProjectParticipants.Add(participant);
            }
            else
            {
                invitation.Status = InvitationStatus.Rejected;
            }

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public bool CanInviteRole(ProjectParticipantRole inviterRole, ProjectParticipantRole targetRole)
        {
            return inviterRole switch
            {
                // Homeowner có thể mời tất cả
                ProjectParticipantRole.Homeowner => true,

                // Nhà thầu chính chỉ mời được nhà thầu phụ
                ProjectParticipantRole.MainContractor => targetRole == ProjectParticipantRole.SubContractor,

                // Giám sát chính chỉ mời được giám sát phụ
                ProjectParticipantRole.MainSupervisor => targetRole == ProjectParticipantRole.SubSupervisor,

                // Các role khác không được mời ai
                _ => false
            };
        }

        public async Task<bool> CancelInvitationAsync(
            Guid invitationId,
            Guid userId,
            CancellationToken ct = default)
        {
            var invitation = await _context.ProjectInvitations
                .FirstOrDefaultAsync(i => i.Id == invitationId, ct);

            if (invitation == null)
                return false;

            // Chỉ người gửi hoặc homeowner mới được hủy
            var userRole = await GetUserRoleInProject(invitation.ProjectId, userId, ct);
            if (invitation.InvitedBy != userId && userRole != ProjectParticipantRole.Homeowner)
                return false;

            if (invitation.Status != InvitationStatus.Pending)
                return false;

            invitation.Status = InvitationStatus.Expired;
            invitation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        #region Private Helper Methods

        private async Task<ProjectParticipantRole> GetUserRoleInProject(
            Guid projectId,
            Guid userId,
            CancellationToken ct)
        {
            // Check if user is homeowner
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);

            if (project?.HomeownerId == userId)
                return ProjectParticipantRole.Homeowner;

            // Check participant role
            var participant = await _context.ProjectParticipants
                .FirstOrDefaultAsync(pp => pp.ProjectId == projectId && pp.UserId == userId, ct);

            return participant?.DetailedRole ?? ProjectParticipantRole.SubContractor; // Default to lowest permission
        }

        private static string GenerateInvitationToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private static string GetRoleName(ProjectParticipantRole role)
        {
            return role switch
            {
                ProjectParticipantRole.MainSupervisor => "Giám sát chính",
                ProjectParticipantRole.SubSupervisor => "Giám sát phụ",
                ProjectParticipantRole.MainContractor => "Nhà thầu chính",
                ProjectParticipantRole.SubContractor => "Nhà thầu phụ",
                ProjectParticipantRole.Homeowner => "Chủ nhà",
                _ => "Unknown"
            };
        }

        private static string GetStatusName(InvitationStatus status)
        {
            return status switch
            {
                InvitationStatus.Pending => "Đang chờ",
                InvitationStatus.Accepted => "Đã chấp nhận",
                InvitationStatus.Rejected => "Đã từ chối",
                InvitationStatus.Expired => "Hết hạn",
                _ => "Unknown"
            };
        }

        private static ProjectRole GetProjectRole(ProjectParticipantRole detailedRole)
        {
            return detailedRole switch
            {
                ProjectParticipantRole.MainSupervisor or ProjectParticipantRole.SubSupervisor
                    => ProjectRole.Supervisor,
                ProjectParticipantRole.MainContractor or ProjectParticipantRole.SubContractor
                    => ProjectRole.Contractor,
                ProjectParticipantRole.Homeowner
                    => ProjectRole.Homeowner,
                _ => ProjectRole.Contractor
            };
        }

        private static ProjectInvitationDto MapToDto(
            ProjectInvitation invitation,
            string projectName,
            string invitedByName,
            string invitedByEmail)
        {
            return new ProjectInvitationDto
            {
                Id = invitation.Id,
                ProjectId = invitation.ProjectId,
                ProjectName = projectName,
                InviteeEmail = invitation.InviteeEmail,
                InvitedByName = invitedByName,
                InvitedByEmail = invitedByEmail,
                Role = invitation.Role,
                RoleName = GetRoleName(invitation.Role),
                Status = invitation.Status,
                StatusName = GetStatusName(invitation.Status),
                CreatedAt = invitation.CreatedAt,
                ExpiresAt = invitation.ExpiresAt,
                RespondedAt = invitation.RespondedAt
            };
        }

        #endregion
    }
}
