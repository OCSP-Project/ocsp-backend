using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.ProjectTimeline;
using OCSP.Application.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectTimelineController : ControllerBase
    {
        private readonly IProjectTimelineService _timelineService;

        public ProjectTimelineController(IProjectTimelineService timelineService)
        {
            _timelineService = timelineService;
        }

        [HttpPost]
        [Route("")]
        //[Authorize(Roles = "Contractor")]
        public async Task<IActionResult> CreateProjectTimeline([FromBody] CreateProjectTimelineDto dto, CancellationToken ct)
        {
            var result = await _timelineService.CreateProjectTimelineAsync(dto, ct);
            return Ok(result);
        }

        /// <summary>
        /// Lấy dữ liệu project timeline cho Gantt chart (bao gồm milestones, deliverables)
        /// </summary>
        [HttpGet]
        [Route("project/{projectId}")]
        //[Authorize]
        public async Task<IActionResult> GetProjectTimelineForGantt(Guid projectId, CancellationToken ct)
        {
            var result = await _timelineService.GetProjectTimelineForGanttAsync(projectId, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

    
        [HttpPut]
        [Route("deliverable/progress")]
        //[Authorize(Roles = "Contractor")]
        public async Task<IActionResult> UpdateDeliverableProgress([FromBody] UpdateDeliverableProgressDto dto, CancellationToken ct)
        {
            var result = await _timelineService.UpdateDeliverableProgressAsync(dto, ct);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }

        
        [HttpPut]
        [Route("milestone/progress")]
        //[Authorize(Roles = "Contractor")]
        public async Task<IActionResult> UpdateMilestoneProgress([FromBody] UpdateMilestoneProgressDto dto, CancellationToken ct)
        {
            var result = await _timelineService.UpdateMilestoneProgressAsync(dto, ct);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }
    }
}