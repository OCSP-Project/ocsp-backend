// OCSP.API/Controllers/RegistrationRequestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.RegistrationRequest;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/registration-request")]
    public class RegistrationRequestController : ControllerBase
    {
        private readonly IRegistrationRequestService _service;
        private readonly ILogger<RegistrationRequestController> _logger;

        public RegistrationRequestController(
            IRegistrationRequestService service,
            ILogger<RegistrationRequestController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // POST api/registration-request (Public - no auth required)
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegistrationRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegistrationRequestDto>> Submit([FromBody] SubmitRegistrationRequestDto dto)
        {
            try
            {
                var result = await _service.SubmitAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (OCSP.Application.Common.Exceptions.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error submitting registration request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error submitting registration request");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/registration-request (Admin only)
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(List<RegistrationRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<RegistrationRequestDto>>> GetAll()
        {
            try
            {
                if (!IsAdmin())
                {
                    return StatusCode(403, new { message = "Chỉ admin mới có quyền xem danh sách yêu cầu đăng ký" });
                }

                var requests = await _service.GetAllAsync();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error getting registration requests");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/registration-request/{id} (Admin only)
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(RegistrationRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegistrationRequestDto>> GetById([FromRoute] Guid id)
        {
            try
            {
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền xem yêu cầu đăng ký");
                }

                var request = await _service.GetByIdAsync(id);
                if (request == null)
                    return NotFound(new { message = "Yêu cầu đăng ký không tồn tại" });

                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error getting registration request");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/registration-request/{id}/approve (Admin only)
        [HttpPost("{id:guid}/approve")]
        [Authorize]
        [ProducesResponseType(typeof(RegistrationRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegistrationRequestDto>> Approve(
            [FromRoute] Guid id,
            [FromBody] ApproveRegistrationRequestDto dto)
        {
            try
            {
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền phê duyệt yêu cầu đăng ký");
                }

                var adminUserId = GetCurrentUserId();
                var result = await _service.ApproveAsync(id, dto, adminUserId);
                return Ok(result);
            }
            catch (OCSP.Application.Common.Exceptions.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error approving registration request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error approving registration request");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/registration-request/{id}/reject (Admin only)
        [HttpPost("{id:guid}/reject")]
        [Authorize]
        [ProducesResponseType(typeof(RegistrationRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegistrationRequestDto>> Reject(
            [FromRoute] Guid id,
            [FromBody] RejectRegistrationRequestDto dto)
        {
            try
            {
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền từ chối yêu cầu đăng ký");
                }

                var adminUserId = GetCurrentUserId();
                var result = await _service.RejectAsync(id, dto, adminUserId);
                return Ok(result);
            }
            catch (OCSP.Application.Common.Exceptions.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error rejecting registration request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error rejecting registration request");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private bool IsAdmin()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role)
                         ?? User.FindFirstValue("role");

            return roleClaim == "Admin" || roleClaim == "0";
        }

        private Guid GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
            return Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }
    }
}

