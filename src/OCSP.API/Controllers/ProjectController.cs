using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // POST api/projects
        [HttpPost]
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProjectDetailDto>> CreateProject(
            [FromBody] CreateProjectDto createDto,
            CancellationToken ct)
        {
            try
            {
                var homeownerId = GetCurrentUserId();
                if (homeownerId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var project = await _projectService.CreateProjectAsync(createDto, homeownerId);

                // 201 + Location header đến GET /api/projects/{id}
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

        private Guid GetCurrentUserId()
        {
            // Ưu tiên NameIdentifier, fallback sang "sub" (OIDC/JWT phổ biến)
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
            return Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }
    }
}
