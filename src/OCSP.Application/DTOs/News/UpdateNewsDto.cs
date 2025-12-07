namespace OCSP.Application.DTOs.News
{
    public class UpdateNewsDto
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public DateTime? DateStart { get; set; }
        public string[]? ImageLinks { get; set; }
        public string? ContentNews { get; set; }
        public string? OriginalLink { get; set; }
        public bool? IsFeatured { get; set; }
        public string? Category { get; set; }
        public string? Tags { get; set; }
    }
}
