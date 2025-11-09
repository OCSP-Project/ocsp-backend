// OCSP.API/Controllers/SupervisorContractsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OCSP.Application.Services.Interfaces;
using OCSP.Application.DTOs.Contracts;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/supervisor-contracts")]
    [Authorize]
    public class SupervisorContractsController : ControllerBase
    {
        private readonly ISupervisorContractService _service;

        public SupervisorContractsController(ISupervisorContractService service) => _service = service;

        // Helper: lấy userId hiện tại
        private Guid Me()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(v, out var g) ? g : Guid.Empty;
        }

        /// <summary>
        /// Tạo hợp đồng giám sát viên cho project (chưa thanh toán)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SupervisorContractDto>> Create([FromBody] CreateSupervisorContractDto dto, CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            try
            {
                var result = await _service.CreateForProjectAsync(dto.ProjectId, uid, dto.MonthlyPrice, ct);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>
        /// Lấy chi tiết 1 hợp đồng giám sát viên
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SupervisorContractDto>> GetById(Guid id, CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            try
            {
                var dto = await _service.GetByIdAsync(id, uid, ct);
                return Ok(dto);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
        }

        /// <summary>
        /// Liệt kê tất cả hợp đồng giám sát viên của tôi
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupervisorContractListItemDto>>> GetAll(CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            var list = await _service.ListMyContractsAsync(uid, ct);
            return Ok(list);
        }

        /// <summary>
        /// Lấy hợp đồng giám sát viên theo projectId
        /// </summary>
        [HttpGet("by-project/{projectId:guid}")]
        public async Task<ActionResult<SupervisorContractDto>> GetByProjectId(Guid projectId, CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            try
            {
                var dto = await _service.GetByProjectIdAsync(projectId, uid, ct);
                if (dto == null) return NotFound("Supervisor contract not found for this project");
                return Ok(dto);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        /// <summary>
        /// Homeowner ký hợp đồng giám sát viên
        /// </summary>
        [HttpPost("{id:guid}/sign-homeowner")]
        public async Task<ActionResult<SupervisorContractDto>> SignByHomeowner(
            Guid id, 
            [FromBody] SignSupervisorContractDto dto, 
            CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            try
            {
                var result = await _service.SignByHomeownerAsync(id, dto, uid, ct);
                return Ok(result);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>
        /// Supervisor ký hợp đồng giám sát viên
        /// </summary>
        [HttpPost("{id:guid}/sign-supervisor")]
        public async Task<ActionResult<SupervisorContractDto>> SignBySupervisor(
            Guid id, 
            [FromBody] SignSupervisorContractDto dto, 
            CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            try
            {
                var result = await _service.SignBySupervisorAsync(id, dto, uid, ct);
                return Ok(result);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        /// <summary>
        /// Tạo PDF template cho hợp đồng giám sát viên (nếu chưa có)
        /// </summary>
        [HttpPost("{id:guid}/generate-pdf")]
        public async Task<ActionResult<SupervisorContractDto>> GeneratePdf(Guid id, CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            try
            {
                var result = await _service.GeneratePdfForContractAsync(id, uid, ct);
                return Ok(result);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        /// <summary>
        /// Tải PDF hợp đồng giám sát viên
        /// </summary>
        [HttpGet("{id:guid}/pdf")]
        public async Task<IActionResult> GetContractPdf(Guid id, CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            try
            {
                var pdfBytes = await _service.GeneratePdfAsync(id, uid, ct);
                return File(pdfBytes, "application/pdf", $"supervisor_contract_{id}.pdf");
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (NotImplementedException) { return StatusCode(500, "PDF generation not yet implemented"); }
        }
    }
}
