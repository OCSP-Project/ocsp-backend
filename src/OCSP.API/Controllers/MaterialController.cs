using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Material;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(IMaterialService materialService, ILogger<MaterialController> logger)
        {
            _materialService = materialService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
        }

        #region Material Request Endpoints

        // POST api/material/requests
        [HttpPost("requests")]
        [ProducesResponseType(typeof(MaterialRequestDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRequest([FromBody] CreateMaterialRequestDto dto, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _materialService.CreateRequestAsync(dto.ProjectId, userId, ct);
                return CreatedAtAction(nameof(GetRequestById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating material request");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/material/requests/project/{projectId}
        [HttpGet("requests/project/{projectId:guid}")]
        [ProducesResponseType(typeof(List<MaterialRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRequestsByProject([FromRoute] Guid projectId, CancellationToken ct)
        {
            try
            {
                var requests = await _materialService.GetAllRequestsAsync(projectId, ct);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting material requests");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/material/requests/{id}
        [HttpGet("requests/{id:guid}")]
        [ProducesResponseType(typeof(MaterialRequestDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRequestById([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var request = await _materialService.GetRequestByIdAsync(id, ct);
                if (request == null)
                    return NotFound(new { message = "Material request not found" });

                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting material request");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/material/requests/{id}/import
        [HttpPost("requests/{id:guid}/import")]
        [ProducesResponseType(typeof(MaterialRequestDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportMaterials([FromRoute] Guid id, [FromForm] IFormFile file, CancellationToken ct)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "File is required" });

                var result = await _materialService.ImportMaterialsFromExcelAsync(id, file, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing materials");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Approval Endpoints

        // POST api/material/requests/{id}/approve/homeowner
        [HttpPost("requests/{id:guid}/approve/homeowner")]
        [ProducesResponseType(typeof(MaterialRequestDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ApproveByHomeowner([FromRoute] Guid id, [FromBody] ApproveMaterialRequestDto dto, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _materialService.ApproveByHomeownerAsync(id, userId, dto, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving by homeowner");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/material/requests/{id}/approve/supervisor
        [HttpPost("requests/{id:guid}/approve/supervisor")]
        [ProducesResponseType(typeof(MaterialRequestDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ApproveBySupervisor([FromRoute] Guid id, [FromBody] ApproveMaterialRequestDto dto, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _materialService.ApproveBySupervisorAsync(id, userId, dto, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving by supervisor");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/material/requests/{id}/reject
        [HttpPost("requests/{id:guid}/reject")]
        [ProducesResponseType(typeof(MaterialRequestDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RejectRequest([FromRoute] Guid id, [FromBody] RejectMaterialRequestDto dto, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _materialService.RejectRequestAsync(id, userId, dto, ct);
                return Ok(result);
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
                _logger.LogError(ex, "Error rejecting request");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE api/material/requests/{id}
        [HttpDelete("requests/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteRequest([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _materialService.DeleteRequestAsync(id, userId, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting material request");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE api/material/requests/{id}/materials
        [HttpDelete("requests/{id:guid}/materials")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClearImportedMaterials([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _materialService.ClearImportedMaterialsAsync(id, userId, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing imported materials");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Material Endpoints

        // GET api/material/project/{projectId}
        [HttpGet("project/{projectId:guid}")]
        [ProducesResponseType(typeof(List<MaterialDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMaterialsByProject([FromRoute] Guid projectId, CancellationToken ct)
        {
            try
            {
                var materials = await _materialService.GetMaterialsByProjectAsync(projectId, ct);
                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting materials");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/material/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(MaterialDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMaterialById([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var material = await _materialService.GetMaterialByIdAsync(id, ct);
                if (material == null)
                    return NotFound(new { message = "Material not found" });

                return Ok(material);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting material");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT api/material/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(MaterialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMaterial([FromRoute] Guid id, [FromBody] UpdateMaterialDto dto, CancellationToken ct)
        {
            try
            {
                var result = await _materialService.UpdateMaterialAsync(id, dto, ct);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating material");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // PUT api/material/{id}/actual-quantity
        [HttpPut("{id:guid}/actual-quantity")]
        [ProducesResponseType(typeof(MaterialDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateActualQuantity([FromRoute] Guid id, [FromBody] UpdateActualQuantityDto dto, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _materialService.UpdateActualQuantityAsync(id, dto, userId, ct);
                return Ok(result);
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
                _logger.LogError(ex, "Error updating actual quantity");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion

        #region Payment Endpoints

        // POST api/material/payments
        [HttpPost("payments")]
        [ProducesResponseType(typeof(MaterialPaymentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePayment([FromBody] CreateMaterialPaymentDto dto, CancellationToken ct)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _materialService.CreatePaymentAsync(dto, userId, ct);
                return CreatedAtAction(nameof(GetPaymentsByMaterial), new { materialId = dto.MaterialId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/material/{materialId}/payments
        [HttpGet("{materialId:guid}/payments")]
        [ProducesResponseType(typeof(List<MaterialPaymentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaymentsByMaterial([FromRoute] Guid materialId, CancellationToken ct)
        {
            try
            {
                var payments = await _materialService.GetPaymentsByMaterialAsync(materialId, ct);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/material/payments/project/{projectId}
        [HttpGet("payments/project/{projectId:guid}")]
        [ProducesResponseType(typeof(List<MaterialPaymentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaymentsByProject([FromRoute] Guid projectId, CancellationToken ct)
        {
            try
            {
                var payments = await _materialService.GetPaymentsByProjectAsync(projectId, ct);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        #endregion
    }
}
