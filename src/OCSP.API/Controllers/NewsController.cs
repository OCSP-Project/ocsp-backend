using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.News;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/news")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(
            INewsService newsService,
            ILogger<NewsController> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        // =============== PUBLIC ENDPOINTS ===============

        /// <summary>
        /// Get published news list (public)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<NewsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<NewsDto>>> GetPublishedNews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null)
        {
            try
            {
                var news = await _newsService.GetPublishedNewsAsync(page, pageSize, category);
                return Ok(news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published news");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get news by ID (public)
        /// </summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(NewsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NewsDto>> GetNewsById(Guid id)
        {
            try
            {
                var news = await _newsService.GetNewsByIdAsync(id);
                if (news == null)
                    return NotFound(new { message = "News not found" });

                // Increment view count
                await _newsService.IncrementViewCountAsync(id);

                return Ok(news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting news by ID");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =============== N8N WEBHOOK ===============

        /// <summary>
        /// Webhook to receive news from n8n
        /// </summary>
        [HttpPost("webhook/n8n")]
        [AllowAnonymous] // Or add API key authentication
        [ProducesResponseType(typeof(NewsDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<NewsDto>> ReceiveFromN8n([FromBody] N8nNewsWebhookDto dto)
        {
            try
            {
                var news = await _newsService.ReceiveFromN8nAsync(dto);
                return CreatedAtAction(nameof(GetNewsById), new { id = news.Id }, news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving news from n8n");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // =============== ADMIN ENDPOINTS ===============

        /// <summary>
        /// Get all news (admin only)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize] // Add role check: Roles = "Admin"
        [ProducesResponseType(typeof(List<NewsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<NewsDto>>> GetAllNews(
            [FromQuery] bool? isPublished = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (!IsAdmin())
                    return Forbid("Only admin can access this endpoint");

                var news = await _newsService.GetAllNewsAsync(isPublished, page, pageSize);
                return Ok(news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all news");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create news manually (admin)
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(NewsDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<NewsDto>> CreateNews([FromBody] CreateNewsDto dto)
        {
            try
            {
                if (!IsAdmin())
                    return Forbid("Only admin can create news");

                var news = await _newsService.CreateNewsAsync(dto);
                return CreatedAtAction(nameof(GetNewsById), new { id = news.Id }, news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating news");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update news (admin)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(NewsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NewsDto>> UpdateNews(Guid id, [FromBody] UpdateNewsDto dto)
        {
            try
            {
                if (!IsAdmin())
                    return Forbid("Only admin can update news");

                var news = await _newsService.UpdateNewsAsync(id, dto);
                return Ok(news);
            }
            catch (OCSP.Application.Common.Exceptions.NotFoundException)
            {
                return NotFound(new { message = "News not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating news");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete news (admin)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteNews(Guid id)
        {
            try
            {
                if (!IsAdmin())
                    return Forbid("Only admin can delete news");

                await _newsService.DeleteNewsAsync(id);
                return NoContent();
            }
            catch (OCSP.Application.Common.Exceptions.NotFoundException)
            {
                return NotFound(new { message = "News not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting news");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Publish news (admin)
        /// </summary>
        [HttpPost("{id:guid}/publish")]
        [Authorize]
        [ProducesResponseType(typeof(NewsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<NewsDto>> PublishNews(Guid id)
        {
            try
            {
                if (!IsAdmin())
                    return Forbid("Only admin can publish news");

                var news = await _newsService.PublishNewsAsync(id);
                return Ok(news);
            }
            catch (OCSP.Application.Common.Exceptions.NotFoundException)
            {
                return NotFound(new { message = "News not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing news");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Unpublish news (admin)
        /// </summary>
        [HttpPost("{id:guid}/unpublish")]
        [Authorize]
        [ProducesResponseType(typeof(NewsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<NewsDto>> UnpublishNews(Guid id)
        {
            try
            {
                if (!IsAdmin())
                    return Forbid("Only admin can unpublish news");

                var news = await _newsService.UnpublishNewsAsync(id);
                return Ok(news);
            }
            catch (OCSP.Application.Common.Exceptions.NotFoundException)
            {
                return NotFound(new { message = "News not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing news");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Schedule news publishing (admin)
        /// </summary>
        [HttpPost("{id:guid}/schedule")]
        [Authorize]
        [ProducesResponseType(typeof(NewsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<NewsDto>> ScheduleNews(Guid id, [FromBody] ScheduleNewsDto dto)
        {
            try
            {
                if (!IsAdmin())
                    return Forbid("Only admin can schedule news");

                var news = await _newsService.ScheduleNewsAsync(id, dto);
                return Ok(news);
            }
            catch (OCSP.Application.Common.Exceptions.NotFoundException)
            {
                return NotFound(new { message = "News not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling news");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private bool IsAdmin()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role)
                         ?? User.FindFirstValue("role");

            return roleClaim == "Admin" || roleClaim == "0";
        }
    }
}
