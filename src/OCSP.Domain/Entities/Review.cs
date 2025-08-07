using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class Review : AuditableEntity
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }

        // Navigation properties
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public Guid ReviewerId { get; set; }
        public User? Reviewer { get; set; }

        public Guid RevieweeId { get; set; }
        public User? Reviewee { get; set; }
    }
}
