using OCSP.Domain.Entities;
using OCSP.Application.DTOs.Contractor;

namespace OCSP.Application.DTOs.Chat
{
    public class ChatMessageResult
    {
        public ChatMessage Message { get; set; } = null!;
        public CommunicationWarningDto? Warning { get; set; }
    }

    public class SendMessageResponse
    {
        public MessageDto Message { get; set; } = null!;
        public CommunicationWarningDto? Warning { get; set; }
        public bool RequiresAcknowledgment { get; set; }
    }

    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ConversationCreatedDto
    {
        public Guid ConversationId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid[] ParticipantIds { get; set; } = Array.Empty<Guid>();
    }

    public class ConversationDto
    {
        public Guid Id { get; set; }
        public Guid? ProjectId { get; set; }
        public ParticipantDto[] Participants { get; set; } = Array.Empty<ParticipantDto>();
        public MessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ParticipantDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Role { get; set; }
    }

    public class StartChatRequest
    {
        public Guid? ProjectId { get; set; } // Make optional
        public Guid[] UserIds { get; set; } = Array.Empty<Guid>();
        public string ChatType { get; set; } = "consultation"; // consultation | project
    }

    public class SendMessageRequest
    {
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class AcknowledgeWarningRequest
    {
        public Guid UserId { get; set; }
        public string WarningType { get; set; } = string.Empty;
        public DateTime AcknowledgedAt { get; set; } = DateTime.UtcNow;
    }
}