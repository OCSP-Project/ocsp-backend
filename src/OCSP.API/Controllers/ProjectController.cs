using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OCSP.Application.DTOs.Project;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;
using OCSP.API.Models;

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
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100_000_000)] // 100MB limit
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<ProjectDetailDto>> CreateProjectWithFiles([FromForm] CreateProjectWithFilesForm form)
        {
            try
            {
                var homeownerId = GetCurrentUserId();
                if (homeownerId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var dto = new CreateProjectDto
                {
                    Name = form.Name,
                    Address = form.Address,
                    Budget = form.Budget,
                    Description = form.Description,
                    FloorArea = form.FloorArea ?? 0,
                    NumberOfFloors = form.NumberOfFloors ?? 0,
                    PermitNumber = form.PermitNumber,
                    ContractorId = form.ContractorId
                };

                var project = await _projectService.CreateProjectWithFilesAsync(
                    dto, form.DrawingFile, form.PermitFile, homeownerId);

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
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(100_000_000)] // 100MB limit
        [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<ProjectDetailDto>> CreateProject([FromForm] CreateProjectWithFilesForm form)
        {
            try
            {
                var homeownerId = GetCurrentUserId();
                if (homeownerId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var dto = new CreateProjectDto
                {
                    Name = form.Name,
                    Address = form.Address,
                    Budget = form.Budget,
                    Description = form.Description,
                    FloorArea = form.FloorArea ?? 0,
                    NumberOfFloors = form.NumberOfFloors ?? 0,
                    PermitNumber = form.PermitNumber,
                    ContractorId = form.ContractorId
                };

                var project = await _projectService.CreateProjectWithFilesAsync(
                    dto, form.DrawingFile, form.PermitFile, homeownerId);

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

        

        // GET api/projects/documents/{documentId}/download - Download any project document by its id
        [HttpGet("documents/{documentId:guid}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadDocument([FromRoute] Guid documentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                // Use ProjectDocumentService via ProjectService if exposed; otherwise inject service directly.
                // Here we rely on ProjectService exposing a passthrough (already available in ProjectDocumentService).
                // For simplicity, resolve via ProjectService when available.
                var (fileStream, fileName, contentType) = await _projectService.DownloadDocumentByIdAsync(documentId, userId);
                return File(fileStream, contentType, fileName);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request downloading document");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Forbidden downloading document");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error downloading document");
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
