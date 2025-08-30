using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OCSP.Application.Services.Interfaces;
using OCSP.Application.DTOs.Project;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // Create Project
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createDto)
        {
            try
            {
                // Get current user ID from JWT token
                var homeownerId = GetCurrentUserId();
                if (homeownerId == Guid.Empty)
                    return Unauthorized("User not authenticated");

                var project = await _projectService.CreateProjectAsync(createDto, homeownerId);
                // Return 201 Created with project data (removed CreatedAtAction since GetProjectById is commented)
                return StatusCode(201, project);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
