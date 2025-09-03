// OCSP.Application/DTOs/AI/AIChatRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace OCSP.Application.DTOs.AI
{
    public class AIChatRequestDto
    {
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? ProjectId { get; set; }

        public List<AIChatMessageDto> ChatHistory { get; set; } = new();

        public AIChatContextDto Context { get; set; } = new();
    }

    public class AIChatMessageDto
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class AIChatContextDto
    {
        public string UserRole { get; set; } = string.Empty;
        public string Location { get; set; } = "Da Nang";
        public Dictionary<string, object> Preferences { get; set; } = new();
    }

    public class AIResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public List<RelevantDocumentDto> RelevantDocs { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public int ProcessingTimeMs { get; set; }
    }

    public class RelevantDocumentDto
    {
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public double Score { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}