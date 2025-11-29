using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Budget;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/work-item-comments")]
    [Authorize]
    public class WorkItemCommentController : ControllerBase
    {
        private readonly IWorkItemCommentService _commentService;
        private readonly ILogger<WorkItemCommentController> _logger;

        public WorkItemCommentController(
            IWorkItemCommentService commentService,
            ILogger<WorkItemCommentController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        // POST api/work-item-comments
        [HttpPost]
        [ProducesResponseType(typeof(WorkItemCommentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<WorkItemCommentDto>> Create(
            [FromBody] CreateWorkItemCommentDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comment = await _commentService.CreateAsync(dto, userId, ct);
                return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work item comment");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT api/work-item-comments/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(WorkItemCommentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkItemCommentDto>> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateWorkItemCommentDto dto,
            CancellationToken ct = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comment = await _commentService.UpdateAsync(id, dto, userId, ct);
                return Ok(comment);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work item comment {CommentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE api/work-item-comments/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid id,
            CancellationToken ct = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _commentService.DeleteAsync(id, userId, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work item comment {CommentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/work-item-comments/work-item/{workItemId}
        [HttpGet("work-item/{workItemId:guid}")]
        [ProducesResponseType(typeof(List<WorkItemCommentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<WorkItemCommentDto>>> GetByWorkItemId(
            [FromRoute] Guid workItemId,
            CancellationToken ct = default)
        {
            try
            {
                var comments = await _commentService.GetByWorkItemIdAsync(workItemId, ct);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for work item {WorkItemId}", workItemId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/work-item-comments/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(WorkItemCommentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<WorkItemCommentDto>> GetById(
            [FromRoute] Guid id,
            CancellationToken ct = default)
        {
            try
            {
                var comment = await _commentService.GetByIdAsync(id, ct);
                if (comment == null)
                {
                    return NotFound(new { message = "Comment not found" });
                }
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work item comment {CommentId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
