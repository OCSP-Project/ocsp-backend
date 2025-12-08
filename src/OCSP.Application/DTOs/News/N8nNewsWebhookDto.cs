using System.Text.Json.Serialization;

namespace OCSP.Application.DTOs.News
{
    /// <summary>
    /// DTO nhận data từ n8n webhook
    /// </summary>
    public class N8nNewsWebhookDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("tác_giả")]
        public string? TácGiả { get; set; } // Author từ n8n

        [JsonPropertyName("date_start")]
        public string? DateStart { get; set; }

        [JsonPropertyName("image_link")]
        public string[] ImageLink { get; set; } = Array.Empty<string>();

        [JsonPropertyName("content_news")]
        public string ContentNews { get; set; } = string.Empty;

        [JsonPropertyName("original_link")]
        public string? OriginalLink { get; set; }

        [JsonPropertyName("n8nWorkflowId")]
        public string? N8nWorkflowId { get; set; }
    }
}
