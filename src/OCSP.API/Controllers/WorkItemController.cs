using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Budget;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/work-items")]
    [Authorize]
    public class WorkItemController : ControllerBase
    {
        private readonly IWorkItemService _workItemService;
        private readonly ILogger<WorkItemController> _logger;

        public WorkItemController(IWorkItemService workItemService, ILogger<WorkItemController> logger)
        {
            _workItemService = workItemService;
            _logger = logger;
        }

        // GET api/work-items/project/{projectId}
        [HttpGet("project/{projectId:guid}")]
        [ProducesResponseType(typeof(List<WorkItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<WorkItemDto>>> GetAllByProject(
            [FromRoute] Guid projectId,
            [FromQuery] bool rootLevelOnly = false,
            [FromQuery] bool includeChildren = true,
            CancellationToken ct = default)
        {
            try
            {
                var workItems = await _workItemService.GetAllByProjectAsync(projectId, rootLevelOnly, includeChildren, ct);
                return Ok(workItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work items for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/work-items/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(WorkItemDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<WorkItemDetailDto>> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var workItem = await _workItemService.GetByIdAsync(id, ct);
                if (workItem == null)
                    return NotFound(new { message = "Work item not found" });

                return Ok(workItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work item {WorkItemId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/work-items/project/{projectId}/tree
        [HttpGet("project/{projectId:guid}/tree")]
        [ProducesResponseType(typeof(List<WorkItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<WorkItemDto>>> GetTreeByProject(
            [FromRoute] Guid projectId,
            CancellationToken ct)
        {
            try
            {
                var workItems = await _workItemService.GetAllByProjectAsync(projectId, false, true, ct);
                return Ok(workItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work item tree for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/work-items
        [HttpPost]
        [ProducesResponseType(typeof(WorkItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<WorkItemDto>> Create(
            [FromBody] CreateWorkItemDto dto,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var workItem = await _workItemService.CreateAsync(dto, currentUserId, ct);
                return CreatedAtAction(nameof(GetById), new { id = workItem.Id }, workItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request creating work item");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work item");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT api/work-items/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(WorkItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkItemDto>> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateWorkItemDto dto,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var workItem = await _workItemService.UpdateAsync(id, dto, currentUserId, ct);
                return Ok(workItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request updating work item");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work item {WorkItemId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT api/work-items/{id}/progress
        [HttpPut("{id:guid}/progress")]
        [ProducesResponseType(typeof(WorkItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkItemDto>> UpdateProgress(
            [FromRoute] Guid id,
            [FromBody] UpdateProgressDto dto,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var workItem = await _workItemService.UpdateProgressAsync(id, dto, currentUserId, ct);
                return Ok(workItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request updating work item progress");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation updating work item progress");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work item progress {WorkItemId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE api/work-items/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                await _workItemService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request deleting work item");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work item {WorkItemId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/work-items/{id}/comments
        [HttpPost("{id:guid}/comments")]
        [ProducesResponseType(typeof(WorkItemDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkItemDetailDto>> AddComment(
            [FromRoute] Guid id,
            [FromBody] AddCommentDto dto,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var workItem = await _workItemService.AddCommentAsync(id, dto, currentUserId, ct);
                return Ok(workItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request adding comment to work item");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to work item {WorkItemId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/work-items/{id}/documents
        [HttpPost("{id:guid}/documents")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)] // 50MB limit
        [ProducesResponseType(typeof(WorkItemDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkItemDocumentDto>> UploadDocument(
            [FromRoute] Guid id,
            [FromForm] IFormFile file,
            [FromForm] string documentType,
            [FromForm] string? description,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File is required" });

                var document = await _workItemService.AddDocumentAsync(id, file, documentType, description, currentUserId, ct);
                return Ok(document);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request uploading document to work item");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document to work item {WorkItemId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/work-items/{id}/activities
        [HttpGet("{id:guid}/activities")]
        [ProducesResponseType(typeof(List<WorkItemActivityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<WorkItemActivityDto>>> GetActivities(
            [FromRoute] Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            try
            {
                var activities = await _workItemService.GetActivitiesAsync(id, pageNumber, pageSize, ct);
                return Ok(activities);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request getting work item activities");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activities for work item {WorkItemId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/work-items/project/{projectId}/gantt
        [HttpGet("project/{projectId:guid}/gantt")]
        [ProducesResponseType(typeof(GanttChartDataDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GanttChartDataDto>> GetGanttChartData(
            [FromRoute] Guid projectId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken ct = default)
        {
            try
            {
                var ganttData = await _workItemService.GetGanttChartDataAsync(projectId, fromDate, toDate, ct);
                return Ok(ganttData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Gantt chart data for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/work-items/import
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)] // 10MB limit
        [ProducesResponseType(typeof(List<WorkItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ImportBudgetResponseDto>> ImportFromExcel(
            [FromForm] Guid projectId,
            [FromForm] IFormFile file,
            [FromForm] bool overwriteExisting = false,
            CancellationToken ct = default)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File is required" });

                var result = await _workItemService.ImportFromExcelAsync(projectId, file, overwriteExisting, currentUserId, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request importing work items from Excel");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing work items from Excel");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/work-items/project/{projectId}/export
        [HttpGet("project/{projectId:guid}/export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExportToExcel([FromRoute] Guid projectId, CancellationToken ct)
        {
            try
            {
                var fileBytes = await _workItemService.ExportToExcelAsync(projectId, ct);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"WorkItems_{projectId}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting work items to Excel for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/work-items/{id}/assign-users
        [HttpPost("{id:guid}/assign-users")]
        [ProducesResponseType(typeof(WorkItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkItemDto>> AssignUsers(
            [FromRoute] Guid id,
            [FromBody] List<Guid> userIds,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var workItem = await _workItemService.AssignUsersAsync(id, userIds, currentUserId, ct);
                return Ok(workItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request assigning users to work item");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning users to work item {WorkItemId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE api/work-items/{id}/unassign-user/{userId}
        [HttpDelete("{id:guid}/unassign-user/{userId:guid}")]
        [ProducesResponseType(typeof(WorkItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkItemDto>> UnassignUser(
            [FromRoute] Guid id,
            [FromRoute] Guid userId,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var workItem = await _workItemService.UnassignUserAsync(id, userId, currentUserId, ct);
                return Ok(workItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request unassigning user from work item");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning user from work item {WorkItemId}", id);
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
