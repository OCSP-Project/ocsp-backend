using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.ConstructionDiary;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConstructionDiaryController : ControllerBase
    {
        private readonly IConstructionDiaryService _diaryService;
        private readonly ILogger<ConstructionDiaryController> _logger;

        public ConstructionDiaryController(
            IConstructionDiaryService diaryService,
            ILogger<ConstructionDiaryController> logger)
        {
            _diaryService = diaryService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
        }

        /// <summary>
        /// Get diary by project and date
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="date">Diary date (yyyy-MM-dd)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Diary detail or 404 if not found</returns>
        [HttpGet("project/{projectId:guid}/date/{date}")]
        [ProducesResponseType(typeof(ConstructionDiaryDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDiaryByDate(
            [FromRoute] Guid projectId,
            [FromRoute] string date,
            CancellationToken ct)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                {
                    return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd" });
                }

                var diary = await _diaryService.GetDiaryByDateAsync(projectId, parsedDate, ct);

                if (diary == null)
                {
                    return NotFound(new { message = $"Diary not found for date {date}" });
                }

                return Ok(diary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting diary by date");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all diaries for a month
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of diary summaries</returns>
        [HttpGet("project/{projectId:guid}/month/{year:int}/{month:int}")]
        [ProducesResponseType(typeof(List<ConstructionDiarySummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDiariesByMonth(
            [FromRoute] Guid projectId,
            [FromRoute] int year,
            [FromRoute] int month,
            CancellationToken ct)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new { message = "Month must be between 1 and 12" });
                }

                var diaries = await _diaryService.GetDiariesByMonthAsync(projectId, year, month, ct);
                return Ok(diaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting diaries by month");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all diaries for a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of diary summaries</returns>
        [HttpGet("project/{projectId:guid}")]
        [ProducesResponseType(typeof(List<ConstructionDiarySummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllDiariesByProject(
            [FromRoute] Guid projectId,
            CancellationToken ct)
        {
            try
            {
                var diaries = await _diaryService.GetAllDiariesByProjectAsync(projectId, ct);
                return Ok(diaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all diaries");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new construction diary
        /// </summary>
        /// <param name="dto">Create diary DTO</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Created diary</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ConstructionDiaryDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDiary(
            [FromBody] CreateConstructionDiaryDto dto,
            CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var diary = await _diaryService.CreateDiaryAsync(dto, userId, ct);

                return CreatedAtAction(
                    nameof(GetDiaryByDate),
                    new { projectId = dto.ProjectId, date = dto.DiaryDate.ToString("yyyy-MM-dd") },
                    diary);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating diary");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update an existing construction diary
        /// </summary>
        /// <param name="diaryId">Diary ID</param>
        /// <param name="dto">Update diary DTO</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Updated diary</returns>
        [HttpPut("{diaryId:guid}")]
        [ProducesResponseType(typeof(ConstructionDiaryDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDiary(
            [FromRoute] Guid diaryId,
            [FromBody] UpdateConstructionDiaryDto dto,
            CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var diary = await _diaryService.UpdateDiaryAsync(diaryId, dto, userId, ct);
                return Ok(diary);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating diary");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a construction diary
        /// </summary>
        /// <param name="diaryId">Diary ID</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>No content</returns>
        [HttpDelete("{diaryId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDiary(
            [FromRoute] Guid diaryId,
            CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _diaryService.DeleteDiaryAsync(diaryId, userId, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting diary");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
