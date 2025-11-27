using System.ComponentModel.DataAnnotations;

namespace OCSP.Application.DTOs.Budget
{
    public class UpdateWorkItemCommentDto
    {
        [Required]
        [MinLength(1)]
        public string Content { get; set; } = string.Empty;

        // Updated list of mentioned user IDs
        public List<Guid> MentionedUserIds { get; set; } = new();

        public string? Attachments { get; set; }
    }
}
