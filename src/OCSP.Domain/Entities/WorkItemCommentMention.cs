using OCSP.Domain.Common;

namespace OCSP.Domain.Entities
{
    public class WorkItemCommentMention : BaseEntity
    {
        public Guid CommentId { get; set; }
        public WorkItemComment? Comment { get; set; }

        public Guid MentionedUserId { get; set; }
        public User? MentionedUser { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
