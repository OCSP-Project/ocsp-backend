using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class News : AuditableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime? DateStart { get; set; } // Date from article
        public string[] ImageLinks { get; set; } = Array.Empty<string>(); // Multiple images
        public string ContentNews { get; set; } = string.Empty; // Main content
        public string? OriginalLink { get; set; } // Link to source article

        // Schedule publishing
        public DateTime? ScheduledPublishAt { get; set; }
        public bool IsPublished { get; set; } = false;
        public DateTime? PublishedAt { get; set; }

        // Admin control
        public bool IsFeatured { get; set; } = false; // Pin to top
        public int ViewCount { get; set; } = 0;
        public string? Category { get; set; } // e.g., "Nội thất", "Bất động sản"
        public string? Tags { get; set; } // Comma-separated tags

        // N8N source tracking
        public string? N8nWorkflowId { get; set; }
        public DateTime? CrawledAt { get; set; }
    }
}
