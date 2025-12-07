using System.ComponentModel.DataAnnotations;

namespace OCSP.Application.DTOs.News
{
    public class CreateNewsDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;
        public DateTime? DateStart { get; set; }
        public string[] ImageLinks { get; set; } = Array.Empty<string>();

        [Required]
        public string ContentNews { get; set; } = string.Empty;

        public string? OriginalLink { get; set; }
        public DateTime? ScheduledPublishAt { get; set; }
        public bool PublishImmediately { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public string? Category { get; set; }
        public string? Tags { get; set; }
    }
}
