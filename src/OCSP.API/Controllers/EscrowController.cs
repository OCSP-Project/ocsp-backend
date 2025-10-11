// OCSP.API/Controllers/EscrowController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Escrow;
using OCSP.Application.DTOs.Payments;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EscrowController : ControllerBase
    {
        private readonly IEscrowService _svc;
        public EscrowController(IEscrowService svc) => _svc = svc;

        [HttpPost("setup")]
        public async Task<ActionResult<EscrowAccountDto>> Setup([FromBody] SetupEscrowDto dto, CancellationToken ct)
        {
            try
            {
                var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
                var res = await _svc.SetupEscrowAsync(dto, uid, ct);
                return Ok(res);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // Direct milestone funding endpoint disabled. Use wallet/MoMo flow via PaymentsController and webhook credit.

        [HttpPost("milestones/{milestoneId:guid}/approve")]
        public async Task<ActionResult<MilestonePayoutResultDto>> Approve(Guid milestoneId, CancellationToken ct)
        {
            try
            {
                var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
                var res = await _svc.ApproveMilestoneAsync(milestoneId, uid, ct);
                return Ok(res);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("milestones/release")]
        public async Task<ActionResult<MilestonePayoutResultDto>> Release([FromBody] ReleaseMilestoneDto dto, CancellationToken ct)
        {
            try
            {
                var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
                var res = await _svc.ReleaseMilestoneAsync(dto, uid, ct);
                return Ok(res);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("by-contract/{contractId:guid}")]
        public async Task<ActionResult<EscrowAccountDto>> GetByContract(Guid contractId, CancellationToken ct)
        {
            try
            {
                var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
                var res = await _svc.GetEscrowByContractAsync(contractId, uid, ct);
                return Ok(res);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<IEnumerable<PaymentTransactionDto>>> ListTxns([FromQuery] Guid contractId, CancellationToken ct)
        {
            try
            {
                var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
                var res = await _svc.ListTransactionsByContractAsync(contractId, uid, ct);
                return Ok(res);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        private Guid Me()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(v, out var g) ? g : Guid.Empty;
        }
    }
}
