using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OCSP.Application.DTOs.Project;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize] // có thể thay bằng [Authorize(Policy = "HomeownerOnly")] nếu đã cấu hình
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        // GET api/projects/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDetailDto>> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var project = await _projectService.GetProjectByIdAsync(id, ct);
            if (project == null) return NotFound();
            return Ok(project);
        }
        [HttpGet("my-projects")]
        public async Task<IActionResult> GetMyProjects()
        {
            var homeownerId = GetCurrentUserId();
            if (homeownerId == Guid.Empty)
                return Unauthorized(new { message = "User not authenticated" });

            var projects = await _projectService.GetProjectsByHomeownerAsync(homeownerId);
            return Ok(projects);
        }


        // POST api/projects/create-with-files
        [HttpPost("create-with-files")]
        [RequestSizeLimit(100_000_000)] // 100MB limit
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<ProjectDetailDto>> CreateProjectWithFiles(
            [FromForm] CreateProjectDto dto,
            [FromForm] IFormFile drawingFile,
            [FromForm] IFormFile permitFile)
        {
            try
            {
                var homeownerId = GetCurrentUserId();
                if (homeownerId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var project = await _projectService.CreateProjectWithFilesAsync(
                    dto, drawingFile, permitFile, homeownerId);

                return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request creating project");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error creating project");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/projects (legacy endpoint - kept for backward compatibility)
        [HttpPost]
        [RequestSizeLimit(100_000_000)] // 100MB limit
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<ProjectDetailDto>> CreateProject(
            [FromForm] CreateProjectDto dto,
            [FromForm] IFormFile drawingFile,
            [FromForm] IFormFile permitFile)
        {
            try
            {
                var homeownerId = GetCurrentUserId();
                if (homeownerId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var project = await _projectService.CreateProjectWithFilesAsync(
                    dto, drawingFile, permitFile, homeownerId);

                return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request creating project");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error creating project");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT api/projects/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProject(
            [FromRoute] Guid id,
            [FromBody] UpdateProjectDto dto,
            CancellationToken ct)
        {
            try
            {
                var homeownerId = GetCurrentUserId();
                if (homeownerId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var project = await _projectService.UpdateProjectAsync(id, dto, homeownerId);
                return Ok(project);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request updating project");
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Forbidden updating project");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error updating project");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            // Ưu tiên NameIdentifier, fallback sang "sub" (OIDC/JWT phổ biến)
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
            return Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }
    }
}
