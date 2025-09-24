using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.ProjectDailyResource;
using OCSP.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectDailyResourceController : ControllerBase
    {
        private readonly IProjectDailyResourceService _service;
        private readonly ILogger<ProjectDailyResourceController> _logger;

        public ProjectDailyResourceController(
            IProjectDailyResourceService service,
            ILogger<ProjectDailyResourceController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Tạo báo cáo tài nguyên hàng ngày (Supervisor, Contractor)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Supervisor,Contractor")]
        [ProducesResponseType(typeof(ProjectDailyResourceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ProjectDailyResourceDto>> CreateDailyResource(
            [FromBody] CreateProjectDailyResourceDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _service.CreateDailyResourceAsync(dto, userId, cancellationToken);
                return CreatedAtAction(nameof(GetDailyResourceById), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to create daily resource");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation creating daily resource");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating daily resource");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cập nhật báo cáo tài nguyên hàng ngày (Supervisor, Contractor)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Supervisor,Contractor")]
        [ProducesResponseType(typeof(ProjectDailyResourceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDailyResourceDto>> UpdateDailyResource(
            [FromRoute] Guid id,
            [FromBody] UpdateProjectDailyResourceDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _service.UpdateDailyResourceAsync(id, dto, userId, cancellationToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to update daily resource");
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Daily resource not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating daily resource");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Xóa báo cáo tài nguyên hàng ngày (Supervisor, Contractor)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Supervisor,Contractor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDailyResource(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _service.DeleteDailyResourceAsync(id, userId, cancellationToken);
                if (!result)
                    return NotFound(new { message = "Daily resource not found" });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to delete daily resource");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting daily resource");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy báo cáo tài nguyên theo ID (Tất cả roles)
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProjectDailyResourceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDailyResourceDto>> GetDailyResourceById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetDailyResourceByIdAsync(id, cancellationToken);
                if (result == null)
                    return NotFound(new { message = "Daily resource not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily resource by ID");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy báo cáo tài nguyên theo project và ngày (Tất cả roles)
        /// </summary>
        [HttpGet("project/{projectId:guid}/date/{resourceDate:datetime}")]
        [ProducesResponseType(typeof(ProjectDailyResourceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDailyResourceDto>> GetDailyResourceByProjectAndDate(
            [FromRoute] Guid projectId,
            [FromRoute] DateTime resourceDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetDailyResourceByProjectAndDateAsync(projectId, resourceDate, cancellationToken);
                if (result == null)
                    return NotFound(new { message = "Daily resource not found for this date" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily resource by project and date");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách báo cáo tài nguyên theo project (Tất cả roles)
        /// </summary>
        [HttpGet("project/{projectId:guid}")]
        [ProducesResponseType(typeof(List<ProjectDailyResourceListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProjectDailyResourceListDto>>> GetDailyResourcesByProject(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetDailyResourcesByProjectAsync(projectId, cancellationToken);
                _logger.LogInformation("[ProjectDailyResource] List for project {ProjectId} -> Count: {Count}", projectId, result?.Count);
                if (result != null && result.Count > 0)
                {
                    var first = result[0];
                    _logger.LogInformation(
                        "[ProjectDailyResource] First item {Id} on {Date} flags => TowerCrane:{TowerCrane}, ConcreteMixer:{ConcreteMixer}, MaterialHoist:{MaterialHoist}, PassengerHoist:{PassengerHoist}, Vibrator:{Vibrator}",
                        first.Id,
                        first.ResourceDate,
                        first.TowerCrane,
                        first.ConcreteMixer,
                        first.MaterialHoist,
                        first.PassengerHoist,
                        first.Vibrator);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily resources by project");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách báo cáo tài nguyên theo project và khoảng thời gian (Tất cả roles)
        /// </summary>
        [HttpGet("project/{projectId:guid}/date-range")]
        [ProducesResponseType(typeof(List<ProjectDailyResourceListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProjectDailyResourceListDto>>> GetDailyResourcesByProjectAndDateRange(
            [FromRoute] Guid projectId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetDailyResourcesByProjectAndDateRangeAsync(projectId, startDate, endDate, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily resources by project and date range");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }
    }
}
