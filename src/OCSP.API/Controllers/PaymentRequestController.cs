using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Budget;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/payment-requests")]
    [Authorize]
    public class PaymentRequestController : ControllerBase
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly ILogger<PaymentRequestController> _logger;

        public PaymentRequestController(
            IPaymentRequestService paymentRequestService,
            ILogger<PaymentRequestController> logger)
        {
            _paymentRequestService = paymentRequestService;
            _logger = logger;
        }

        // GET api/payment-requests/project/{projectId}
        [HttpGet("project/{projectId:guid}")]
        [ProducesResponseType(typeof(List<PaymentRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<PaymentRequestDto>>> GetAllByProject(
            [FromRoute] Guid projectId,
            CancellationToken ct)
        {
            try
            {
                var paymentRequests = await _paymentRequestService.GetAllByProjectAsync(projectId, ct);
                return Ok(paymentRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment requests for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/payment-requests/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PaymentRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PaymentRequestDto>> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var paymentRequest = await _paymentRequestService.GetByIdAsync(id, ct);
                if (paymentRequest == null)
                    return NotFound(new { message = "Payment request not found" });

                return Ok(paymentRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment request {PaymentRequestId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/payment-requests
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)] // 50MB limit
        [ProducesResponseType(typeof(PaymentRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PaymentRequestDto>> Create(
            [FromForm] Guid projectId,
            [FromForm] decimal amount,
            [FromForm] string description,
            [FromForm] string? relatedWorkItemIds,
            [FromForm] IFormFileCollection? supportingDocuments,
            [FromForm] string? notes,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                // Parse related work item IDs
                List<Guid>? workItemIds = null;
                if (!string.IsNullOrEmpty(relatedWorkItemIds))
                {
                    try
                    {
                        workItemIds = relatedWorkItemIds
                            .Split(',')
                            .Select(id => Guid.Parse(id.Trim()))
                            .ToList();
                    }
                    catch
                    {
                        return BadRequest(new { message = "Invalid work item IDs format" });
                    }
                }

                var dto = new CreatePaymentRequestDto
                {
                    ProjectId = projectId,
                    Amount = amount,
                    Description = description,
                    RelatedWorkItemIds = workItemIds,
                    SupportingDocuments = supportingDocuments?.ToList(),
                    Notes = notes
                };

                var paymentRequest = await _paymentRequestService.CreateAsync(dto, currentUserId, ct);
                return CreatedAtAction(nameof(GetById), new { id = paymentRequest.Id }, paymentRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request creating payment request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment request");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE api/payment-requests/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                await _paymentRequestService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request deleting payment request");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation deleting payment request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment request {PaymentRequestId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/payment-requests/{id}/approve
        [HttpPost("{id:guid}/approve")]
        [ProducesResponseType(typeof(PaymentRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentRequestDto>> Approve(
            [FromRoute] Guid id,
            [FromBody] ApprovePaymentRequestDto dto,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var paymentRequest = await _paymentRequestService.ApproveAsync(id, currentUserId, dto, ct);
                return Ok(paymentRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request approving payment request");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation approving payment request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving payment request {PaymentRequestId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/payment-requests/{id}/reject
        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(typeof(PaymentRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentRequestDto>> Reject(
            [FromRoute] Guid id,
            [FromBody] RejectPaymentRequestDto dto,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var paymentRequest = await _paymentRequestService.RejectAsync(id, currentUserId, dto, ct);
                return Ok(paymentRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request rejecting payment request");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation rejecting payment request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting payment request {PaymentRequestId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/payment-requests/{id}/mark-paid
        [HttpPost("{id:guid}/mark-paid")]
        [ProducesResponseType(typeof(PaymentRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentRequestDto>> MarkAsPaid(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var paymentRequest = await _paymentRequestService.MarkAsPaidAsync(id, currentUserId, ct);
                return Ok(paymentRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request marking payment request as paid");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation marking payment request as paid");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment request as paid {PaymentRequestId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/payment-requests/{id}/cancel
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(PaymentRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentRequestDto>> Cancel(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty)
                    return Unauthorized(new { message = "User not authenticated" });

                var paymentRequest = await _paymentRequestService.CancelAsync(id, currentUserId, ct);
                return Ok(paymentRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request cancelling payment request");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation cancelling payment request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment request {PaymentRequestId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/payment-requests/project/{projectId}/statistics
        [HttpGet("project/{projectId:guid}/statistics")]
        [ProducesResponseType(typeof(PaymentStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PaymentStatisticsDto>> GetStatistics(
            [FromRoute] Guid projectId,
            CancellationToken ct)
        {
            try
            {
                var statistics = await _paymentRequestService.GetStatisticsAsync(projectId, ct);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment statistics for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/payment-requests/{id}/download-document
        [HttpGet("{id:guid}/download-document")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<IActionResult> DownloadDocument([FromRoute] Guid id, CancellationToken ct)
        {
            try
            {
                var fileBytes = await _paymentRequestService.DownloadDocumentAsync(id, ct);
                return File(fileBytes, "application/octet-stream", $"PaymentRequest_{id}_Document.pdf");
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, new { message = "Document download not yet implemented" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request downloading payment request document");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document for payment request {PaymentRequestId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
            return Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }
    }
}
