using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Payments;
using OCSP.Application.Services.Interfaces;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/momo/webhook")]
    public class MomoWebhookController : ControllerBase
    {
        private readonly IPaymentService _payments;
        public MomoWebhookController(IPaymentService payments) { _payments = payments; }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MomoWebhookDto payload, CancellationToken ct)
        {
            // Truyền lại object serialize làm rawBody, tránh lỗi NotSupportedException
            var raw = System.Text.Json.JsonSerializer.Serialize(payload);
            await _payments.HandleMomoWebhookAsync(payload, raw, ct);
            return Ok(new { resultCode = 0, message = "OK" });
        }
    }
}


