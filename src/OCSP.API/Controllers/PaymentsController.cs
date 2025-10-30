using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Payments;
using OCSP.Application.Services.Interfaces;
using System;
using System.Text.Json;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _payments;
        public PaymentsController(IPaymentService payments) { _payments = payments; }

        [HttpPost("momo/create")]
        public async Task<ActionResult<MomoCreatePaymentResultDto>> CreateMomo([FromBody] MomoCreatePaymentDto dto, CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            var res = await _payments.CreateMomoPaymentAsync(dto, uid, ct);
            return Ok(res);
        }

        [HttpPost("manual-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> ManualWebhook([FromBody] MomoWebhookDto payload, CancellationToken ct)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(payload.OrderId))
                return BadRequest(new { error = "OrderId is required" });
            
            if (string.IsNullOrWhiteSpace(payload.RequestId))
                return BadRequest(new { error = "RequestId is required" });
            
            if (string.IsNullOrWhiteSpace(payload.ExtraData))
                return BadRequest(new { error = "ExtraData is required" });
            
            var raw = JsonSerializer.Serialize(payload);
            try {
                await _payments.HandleMomoWebhookAsync(payload, raw, ct);
                return Ok(new { result = "ok" });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpGet("wallet/balance")]
        public async Task<ActionResult<object>> GetWalletBalance(CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            var balance = await _payments.GetWalletBalanceAsync(uid, ct);
            return Ok(new { balance });
        }

        [HttpGet("commission/status")]
        public async Task<ActionResult<object>> GetCommissionStatus([FromQuery] Guid contractId, CancellationToken ct)
        {
            var uid = Me(); if (uid == Guid.Empty) return Unauthorized();
            if (contractId == Guid.Empty) return BadRequest("contractId is required");
            var paid = await _payments.IsCommissionPaidAsync(uid, contractId, ct);
            return Ok(new { paid });
        }

        private Guid Me()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(v, out var g) ? g : Guid.Empty;
        }
    }
}


