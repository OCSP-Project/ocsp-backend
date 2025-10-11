// OCSP.API/Controllers/MilestonesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Milestones;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MilestonesController : ControllerBase
    {
        private readonly IContractMilestoneService _svc;
        public MilestonesController(IContractMilestoneService svc) => _svc = svc;

        [HttpPost("{contractId}")]
public async Task<ActionResult<MilestoneDto>> Create(Guid contractId, [FromBody] CreateMilestoneDto dto, CancellationToken ct)
{
    try
    {
        var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
        var result = await _svc.CreateAsync(contractId, dto, uid, ct);
        return CreatedAtAction(nameof(ListByContract), new { contractId = result.ContractId }, result);
    }
    catch (ArgumentException ex) { return NotFound(ex.Message); }
    catch (UnauthorizedAccessException) { return Forbid(); }
    catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
}



        [HttpGet("by-contract/{contractId:guid}")]
        public async Task<ActionResult<IEnumerable<MilestoneDto>>> ListByContract(Guid contractId, CancellationToken ct)
        {
            try
            {
                var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
                var list = await _svc.ListByContractAsync(contractId, uid, ct);
                return Ok(list);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        }

        [HttpPost("submit")]
        public async Task<ActionResult<MilestoneDto>> Submit([FromBody] SubmitMilestoneDto dto, CancellationToken ct)
        {
            try
            {
                var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
                var result = await _svc.SubmitAsync(dto, uid, ct);
                return Ok(result);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("bulk")]
public async Task<ActionResult<IEnumerable<MilestoneDto>>> BulkCreate([FromBody] BulkCreateMilestonesDto dto, CancellationToken ct)
{
    try
    {
        var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
        var result = await _svc.BulkCreateAsync(dto, uid, ct);
        return Ok(result);
    }
    catch (ArgumentException ex) { return BadRequest(ex.Message); }
    catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
}


        [HttpPut("{milestoneId:guid}")]
        public async Task<ActionResult<MilestoneDto>> Update(Guid milestoneId, [FromBody] UpdateMilestoneDto dto, CancellationToken ct)
        {
            try
            {
                var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
                var result = await _svc.UpdateAsync(milestoneId, dto, uid, ct);
                return Ok(result);
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("{milestoneId:guid}")]
        public async Task<IActionResult> Delete(Guid milestoneId, CancellationToken ct)
        {
            try
            {
                var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
                await _svc.DeleteAsync(milestoneId, uid, ct);
                return NoContent();
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        private Guid Me()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(v, out var g) ? g : Guid.Empty;
        }
    }
}
