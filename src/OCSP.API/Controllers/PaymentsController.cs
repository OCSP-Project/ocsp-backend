using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Payments;
using OCSP.Application.Services.Interfaces;
using System;

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

        // Fallback return handler: process MoMo return when IPN is unreachable
        [AllowAnonymous]
        [HttpGet("momo/return")]
        public async Task<IActionResult> MomoReturn([FromQuery] string partnerCode, [FromQuery] string orderId, [FromQuery] string requestId,
            [FromQuery] long amount, [FromQuery] string orderInfo, [FromQuery] string orderType, [FromQuery] string transId,
            [FromQuery] int resultCode, [FromQuery] string message, [FromQuery] string payType, [FromQuery] string responseTime,
            [FromQuery] string extraData, [FromQuery] string signature, CancellationToken ct)
        {
            var decodedExtra = Uri.UnescapeDataString(extraData ?? string.Empty);
            var payload = new MomoWebhookDto
            {
                OrderId = orderId,
                RequestId = requestId,
                Amount = amount,
                OrderInfo = orderInfo,
                ResultCode = resultCode,
                Message = message,
                PayType = payType,
                ResponseTime = responseTime,
                ExtraData = decodedExtra,
                TransId = transId,
                Signature = signature
            };

            try
            {
                var raw = Request.QueryString.Value ?? string.Empty;
                await _payments.HandleMomoWebhookAsync(payload, raw, ct);
            }
            catch
            {
                // ignore; we still redirect to FE so user flow is not blocked
            }

            // Redirect to FE milestones tab
            return Redirect("http://localhost:3000/projects?tab=milestones");
        }

        private Guid Me()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(v, out var g) ? g : Guid.Empty;
        }
    }
}


