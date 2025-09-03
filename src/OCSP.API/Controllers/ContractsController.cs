// OCSP.API/Controllers/ContractsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OCSP.Application.Services.Interfaces;
using OCSP.Application.DTOs.Contracts;
using OCSP.Domain.Enums;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _svc;

        public ContractsController(IContractService svc) => _svc = svc;

        // Helper: lấy userId hiện tại
        private Guid Me()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(v, out var g) ? g : Guid.Empty;
        }

        /// <summary>
        /// (UC26) Tạo hợp đồng từ một Proposal (homeowner).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ContractDetailDto>> Create([FromBody] CreateContractDto dto, CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            try
            {
                var result = await _svc.CreateFromProposalAsync(dto, uid, ct);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex)          { return NotFound(ex.Message); }
            catch (InvalidOperationException ex)  { return BadRequest(ex.Message); }
        }

        /// <summary>
        /// Lấy chi tiết 1 hợp đồng (homeowner/contractor của hợp đồng).
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ContractDetailDto>> GetById(Guid id, CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            try
            {
                var dto = await _svc.GetByIdAsync(id, uid, ct);
                return Ok(dto);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex)          { return NotFound(ex.Message); }
        }

        /// <summary>
        /// Liệt kê hợp đồng theo Project (homeowner/participant của project).
        /// </summary>
        [HttpGet("by-project")]
        public async Task<ActionResult<IEnumerable<ContractListItemDto>>> GetByProject([FromQuery] Guid projectId, CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            try
            {
                var list = await _svc.ListByProjectAsync(projectId, uid, ct);
                return Ok(list);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex)          { return NotFound(ex.Message); }
        }

        /// <summary>
        /// Liệt kê tất cả hợp đồng của tôi (homeowner hoặc contractor).
        /// </summary>
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ContractListItemDto>>> MyContracts(CancellationToken ct)
        {
            var uid = Me();
            if (uid == Guid.Empty) return Unauthorized();

            var list = await _svc.ListMyContractsAsync(uid, ct);
            return Ok(list);
        }

        /// <summary>
        /// (UC27) Cập nhật trạng thái hợp đồng: 
        /// Draft→PendingSignatures→Active→(Completed|Cancelled).
        /// </summary>
        [HttpPost("{id:guid}/status")]
public async Task<ActionResult<ContractDto>> UpdateStatus(Guid id, [FromBody] UpdateContractStatusDto body, CancellationToken ct)
{
    var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
    if (id != body.ContractId && body.ContractId != Guid.Empty) return BadRequest("Route id != body id");
    if (body.ContractId == Guid.Empty) body.ContractId = id;

    try
    {
        var dto = await _svc.UpdateStatusAsync(body, uid, ct);
        return Ok(dto);
    }
    catch (ArgumentException ex)              { return NotFound(ex.Message); }
    catch (UnauthorizedAccessException ex)    { return StatusCode(StatusCodes.Status403Forbidden, ex.Message); } // <—
    catch (InvalidOperationException ex)      { return BadRequest(ex.Message); }
}
    }
}
