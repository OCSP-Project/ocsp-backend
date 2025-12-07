namespace OCSP.Application.DTOs.News
{
    /// <summary>
    /// DTO nhận data từ n8n webhook
    /// </summary>
    public class N8nNewsWebhookDto
    {
        public string Title { get; set; } = string.Empty;
        public string? TácGiả { get; set; } // Author từ n8n
        public DateTime? DateStart { get; set; }
        public string[] ImageLink { get; set; } = Array.Empty<string>();
        public string ContentNews { get; set; } = string.Empty;
        public string? OriginalLink { get; set; }
        public string? N8nWorkflowId { get; set; }
    }
}
