using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.BuildingElements;
using OCSP.Application.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/building-elements")]
    [Authorize]
    public class BuildingElementController : ControllerBase
    {
        private readonly IBuildingElementService _service;
        private readonly ILogger<BuildingElementController> _logger;
        public BuildingElementController(IBuildingElementService service, ILogger<BuildingElementController> logger)
        {
            _service = service; _logger = logger;
        }

        [HttpGet("models/{modelId:guid}/elements")]
        public async Task<IActionResult> GetByModel(Guid modelId)
        {
            var list = await _service.GetByModelAsync(modelId);
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var e = await _service.GetAsync(id);
            if (e == null) return NotFound(new { message = "Element not found" });
            return Ok(e);
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateBuildingElementRequest req)
        {
            var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
            var created = await _service.CreateAsync(req, userId);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBuildingElementRequest req)
        {
            var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
            var updated = await _service.UpdateAsync(id, req, userId);
            return Ok(updated);
        }

        [HttpPost("{elementId:guid}/tracking")]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> AddTracking(Guid elementId, [FromBody] AddTrackingRecordRequest req)
        {
            var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
            var hist = await _service.AddTrackingAsync(elementId, req, userId);
            return Ok(hist);
        }

        [HttpGet("{elementId:guid}/tracking/history")]
        public async Task<IActionResult> GetHistory(Guid elementId)
        {
            var list = await _service.GetHistoryAsync(elementId);
            return Ok(list);
        }

        [HttpPost("tracking/{historyId:guid}/photos")]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> AddPhoto(Guid historyId, [FromBody] AddTrackingPhotoRequest req)
        {
            var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
            var photo = await _service.AddPhotoAsync(historyId, req, userId);
            return Ok(photo);
        }

        // Multipart upload for actual photo file
        [HttpPost("tracking/{historyId:guid}/photos/upload")]
        [Authorize(Roles = "Supervisor,Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPhoto(Guid historyId, [FromForm] IFormFile photo, [FromForm] string? caption)
        {
            var userId = GetUserId(); if (userId == Guid.Empty) return Unauthorized();
            if (photo == null || photo.Length == 0) return BadRequest(new { message = "No photo provided" });
            var dto = await _service.AddPhotoAsync(historyId, photo, caption, userId);
            return Ok(dto);
        }

        [HttpGet("tracking/{historyId:guid}/photos")]
        public async Task<IActionResult> GetPhotos(Guid historyId)
        {
            var list = await _service.GetPhotosAsync(historyId);
            return Ok(list);
        }

        [HttpDelete("photos/{photoId:guid}")]
        [Authorize(Roles = "Supervisor,Admin")]
        public async Task<IActionResult> DeletePhoto(Guid photoId)
        {
            await _service.DeletePhotoAsync(photoId);
            return NoContent();
        }

        [HttpGet("models/{modelId:guid}/summary")]
        public async Task<IActionResult> GetModelSummary(Guid modelId)
        {
            var summary = await _service.GetModelSummaryAsync(modelId);
            return Ok(summary);
        }

        private Guid GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(id, out var uid) ? uid : Guid.Empty;
        }
    }
}
