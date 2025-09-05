// OCSP.API/Controllers/QuotesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Quotes;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuotesController : ControllerBase
    {
        private readonly IQuoteService _quoteService;
        public QuotesController(IQuoteService quoteService) => _quoteService = quoteService;

        private Guid Me()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(v, out var g) ? g : Guid.Empty;
        }

        // ─────────────────────────────────────────────────────────────
        // Create quote (Homeowner)
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult<QuoteRequestDto>> Create([FromBody] CreateQuoteRequestDto dto, CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            try
            {
                var result = await _quoteService.CreateAsync(dto, uid, ct);
                return CreatedAtAction(nameof(GetByProject), new { projectId = result.ProjectId }, result);
            }
            catch (ArgumentException ex)            { return BadRequest(ex.Message); }
            catch (UnauthorizedAccessException ex)  { return Forbid(ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────
        // Add invitee to quote (Homeowner)
        // ─────────────────────────────────────────────────────────────
        [HttpPost("{id:guid}/invite")]
        public async Task<IActionResult> AddInvitee(Guid id, [FromBody] AddInviteeDto dto, CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            try
            {
                await _quoteService.AddInviteeAsync(id, dto.ContractorUserId, uid, ct);
                return NoContent();
            }
            catch (ArgumentException ex)            { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex)  { return Forbid(ex.Message); }
            catch (InvalidOperationException ex)    { return BadRequest(ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────
        // Send quote (Homeowner) : Draft -> Sent
        // ─────────────────────────────────────────────────────────────
        [HttpPost("{id:guid}/send")]
        public async Task<IActionResult> Send(Guid id, CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            try
            {
                await _quoteService.SendAsync(id, uid, ct);
                return NoContent();
            }
            catch (ArgumentException ex)            { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex)  { return Forbid(ex.Message); }
            catch (InvalidOperationException ex)    { return BadRequest(ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────
        // Send quote to all contractors (Homeowner)
        // ─────────────────────────────────────────────────────────────
        [HttpPost("{id:guid}/send-to-all")]
        public async Task<IActionResult> SendToAllContractors(Guid id, CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            try
            {
                await _quoteService.SendToAllContractorsAsync(id, uid, ct);
                return NoContent();
            }
            catch (ArgumentException ex)            { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex)  { return Forbid(ex.Message); }
            catch (InvalidOperationException ex)    { return BadRequest(ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────
        // Send quote to specific contractor (Homeowner)
        // ─────────────────────────────────────────────────────────────
        [HttpPost("{id:guid}/send-to-contractor")]
        public async Task<IActionResult> SendToContractor(Guid id, [FromBody] AddInviteeDto dto, CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            try
            {
                await _quoteService.SendToContractorAsync(id, dto.ContractorUserId, uid, ct);
                return NoContent();
            }
            catch (ArgumentException ex)            { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex)  { return Forbid(ex.Message); }
            catch (InvalidOperationException ex)    { return BadRequest(ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────
        // List quotes by project (Homeowner / participants)
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuoteRequestDto>>> GetByProject([FromQuery] Guid projectId, CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            try
            {
                var list = await _quoteService.ListByProjectAsync(projectId, uid, ct);
                return Ok(list);
            }
            catch (ArgumentException ex)            { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex)  { return Forbid(ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────
        // Get basic quote by id (Homeowner or invited Contractor)
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<QuoteRequestDto>> GetById(Guid id, CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            try
            {
                var dto = await _quoteService.GetByIdAsync(id, uid, ct);
                return Ok(dto);
            }
            catch (ArgumentException ex)            { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex)  { return Forbid(ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────
        // Contractor: list quotes inviting me (basic)
        // ─────────────────────────────────────────────────────────────
        [HttpGet("my-invites")]
        public async Task<ActionResult<IEnumerable<QuoteRequestDto>>> MyInvites(CancellationToken ct)
        {
            var contractorUserId = Me(); if (contractorUserId == Guid.Empty) return Unauthorized();
            var list = await _quoteService.ListMyInvitesAsync(contractorUserId, ct);
            return Ok(list);
        }

        // ─────────────────────────────────────────────────────────────
        // Contractor: list quotes inviting me (detailed)
        // ─────────────────────────────────────────────────────────────
        [HttpGet("my-invites/detailed")]
        public async Task<ActionResult<IEnumerable<QuoteRequestDetailDto>>> MyInvitesDetailed(CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            var list = await _quoteService.ListMyInvitesDetailedAsync(uid, ct);
            return Ok(list);
        }

        // ─────────────────────────────────────────────────────────────
        // Get quote detail for current user (Homeowner or invited Contractor)
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id:guid}/detail")]
        public async Task<ActionResult<QuoteRequestDetailDto>> GetDetail(Guid id, CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            try
            {
                var dto = await _quoteService.GetDetailForUserAsync(id, uid, ct);
                return Ok(dto);
            }
            catch (ArgumentException ex)            { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex)  { return Forbid(ex.Message); }
        }
    }
}
