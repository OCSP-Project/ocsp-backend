using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Budget;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/budgets")]
    [Authorize]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<BudgetController> _logger;

        public BudgetController(IBudgetService budgetService, ILogger<BudgetController> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }

        // GET api/budgets/project/{projectId}
        [HttpGet("project/{projectId:guid}")]
        [ProducesResponseType(typeof(List<BudgetDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<BudgetDetailDto>>> GetAllByProject(
            [FromRoute] Guid projectId,
            CancellationToken ct)
        {
            try
            {
                var budgetDetails = await _budgetService.GetAllByProjectAsync(projectId, ct);
                return Ok(budgetDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting budget details for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/budgets/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BudgetDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BudgetDetailDto>> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var budgetDetail = await _budgetService.GetByIdAsync(id, ct);
                if (budgetDetail == null)
                    return NotFound(new { message = "Budget detail not found" });

                return Ok(budgetDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting budget detail {BudgetDetailId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/budgets
        [HttpPost]
        [ProducesResponseType(typeof(BudgetDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BudgetDetailDto>> Create(
            [FromBody] CreateBudgetDetailDto dto,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var budgetDetail = await _budgetService.CreateAsync(dto, currentUserId, ct);
                return CreatedAtAction(nameof(GetById), new { id = budgetDetail.Id }, budgetDetail);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request creating budget detail");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget detail");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT api/budgets/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(BudgetDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BudgetDetailDto>> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateBudgetDetailDto dto,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var budgetDetail = await _budgetService.UpdateAsync(id, dto, currentUserId, ct);
                return Ok(budgetDetail);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request updating budget detail");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating budget detail {BudgetDetailId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE api/budgets/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                await _budgetService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request deleting budget detail");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting budget detail {BudgetDetailId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/budgets/project/{projectId}/summary
        [HttpGet("project/{projectId:guid}/summary")]
        [ProducesResponseType(typeof(BudgetSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BudgetSummaryDto>> GetSummary(
            [FromRoute] Guid projectId,
            CancellationToken ct)
        {
            try
            {
                var summary = await _budgetService.GetSummaryAsync(projectId, ct);
                return Ok(summary);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request getting budget summary");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting budget summary for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/budgets/project/{projectId}/by-category
        [HttpGet("project/{projectId:guid}/by-category")]
        [ProducesResponseType(typeof(List<BudgetByCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<BudgetByCategoryDto>>> GetByCategory(
            [FromRoute] Guid projectId,
            CancellationToken ct)
        {
            try
            {
                var categoryBreakdown = await _budgetService.GetByCategoryAsync(projectId, ct);
                return Ok(categoryBreakdown);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting budget by category for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/budgets/project/{projectId}/recalculate
        [HttpPost("project/{projectId:guid}/recalculate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RecalculateProjectBudget(
            [FromRoute] Guid projectId,
            CancellationToken ct)
        {
            try
            {
                await _budgetService.RecalculateProjectBudgetAsync(projectId, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating project budget for project {ProjectId}", projectId);
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
