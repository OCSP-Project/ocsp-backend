using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.ProjectInvitation;
using OCSP.Application.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/invitations")]
    [Authorize]
    public class ProjectInvitationController : ControllerBase
    {
        private readonly IProjectInvitationService _invitationService;
        private readonly ILogger<ProjectInvitationController> _logger;

        public ProjectInvitationController(
            IProjectInvitationService invitationService,
            ILogger<ProjectInvitationController> logger)
        {
            _invitationService = invitationService;
            _logger = logger;
        }

        /// <summary>
        /// Mời nhiều thành viên vào project
        /// POST api/projects/{projectId}/invitations
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(InvitationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<InvitationResponseDto>> InviteMembers(
            [FromRoute] Guid projectId,
            [FromBody] InviteMembersDto dto,
            CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _invitationService.InviteMembersAsync(projectId, userId, dto, ct);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting members to project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách lời mời của project
        /// GET api/projects/{projectId}/invitations
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProjectInvitationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<ProjectInvitationDto>>> GetProjectInvitations(
            [FromRoute] Guid projectId,
            CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var invitations = await _invitationService.GetProjectInvitationsAsync(projectId, ct);
                return Ok(invitations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitations for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Hủy lời mời
        /// DELETE api/projects/{projectId}/invitations/{invitationId}
        /// </summary>
        [HttpDelete("{invitationId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelInvitation(
            [FromRoute] Guid projectId,
            [FromRoute] Guid invitationId,
            CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _invitationService.CancelInvitationAsync(invitationId, userId, ct);

                if (!result)
                    return BadRequest(new { message = "Cannot cancel invitation" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling invitation {InvitationId}", invitationId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>
    /// Controller riêng cho việc xử lý invitation link (không cần projectId trong route)
    /// </summary>
    [ApiController]
    [Route("api/invitations")]
    public class InvitationController : ControllerBase
    {
        private readonly IProjectInvitationService _invitationService;
        private readonly ILogger<InvitationController> _logger;

        public InvitationController(
            IProjectInvitationService invitationService,
            ILogger<InvitationController> logger)
        {
            _invitationService = invitationService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin invitation bằng token
        /// GET api/invitations/{token}
        /// </summary>
        [HttpGet("{token}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProjectInvitationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectInvitationDto>> GetInvitationByToken(
            [FromRoute] string token,
            CancellationToken ct)
        {
            try
            {
                var invitation = await _invitationService.GetInvitationByTokenAsync(token, ct);

                if (invitation == null)
                    return NotFound(new { message = "Invitation not found" });

                return Ok(invitation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invitation by token");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Chấp nhận hoặc từ chối lời mời
        /// POST api/invitations/{token}/respond
        /// </summary>
        [HttpPost("{token}/respond")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RespondToInvitation(
            [FromRoute] string token,
            [FromBody] RespondToInvitationDto dto,
            CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _invitationService.RespondToInvitationAsync(token, userId, dto.Accept, ct);

                if (!result)
                    return BadRequest(new { message = "Cannot respond to invitation" });

                return Ok(new { message = dto.Accept ? "Invitation accepted" : "Invitation rejected" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to invitation");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
