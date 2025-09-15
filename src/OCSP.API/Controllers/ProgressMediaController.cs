using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.ProgressMedia;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/gallery")]
    [Authorize]
    public class ProgressMediaController : ControllerBase
    {
        private readonly IProgressMediaService _progressMediaService;
        private readonly ILogger<ProgressMediaController> _logger;

        public ProgressMediaController(IProgressMediaService progressMediaService, ILogger<ProgressMediaController> logger)
        {
            _progressMediaService = progressMediaService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách ảnh tiến độ của project
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProgressMediaListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProgressMediaListDto>> GetGallery(
            [FromRoute] Guid projectId,
            [FromQuery] Guid? taskId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                // Kiểm tra quyền xem
                if (!await _progressMediaService.IsUserAuthorizedForProjectAsync(projectId, userId, ct))
                    return Forbid();

                var result = await _progressMediaService.GetByProjectIdAsync(projectId, taskId, fromDate, toDate, page, pageSize, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting gallery for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Upload ảnh tiến độ
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProgressMediaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ProgressMediaDto>> UploadMedia(
            [FromRoute] Guid projectId,
            [FromForm] IFormFile file,
            [FromForm] string caption = "",
            [FromForm] Guid? taskId = null,
            [FromForm] Guid? progressUpdateId = null,
            CancellationToken ct = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File is required" });

                // Kiểm tra loại file
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    return BadRequest(new { message = "Only image files are allowed" });

                // Kiểm tra kích thước file (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                    return BadRequest(new { message = "File size must be less than 10MB" });

                var result = await _progressMediaService.UploadAsync(projectId, file, caption, taskId, progressUpdateId, userId, ct);
                return CreatedAtAction(nameof(GetMedia), new { projectId, mediaId = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized upload attempt for project {ProjectId}", projectId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading media for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin 1 ảnh
        /// </summary>
        [HttpGet("{mediaId:guid}")]
        [ProducesResponseType(typeof(ProgressMediaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProgressMediaDto>> GetMedia(
            [FromRoute] Guid projectId,
            [FromRoute] Guid mediaId,
            CancellationToken ct = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                // Kiểm tra quyền xem
                if (!await _progressMediaService.IsUserAuthorizedForProjectAsync(projectId, userId, ct))
                    return Forbid();

                var media = await _progressMediaService.GetByIdAsync(mediaId, ct);
                if (media == null)
                    return NotFound();

                return Ok(media);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting media {MediaId} for project {ProjectId}", mediaId, projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Xóa ảnh tiến độ
        /// </summary>
        [HttpDelete("{mediaId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteMedia(
            [FromRoute] Guid projectId,
            [FromRoute] Guid mediaId,
            CancellationToken ct = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var success = await _progressMediaService.DeleteAsync(mediaId, userId, ct);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting media {MediaId} for project {ProjectId}", mediaId, projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
            return Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }
    }
}
