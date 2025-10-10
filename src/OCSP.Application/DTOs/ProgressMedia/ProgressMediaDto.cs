namespace OCSP.Application.DTOs.ProgressMedia
{
    public class ProgressMediaDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? TaskId { get; set; }
        public Guid? ProgressUpdateId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UploadProgressMediaDto
    {
        public string Caption { get; set; } = string.Empty;
        public Guid? TaskId { get; set; }
        public Guid? ProgressUpdateId { get; set; }
    }

    public class ProgressMediaListDto
    {
        public List<ProgressMediaDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
