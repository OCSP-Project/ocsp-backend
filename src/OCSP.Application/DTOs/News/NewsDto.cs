namespace OCSP.Application.DTOs.News
{
    public class NewsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime? DateStart { get; set; }
        public string[] ImageLinks { get; set; } = Array.Empty<string>();
        public string ContentNews { get; set; } = string.Empty;
        public string? OriginalLink { get; set; }

        public DateTime? ScheduledPublishAt { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }

        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public string? Category { get; set; }
        public string? Tags { get; set; }

        public string? N8nWorkflowId { get; set; }
        public DateTime? CrawledAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
