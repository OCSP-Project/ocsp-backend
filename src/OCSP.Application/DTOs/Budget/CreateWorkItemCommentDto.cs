using System.ComponentModel.DataAnnotations;

namespace OCSP.Application.DTOs.Budget
{
    public class CreateWorkItemCommentDto
    {
        [Required]
        public Guid WorkItemId { get; set; }

        [Required]
        [MinLength(1)]
        public string Content { get; set; } = string.Empty;

        public Guid? ParentCommentId { get; set; }

        // List of user IDs mentioned in the comment (from @mentions)
        public List<Guid> MentionedUserIds { get; set; } = new();

        public string? Attachments { get; set; }
    }
}
