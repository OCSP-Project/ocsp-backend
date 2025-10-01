using Microsoft.AspNetCore.Mvc;
using OCSP.Application.Services.Interfaces;
using OCSP.Application.DTOs.Chat;
using OCSP.Domain.Entities;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("start")]
        [ProducesResponseType(typeof(ConversationCreatedDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartConversation([FromBody] StartChatRequest request)
        {
            if (request is null || request.UserIds is null || request.UserIds.Length == 0)
                return BadRequest("userIds là bắt buộc.");

            // projectId is now optional for consultation chats
            var conversation = await _chatService.StartConversationAsync(request.ProjectId, request.UserIds);

            var result = new ConversationCreatedDto
            {
                ConversationId = conversation.Id,
                ProjectId = conversation.ProjectId ?? Guid.Empty, // Handle null
                ParticipantIds = conversation.Participants?.Select(p => p.UserId).ToArray() ?? Array.Empty<Guid>()
            };

            return Ok(result);
        }

        public class StartChatRequest
        {
            public Guid? ProjectId { get; set; } // Make nullable
            public Guid[] UserIds { get; set; } = Array.Empty<Guid>();
        }

        [HttpGet("{conversationId:guid}/messages")]
        [ProducesResponseType(typeof(IEnumerable<MessageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMessages([FromRoute] Guid conversationId)
        {
            if (conversationId == Guid.Empty) return BadRequest("conversationId không hợp lệ.");

            var messages = await _chatService.GetMessagesAsync(conversationId);

            var result = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            });

            return Ok(result);
        }

        [HttpPost("{conversationId:guid}/send")]
        [ProducesResponseType(typeof(SendMessageResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendMessage([FromRoute] Guid conversationId, [FromBody] SendMessageRequest request)
        {
            if (conversationId == Guid.Empty) return BadRequest("conversationId không hợp lệ.");
            if (request is null || request.SenderId == Guid.Empty || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("senderId và content là bắt buộc.");

            var result = await _chatService.SendMessageAsync(conversationId, request.SenderId, request.Content);

            var response = new SendMessageResponse
            {
                Message = new MessageDto
                {
                    Id = result.Message.Id,
                    ConversationId = result.Message.ConversationId,
                    SenderId = result.Message.SenderId,
                    Content = result.Message.Content,
                    CreatedAt = result.Message.CreatedAt
                },
                Warning = result.Warning,
                RequiresAcknowledgment = result.Warning?.RequiresAcknowledgment ?? false
            };

            return Ok(response);
        }

        // New endpoint for acknowledging warnings
        [HttpPost("acknowledge-warning")]
        public IActionResult AcknowledgeWarning([FromBody] AcknowledgeWarningRequest request)
        {
            // Remove async since no await operations
            return Ok(new { Message = "Cảnh báo đã được ghi nhận" });
        }
    }
}