using Microsoft.AspNetCore.Mvc;
using OCSP.Application.Services.Interfaces;

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

        // ─────────────────────────────────────────────────────────────
        // 1) Bắt đầu conversation (Homeowner ↔ Contractor hoặc Group)
        // ─────────────────────────────────────────────────────────────
        [HttpPost("start")]
        [ProducesResponseType(typeof(ConversationCreatedDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartConversation([FromBody] StartChatRequest request)
        {
            if (request is null || request.ProjectId == Guid.Empty || request.UserIds is null || request.UserIds.Length == 0)
                return BadRequest("projectId và userIds là bắt buộc.");

            var conversation = await _chatService.StartConversationAsync(request.ProjectId, request.UserIds);

            var result = new ConversationCreatedDto
            {
                ConversationId = conversation.Id,
                ProjectId      = conversation.ProjectId,
                ParticipantIds = conversation.Participants?.Select(p => p.UserId).ToArray() ?? Array.Empty<Guid>()
            };

            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────
        // 2) Lấy danh sách tin nhắn
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{conversationId:guid}/messages")]
        [ProducesResponseType(typeof(IEnumerable<MessageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMessages([FromRoute] Guid conversationId)
        {
            if (conversationId == Guid.Empty) return BadRequest("conversationId không hợp lệ.");

            var messages = await _chatService.GetMessagesAsync(conversationId);

            var result = messages.Select(m => new MessageDto
            {
                Id             = m.Id,
                ConversationId = m.ConversationId,
                SenderId       = m.SenderId,
                Content        = m.Content,
                CreatedAt      = m.CreatedAt   // 🔹 Dùng CreatedAt thay cho SentAt
            });

            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────
        // 3) Gửi tin nhắn
        // ─────────────────────────────────────────────────────────────
        [HttpPost("{conversationId:guid}/send")]
        [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendMessage([FromRoute] Guid conversationId, [FromBody] SendMessageRequest request)
        {
            if (conversationId == Guid.Empty) return BadRequest("conversationId không hợp lệ.");
            if (request is null || request.SenderId == Guid.Empty || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("senderId và content là bắt buộc.");

            var message = await _chatService.SendMessageAsync(conversationId, request.SenderId, request.Content);

            var result = new MessageDto
            {
                Id             = message.Id,
                ConversationId = message.ConversationId,
                SenderId       = message.SenderId,
                Content        = message.Content,
                CreatedAt      = message.CreatedAt   // 🔹 Dùng CreatedAt thay cho SentAt
            };

            return Ok(result);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Request/Response DTOs
    // ─────────────────────────────────────────────────────────────
    public class StartChatRequest
    {
        public Guid   ProjectId { get; set; }
        public Guid[] UserIds   { get; set; } = Array.Empty<Guid>();
    }

    public class SendMessageRequest
    {
        public Guid   SenderId { get; set; }
        public string Content  { get; set; } = string.Empty;
    }

    public class ConversationCreatedDto
    {
        public Guid   ConversationId { get; set; }
        public Guid   ProjectId      { get; set; }
        public Guid[] ParticipantIds { get; set; } = Array.Empty<Guid>();
    }

    public class MessageDto
    {
        public Guid   Id             { get; set; }
        public Guid   ConversationId { get; set; }
        public Guid   SenderId       { get; set; }
        public string Content        { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }   // 🔹 Đổi từ SentAt sang CreatedAt
    }
}
