using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Proposals;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProposalsController : ControllerBase
    {
        private readonly IProposalService _svc;
        public ProposalsController(IProposalService svc) => _svc = svc;

        [HttpPost] // contractor tạo Draft
        public async Task<ActionResult<ProposalDto>> Create([FromBody] CreateProposalDto dto, CancellationToken ct)
        {
            var uid = GetUserId(); if (uid == Guid.Empty) return Unauthorized();
            try { var res = await _svc.CreateAsync(dto, uid, ct); return CreatedAtAction(nameof(GetByQuote), new { quoteId = res.QuoteRequestId }, res); }
            catch (ArgumentException ex)           { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex)   { return BadRequest(ex.Message); }
        }

        [HttpGet("{id:guid}")] // contractor xem bản nháp của mình
        public async Task<ActionResult<ProposalDto>> GetMy(Guid id, CancellationToken ct)
        {
            var uid = GetUserId(); if (uid == Guid.Empty) return Unauthorized();
            try { var res = await _svc.GetMyByIdAsync(id, uid, ct); return Ok(res); }
            catch (ArgumentException ex)           { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpGet("by-quote/{quoteId:guid}/mine")] // contractor lấy bản nháp của mình theo quote
        public async Task<ActionResult<ProposalDto>> GetMyByQuote(Guid quoteId, CancellationToken ct)
        {
            var uid = GetUserId(); if (uid == Guid.Empty) return Unauthorized();
            var res = await _svc.GetMyByQuoteAsync(quoteId, uid, ct);
            if (res == null) return NotFound();
            return Ok(res);
        }

        [HttpPut("{id:guid}")] // contractor cập nhật draft
        public async Task<ActionResult<ProposalDto>> Update(Guid id, [FromBody] UpdateProposalDto dto, CancellationToken ct)
        {
            var uid = GetUserId(); if (uid == Guid.Empty) return Unauthorized();
            try { var res = await _svc.UpdateDraftAsync(id, dto, uid, ct); return Ok(res); }
            catch (ArgumentException ex)           { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex)   { return BadRequest(ex.Message); }
        }

        [HttpPost("{id:guid}/submit")] // contractor submit
        public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
        {
            var uid = GetUserId(); if (uid == Guid.Empty) return Unauthorized();
            try { await _svc.SubmitAsync(id, uid, ct); return NoContent(); }
            catch (ArgumentException ex)           { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex)   { return BadRequest(ex.Message); }
        }

        [HttpGet("by-quote/{quoteId:guid}")] // homeowner xem
        public async Task<ActionResult<IEnumerable<ProposalDto>>> GetByQuote(Guid quoteId, CancellationToken ct)
        {
            var uid = GetUserId(); if (uid == Guid.Empty) return Unauthorized();
            try { var list = await _svc.ListByQuoteAsync(quoteId, uid, ct); return Ok(list); }
            catch (ArgumentException ex)           { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpPost("{id:guid}/accept")] // homeowner accept 1 proposal
        public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
        {
            var uid = GetUserId(); if (uid == Guid.Empty) return Unauthorized();
            try { await _svc.AcceptAsync(id, uid, ct); return NoContent(); }
            catch (ArgumentException ex)           { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex)   { return BadRequest(ex.Message); }
        }

        private Guid GetUserId()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(v, out var g) ? g : Guid.Empty;
        }
    }
}
