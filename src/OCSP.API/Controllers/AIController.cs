// OCSP.API/Controllers/AIController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OCSP.Application.Services.Interfaces;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ILogger<AIController> _logger;

        public AIController(IAIService aiService, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost("chat")]
        [ProducesResponseType(typeof(AIResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendMessage([FromBody] AIChatRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _aiService.GetAIResponseAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI chat request for user {UserId}", request.UserId);
                return StatusCode(500, new { message = "AI service temporarily unavailable" });
            }
        }

        [HttpGet("health")]
        public async Task<IActionResult> CheckHealth()
        {
            var isHealthy = await _aiService.IsServiceHealthyAsync();
            return Ok(new { healthy = isHealthy });
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetChatHistory(string userId, [FromQuery] int limit = 50)
        {
            var history = await _aiService.GetChatHistoryAsync(userId, limit);
            return Ok(history);
        }
    }
}