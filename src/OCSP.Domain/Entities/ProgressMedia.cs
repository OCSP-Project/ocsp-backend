using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class ProgressMedia : AuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        public Guid? TaskId { get; set; } // optional, cho UC-32 sau này
        public Guid? ProgressUpdateId { get; set; } // optional, cho UC-33 sau này

        public string Url { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;

        public Guid CreatorId { get; set; }
        public User? Creator { get; set; }
    }
}
